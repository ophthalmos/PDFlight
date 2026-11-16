using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PDFLight.Classes;

/// <summary>Der Gemini-API-Schlüssel des Benutzers (KI-Zusammenfassung, seit 14.11.2026): jeder trägt seinen eigenen ein, im Programm steckt
/// keiner (Entscheidung des Autors). Gespeichert in einer eigenen Datei neben settings.json, mit Windows (DPAPI) für das Benutzerkonto
/// verschlüsselt wie die Unterschriften – nicht in settings.json, die mehrere Instanzen wechselseitig überschreiben und die ohne
/// Verschlüsselung in Sicherungen landet.</summary>
internal static class GeminiKeyStore
{
    public static string FilePath => Path.Combine(Path.GetDirectoryName(AppSettings.SettingsPath)!, "gemini.dat"); // der Einstellungsordner hat immer einen Elternordner
    private static readonly byte[] Entropy = "PDFlight.Gemini"u8.ToArray();

    /// <summary>Der gespeicherte Schlüssel; leer, wenn keiner da ist oder die Datei nicht zu lesen ist (anderes Konto, beschädigt).</summary>
    public static string Load()
    {
        try
        {
            if (!File.Exists(FilePath)) { return string.Empty; }
            return DataProtection.Unprotect(File.ReadAllBytes(FilePath), Entropy) is { } plain ? Encoding.UTF8.GetString(plain) : string.Empty;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return string.Empty; }
    }

    public static bool HasKey => Load().Length > 0;

    /// <summary>Speichert den Schlüssel; leer löscht die Datei.</summary>
    public static void Save(string key)
    {
        key = key.Trim();
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (key.Length == 0) { File.Delete(FilePath); return; }
        var protectedBytes = DataProtection.Protect(Encoding.UTF8.GetBytes(key), Entropy) ?? throw new IOException("Der API-Schlüssel konnte nicht verschlüsselt werden.");
        var temp = FilePath + ".tmp";
        File.WriteAllBytes(temp, protectedBytes);
        File.Move(temp, FilePath, overwrite: true);
    }
}

/// <summary>Wovon ein Fehler handelt – entscheidet, ob ein anderes Modell helfen kann. InvalidRequest: das Modell lehnt die Denkstufe ab.</summary>
internal enum GeminiFailure { Other, ModelUnavailable, Overloaded, Quota, Timeout, InvalidRequest }

/// <summary>Fehler einer Gemini-Anfrage mit einer Meldung für den Benutzer.</summary>
internal sealed class GeminiException(string message, Exception? inner = null, GeminiFailure kind = GeminiFailure.Other) : Exception(message, inner)
{
    public GeminiFailure Kind { get; } = kind;

    /// <summary>Ein anderes Modell könnte antworten (dieses ist nicht verfügbar, überlastet, hängt oder sein Kontingent ist erschöpft).</summary>
    public bool OtherModelMayHelp => Kind is GeminiFailure.ModelUnavailable or GeminiFailure.Overloaded or GeminiFailure.Quota or GeminiFailure.Timeout;
}

/// <summary>Anfragen an die Gemini API (REST, ohne Googles Paket). Der Schlüssel steht im Kopf der Anfrage (x-goog-api-key), nie in der
/// Adresse – sonst landete er in Protokollen. Gesendet wird nur Text, nie die PDF-Datei.</summary>
internal static partial class GeminiClient
{
    /// <summary>Googles Aliasname für das jeweils aktuelle Flash-Modell – feste Namen veralten (Fehlerbericht 14.11.2026: gemini-2.5-flash
    /// gab es für ein neues Projekt nicht mehr, Fehler 404; laut Google-Doku „Models“ sind die 2.5-Modelle bisherigen Nutzern vorbehalten).</summary>
    public const string DefaultModel = "gemini-flash-latest";

    /// <summary>Frühere Vorgaben, die beim Laden der Einstellungen einmalig auf „automatisch“ (leer) umgestellt werden – auch das zwischenzeitliche
    /// gemini-flash-latest: Flash denkt vor jeder Antwort, Flash-Lite ist für Zusammenfassungen viel schneller (s. <see cref="ChoosePreferred"/>).</summary>
    public static readonly string[] RetiredDefaults = ["gemini-2.5-flash", "gemini-2.5-pro", "gemini-flash-latest"];

    /// <summary>Das Modell für „automatisch“ aus den Modellen des Schlüssels (kostet kein Kontingent): Flash-Lite zuerst. Fehlerbericht vom
    /// 14.11.2026: auch mit Denkstufe „low“ brauchte gemini-flash-latest für zwei Seiten über eine Minute – Flash-Lite steht von Haus aus auf
    /// „minimal“, ist auf kurze Antwortzeiten gebaut und laut Google-Doku „Models“ neben Gemini 4 Hydrogen für neue Projekte empfohlen.</summary>
    public static async Task<string> PreferredModelAsync(string key, CancellationToken cancel) => ChoosePreferred(await CheckKeyAsync(key, cancel));

    /// <summary>Die Wahl zu <see cref="PreferredModelAsync"/>: Flash-Lite-Alias „…-latest“, sonst das neueste stabile Flash-Lite, sonst
    /// <see cref="DefaultModel"/>, sonst ein stabiles Flash.</summary>
    internal static string ChoosePreferred(IReadOnlyList<string> models) =>
        models.FirstOrDefault(m => IsLite(m) && m.EndsWith("-latest", StringComparison.Ordinal))
        ?? models.FirstOrDefault(m => IsLite(m) && !IsPreview(m))
        ?? models.FirstOrDefault(m => m == DefaultModel)
        ?? models.FirstOrDefault(m => m.Contains("flash", StringComparison.OrdinalIgnoreCase) && !IsPreview(m))
        ?? DefaultModel;

    private static bool IsLite(string model) => model.Contains("flash-lite", StringComparison.OrdinalIgnoreCase);

    private static bool IsPreview(string model) => model.Contains("preview", StringComparison.OrdinalIgnoreCase) || model.Contains("exp", StringComparison.OrdinalIgnoreCase);

    public const string TermsUrl = "https://ai.google.dev/gemini-api/terms";
    public const string KeyUrl = "https://aistudio.google.com/app/apikey";
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/";

    private static readonly Lazy<HttpClient> http = new(() => new HttpClient { Timeout = TimeSpan.FromMinutes(3) }); // lange Dokumente brauchen eine Weile

    /// <summary>Prüft einen Schlüssel ohne Kosten (Liste der Modelle, verbraucht kein Kontingent) und liefert die Textmodelle, die er benutzen
    /// darf – Aliasnamen „…-latest“ zuerst, dann die neueren vor den älteren. Wirft <see cref="GeminiException"/>.</summary>
    public static async Task<List<string>> CheckKeyAsync(string key, CancellationToken cancel = default)
    {
        List<string> models = [];
        string? pageToken = null;
        for (var page = 0; page < 5; page++) // eine Seite reicht meist; mehr als 5 × 1000 Modelle gibt es nicht
        {
            var url = BaseUrl + "models?pageSize=1000" + (pageToken != null ? "&pageToken=" + Uri.EscapeDataString(pageToken) : string.Empty);
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            AddKey(request, key);
            using var answer = await SendAsync(request, cancel);
            var root = answer.RootElement;
            if (root.TryGetProperty("models", out var list) && list.ValueKind == JsonValueKind.Array)
            {
                foreach (var model in list.EnumerateArray())
                {
                    if (model.ValueKind != JsonValueKind.Object || !model.TryGetProperty("name", out var nameElement) || nameElement.GetString() is not { } name) { continue; }
                    var methods = model.TryGetProperty("supportedGenerationMethods", out var m) && m.ValueKind == JsonValueKind.Array
                        ? m.EnumerateArray().Select(e => e.GetString()).ToList() : [];
                    name = name.StartsWith("models/", StringComparison.Ordinal) ? name["models/".Length..] : name;
                    if (methods.Contains("generateContent") && IsTextModel(name)) { models.Add(name); }
                }
            }
            pageToken = root.TryGetProperty("nextPageToken", out var next) ? next.GetString() : null;
            if (string.IsNullOrEmpty(pageToken)) { break; }
        }
        return [.. models.Distinct().OrderBy(n => n.EndsWith("-latest", StringComparison.Ordinal) ? 0 : 1).ThenByDescending(n => n, StringComparer.Ordinal)];
    }

    /// <summary>Ausweichmodell, wenn <paramref name="failed"/> nicht antwortet (seit 14.11.2026 statt eines Auswahlfelds in den Einstellungen):
    /// aus den Modellen des Schlüssels zuerst ein Flash-Lite-Alias „…-latest“, dann das neueste Flash-Lite, dann ein anderes Flash-Modell.
    /// Lite-Modelle sind schneller, großzügiger begrenzt und haben ein eigenes Kontingent. Null, wenn es keins gibt.</summary>
    public static async Task<string?> FallbackModelAsync(string key, string failed, CancellationToken cancel) =>
        ChooseFallback(await CheckKeyAsync(key, cancel), failed);

    /// <summary>Die Wahl zu <see cref="FallbackModelAsync"/> aus einer Modellliste wie <see cref="CheckKeyAsync"/> sie liefert.</summary>
    internal static string? ChooseFallback(IEnumerable<string> available, string failed)
    {
        var models = available.Where(m => !string.Equals(m, failed, StringComparison.OrdinalIgnoreCase)).ToList();
        return models.FirstOrDefault(m => IsLite(m) && m.EndsWith("-latest", StringComparison.Ordinal))
            ?? models.FirstOrDefault(m => IsLite(m) && !IsPreview(m)) // die Liste ist absteigend sortiert: das neueste zuerst
            ?? models.FirstOrDefault(m => m.Contains("flash", StringComparison.OrdinalIgnoreCase) && !IsPreview(m));
    }

    /// <summary>Der Schlüssel in den Kopf der Anfrage. Googles Schlüssel bestehen aus sichtbaren ASCII-Zeichen; ein anderes Zeichen (etwa ein
    /// mitkopiertes Leer- oder Anführungszeichen) ließe HttpClient mit einer Ausnahme scheitern, die nicht nach dem Schlüssel klingt.</summary>
    private static void AddKey(HttpRequestMessage request, string key)
    {
        key = key.Trim();
        if (key.Length == 0 || key.Any(c => c is < '!' or > '~')) { throw new GeminiException(Lng.T("Der API-Schlüssel ist ungültig.")); }
        request.Headers.Add("x-goog-api-key", key);
    }

    /// <summary>Für Zusammenfassungen taugen nur Gemini-Textmodelle – nicht Sprache, Bilder, Video, Live-Audio oder Einbettungen.</summary>
    private static bool IsTextModel(string name) =>
        name.StartsWith("gemini", StringComparison.OrdinalIgnoreCase)
        && !sourceArray.Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));

    /// <summary>Bis die erste Antwort kommt (das Modell denkt bei langen Dokumenten erst nach) bzw. zwischen zwei Stücken der Antwort – danach
    /// gilt die Anfrage als hängengeblieben (Fehlerbericht 14.11.2026: nach drei Minuten „Gemini antwortet…“ ohne jede Reaktion).</summary>
    internal static TimeSpan FirstChunkTimeout { get; set; } = TimeSpan.FromMinutes(2); // änderbar nur für Tests
    internal static TimeSpan NextChunkTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>So oft höchstens bekommt der Aufrufer die entstehende Antwort (s. <see cref="GenerateAsync"/>).</summary>
    private static readonly TimeSpan ProgressInterval = TimeSpan.FromMilliseconds(400);

    /// <summary>Nur für Tests: ersetzt die Verbindung zu Google durch einen nachgestellten Server (muss vor der ersten Anfrage gesetzt sein).</summary>
    internal static HttpMessageHandler? TestHandler { get; set; }

    /// <summary>Für die gestreamte Antwort: ohne Gesamtfrist, die Fristen setzt <see cref="StreamAsync"/> je Stück.</summary>
    private static readonly Lazy<HttpClient> streamHttp = new(() => new HttpClient(TestHandler ?? new HttpClientHandler()) { Timeout = Timeout.InfiniteTimeSpan });
    internal static readonly string[] sourceArray = ["tts", "image", "live", "embedding", "audio", "transcribe", "omni", "banana", "robotics", "computer-use", "aqa"];

    /// <summary>Eine Anfrage mit Systemanweisung und dem bisherigen Gespräch (abwechselnd Benutzer und Modell, zuletzt Benutzer – die
    /// Schnittstelle merkt sich nichts, jede Frage schickt alles mit); liefert den Antworttext (Markdown). Die Antwort kommt gestreamt
    /// (streamGenerateContent mit Server-Sent Events): <paramref name="progress"/> bekommt den bisherigen Text, sobald neuer da ist (höchstens
    /// alle <see cref="ProgressInterval"/>) – so
    /// sieht man sie entstehen, statt minutenlang auf die ganze zu warten. Wirft <see cref="GeminiException"/>.</summary>
    public static async Task<string> GenerateAsync(string key, string model, string systemInstruction, IReadOnlyList<(bool User, string Text)> contents, Action<string>? progress, CancellationToken cancel)
    {
        byte[] Body(bool lowThinking)
        {
            using MemoryStream body = new();
            using (Utf8JsonWriter json = new(body))
            {
                json.WriteStartObject();
                json.WriteStartObject("systemInstruction");
                WriteParts(json, systemInstruction);
                json.WriteEndObject();
                json.WriteStartArray("contents");
                foreach (var (user, text) in contents)
                {
                    json.WriteStartObject();
                    json.WriteString("role", user ? "user" : "model");
                    WriteParts(json, text);
                    json.WriteEndObject();
                }
                json.WriteEndArray();
                if (lowThinking)
                {
                    // Die aktuellen Modelle denken vor jeder Antwort nach, Gemini 4 Hydrogen von Haus aus auf „high“ – für Zusammenfassungen
                    // und Fragen zu einem Dokument reicht „low“, die Antwort kommt so deutlich schneller (Fehlerbericht 14.11.2026: über eine
                    // Minute für eine zweiseitige Gebrauchsanweisung; Stufen laut Google-Doku „Thinking“, „low“ können alle aktuellen Modelle)
                    json.WriteStartObject("generationConfig");
                    json.WriteStartObject("thinkingConfig");
                    json.WriteString("thinkingLevel", "low");
                    json.WriteEndObject();
                    json.WriteEndObject();
                }
                json.WriteEndObject();
            }
            return body.ToArray();
        }
        var name = string.IsNullOrWhiteSpace(model) ? DefaultModel : model.Trim();
        if (name.StartsWith("models/", StringComparison.Ordinal)) { name = name["models/".Length..]; }
        var lowThinking = !IsLite(name); // Flash-Lite denkt von Haus aus kaum („minimal“) – „low“ wäre dort mehr, nicht weniger
        var bytes = Body(lowThinking);
        // Überlastung (Fehler 503) dauert oft nur Sekunden (Fehlerbericht 14.11.2026): zweimal kurz danach erneut, erst dann melden
        for (var attempt = 0; ; attempt++)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, BaseUrl + "models/" + Uri.EscapeDataString(name) + ":streamGenerateContent?alt=sse");
            AddKey(request, key);
            request.Content = new ByteArrayContent(bytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            try { return await StreamAsync(request, progress, cancel); }
            catch (GeminiException ex) when (ex.Kind == GeminiFailure.Overloaded && attempt < 2)
            {
                await Task.Delay(attempt == 0 ? 2000 : 5000, cancel);
            }
            catch (GeminiException ex) when (ex.Kind == GeminiFailure.InvalidRequest && lowThinking)
            {
                (lowThinking, bytes) = (false, Body(false)); // ein Modell, das die Denkstufe nicht kennt: ohne sie noch einmal
            }
        }

        static void WriteParts(Utf8JsonWriter json, string text)
        {
            json.WriteStartArray("parts");
            json.WriteStartObject();
            json.WriteString("text", text);
            json.WriteEndObject();
            json.WriteEndArray();
        }
    }

    /// <summary>Liest eine gestreamte Antwort: Zeilen „data: {…}“, je eine mit dem nächsten Stück Text (Denkschritte übersprungen). Kommt
    /// innerhalb der Fristen nichts, gilt das Modell als hängengeblieben (<see cref="GeminiFailure.Timeout"/>, ein anderes Modell darf es
    /// versuchen). Fehler vor dem Stream kommen wie bei <see cref="SendAsync"/> als JSON, Fehler mitten darin als Stück mit „error“.</summary>
    private static async Task<string> StreamAsync(HttpRequestMessage request, Action<string>? progress, CancellationToken cancel)
    {
        using var idle = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        idle.CancelAfter(FirstChunkTimeout);
        static GeminiException TimedOut(Exception? inner) => new(Lng.T("Google hat nicht rechtzeitig geantwortet."), inner, GeminiFailure.Timeout);
        HttpResponseMessage response;
        try { response = await streamHttp.Value.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, idle.Token); }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { throw; }
        catch (OperationCanceledException ex) { throw TimedOut(ex); }
        catch (HttpRequestException ex) { throw new GeminiException(Lng.T("Keine Verbindung zu Google – bist du mit dem Internet verbunden?"), ex); }
        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                byte[] body;
                try { body = await response.Content.ReadAsByteArrayAsync(idle.Token); }
                catch (OperationCanceledException) when (!cancel.IsCancellationRequested) { body = []; }
                JsonDocument? document = null;
                try { document = JsonDocument.Parse(body); }
                catch (JsonException) { } // keine JSON-Antwort (etwa eine Fehlerseite eines Proxys)
                var error = ReadError(document);
                document?.Dispose();
                throw new GeminiException(ErrorText(response.StatusCode, error), null, Classify(response.StatusCode, error));
            }
            StringBuilder result = new();
            string? finish = null;
            var reported = Stopwatch.StartNew();
            var reportedOnce = false;
            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync(idle.Token);
                using StreamReader reader = new(stream, Encoding.UTF8);
                while (await reader.ReadLineAsync(idle.Token) is { } line)
                {
                    if (!line.StartsWith("data:", StringComparison.Ordinal)) { continue; } // Leerzeilen trennen die Ereignisse
                    JsonDocument chunk;
                    try { chunk = JsonDocument.Parse(line.AsMemory(5)); }
                    catch (JsonException) { continue; }
                    using (chunk)
                    {
                        var root = chunk.RootElement;
                        if (root.TryGetProperty("error", out var failure) && failure.ValueKind == JsonValueKind.Object)
                        {
                            var code = failure.TryGetProperty("code", out var c) && c.TryGetInt32(out var n) ? (HttpStatusCode)n : HttpStatusCode.InternalServerError;
                            var info = ReadError(chunk);
                            throw new GeminiException(ErrorText(code, info), null, Classify(code, info));
                        }
                        if (root.TryGetProperty("promptFeedback", out var feedback) && feedback.TryGetProperty("blockReason", out var blocked))
                        {
                            throw new GeminiException(string.Format(Lng.T("Google hat die Anfrage abgelehnt ({0})."), blocked.GetString()));
                        }
                        var before = result.Length;
                        finish = AppendText(root, result) ?? finish;
                        // den bisherigen Text höchstens alle ProgressInterval weitergeben (das erste Stück sofort) – sonst entstünde er bei
                        // vielen kleinen Stücken jedes Mal neu; den Rest zeigt der Aufrufer ohnehin mit der fertigen Antwort
                        if (result.Length > before && progress != null && (!reportedOnce || reported.Elapsed >= ProgressInterval))
                        {
                            progress(result.ToString());
                            reportedOnce = true;
                            reported.Restart();
                        }
                    }
                    idle.CancelAfter(result.Length > 0 ? NextChunkTimeout : FirstChunkTimeout); // jedes Stück verlängert die Frist
                }
            }
            catch (OperationCanceledException) when (cancel.IsCancellationRequested) { throw; }
            catch (OperationCanceledException ex) { throw TimedOut(ex); }
            catch (IOException ex) { throw new GeminiException(Lng.T("Keine Verbindung zu Google – bist du mit dem Internet verbunden?"), ex); }
            if (result.Length == 0)
            {
                throw new GeminiException(finish is null or "STOP" ? Lng.T("Google hat keine Antwort geliefert.") : string.Format(Lng.T("Google hat keine Antwort geliefert ({0})."), finish));
            }
            return result.ToString();
        }
    }

    /// <summary>Hängt den Text eines Antwortstücks an (ohne Denkschritte); liefert den Grund des Endes, falls das Stück ihn nennt.</summary>
    private static string? AppendText(JsonElement root, StringBuilder result)
    {
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0) { return null; }
        var candidate = candidates[0];
        if (candidate.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True) { continue; } // Denkschritte nicht anzeigen
                if (part.TryGetProperty("text", out var piece) && piece.ValueKind == JsonValueKind.String) { result.Append(piece.GetString()); }
            }
        }
        return candidate.TryGetProperty("finishReason", out var reason) && reason.ValueKind == JsonValueKind.String ? reason.GetString() : null;
    }

    /// <summary>Sendet und liest die Antwort als JSON; Fehler werden zu verständlichen Meldungen.</summary>
    private static async Task<JsonDocument> SendAsync(HttpRequestMessage request, CancellationToken cancel)
    {
        HttpResponseMessage response;
        try { response = await http.Value.SendAsync(request, cancel); }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { throw; }
        catch (TaskCanceledException ex) { throw new GeminiException(Lng.T("Google hat nicht rechtzeitig geantwortet."), ex); }
        catch (HttpRequestException ex) { throw new GeminiException(Lng.T("Keine Verbindung zu Google – bist du mit dem Internet verbunden?"), ex); }
        using (response)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync(cancel);
            JsonDocument? document = null;
            try { document = JsonDocument.Parse(bytes); }
            catch (JsonException) { } // keine JSON-Antwort (etwa eine Fehlerseite eines Proxys)
            if (response.IsSuccessStatusCode && document != null) { return document; }
            var error = ReadError(document);
            document?.Dispose();
            throw new GeminiException(ErrorText(response.StatusCode, error), null, Classify(response.StatusCode, error));
        }
    }

    /// <summary>Was die Fehlerantwort verrät. Bei Fehler 429 nennt Google in /details das gerissene Kontingent (google.rpc.QuotaFailure:
    /// quotaId wie „GenerateRequestsPerDayPerProjectPerModel-FreeTier“, quotaMetric, quotaValue = Grenze, quotaDimensions.model) und bei
    /// kurzen Grenzen die Wartezeit (google.rpc.RetryInfo: retryDelay wie „39s“) – eine Uhrzeit für die Freigabe steht nicht darin.</summary>
    internal sealed record ErrorInfo(string? Status, string? Reason, string? Message, string? QuotaId, string? QuotaMetric, string? QuotaValue, string? Model, double? RetrySeconds);

    internal static ErrorInfo ReadError(JsonDocument? document)
    {
        if (document == null || !document.RootElement.TryGetProperty("error", out var error) || error.ValueKind != JsonValueKind.Object)
        {
            return new(null, null, null, null, null, null, null, null);
        }
        static string? Text(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        string? reason = null, quotaId = null, quotaMetric = null, quotaValue = null, model = null;
        double? retry = null;
        if (error.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
        {
            foreach (var detail in details.EnumerateArray())
            {
                if (detail.ValueKind != JsonValueKind.Object) { continue; }
                reason ??= Text(detail, "reason");
                if (detail.TryGetProperty("violations", out var violations) && violations.ValueKind == JsonValueKind.Array && violations.GetArrayLength() > 0
                    && violations[0].ValueKind == JsonValueKind.Object)
                {
                    var violation = violations[0]; // die erste genügt – meist ist es nur eine
                    (quotaId, quotaMetric, quotaValue) = (Text(violation, "quotaId"), Text(violation, "quotaMetric"), Text(violation, "quotaValue"));
                    if (violation.TryGetProperty("quotaDimensions", out var dimensions) && dimensions.ValueKind == JsonValueKind.Object) { model = Text(dimensions, "model"); }
                }
                if (Text(detail, "retryDelay") is { } delay && double.TryParse(delay.TrimEnd('s'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
                {
                    retry = seconds;
                }
            }
        }
        // Fehlen die Angaben in /details, stehen sie oft nur im Fließtext der Meldung (Fehlerbericht 14.11.2026: nur die allgemeine Meldung
        // erschien), etwa „Quota exceeded for metric: …, limit: 20, model: gemini-2.5-flash … Please retry in 52.9s.“
        var message = Text(error, "message");
        if (message != null)
        {
            if (MessageQuotaRegex().Match(message) is { Success: true } quota)
            {
                quotaMetric ??= quota.Groups["metric"].Value;
                quotaValue ??= quota.Groups["limit"].Value;
                if (quota.Groups["model"].Success) { model ??= quota.Groups["model"].Value; }
            }
            if (retry == null && MessageRetryRegex().Match(message) is { Success: true } wait
                && double.TryParse(wait.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
            {
                retry = seconds;
            }
        }
        return new(Text(error, "status"), reason, message, quotaId, quotaMetric, quotaValue, model, retry);
    }

    /// <summary>Mitternacht pazifischer Zeit in Ortszeit – dann setzt Google die Tageskontingente zurück (laut Dokumentation der Gemini API,
    /// „Rate limits“, gelesen am 14.11.2026). Null, wenn Windows die Zeitzone nicht kennt.</summary>
    internal static DateTime? DailyQuotaReset(DateTimeOffset now)
    {
        try
        {
            var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // Windows-Name, gilt auch für die Sommerzeit
            var there = TimeZoneInfo.ConvertTime(now, pacific);
            var midnight = new DateTime(there.Year, there.Month, there.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1);
            return TimeZoneInfo.ConvertTimeToUtc(midnight, pacific).ToLocalTime();
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { return null; }
    }

    /// <summary>Verständliche Meldung zu einem Kontingentfehler: welches Kontingent, seine Grenze und wann es wieder geht.</summary>
    private static string QuotaText(ErrorInfo error)
    {
        var id = (error.QuotaId ?? string.Empty) + " " + (error.QuotaMetric ?? string.Empty);
        var model = error.Model ?? string.Empty;
        var limit = long.TryParse(error.QuotaValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : (long?)null;
        var retry = error.RetrySeconds is { } seconds ? string.Format(Lng.T("Versuche es in {0} Sekunden noch einmal."), Math.Max(1, (int)Math.Ceiling(seconds))) : null;
        if (limit == 0 && model.Length > 0) { return string.Format(Lng.T("Für {0} gibt es kein kostenloses Kontingent."), model); }
        if (id.Contains("PerDay", StringComparison.OrdinalIgnoreCase))
        {
            var text = limit is { } perDay && model.Length > 0
                ? string.Format(Lng.T("Das kostenlose Tageskontingent für {0} ist aufgebraucht ({1} Anfragen am Tag)."), model, perDay)
                : Lng.T("Das kostenlose Tageskontingent ist aufgebraucht.");
            if (DailyQuotaReset(DateTimeOffset.Now) is { } reset)
            {
                var time = reset.ToString("t", System.Globalization.CultureInfo.CurrentCulture);
                text += " " + string.Format(reset.Date == DateTime.Today ? Lng.T("Google gibt es heute um {0} Uhr wieder frei.") : Lng.T("Google gibt es morgen um {0} Uhr wieder frei."), time);
            }
            return text;
        }
        if (id.Contains("Token", StringComparison.OrdinalIgnoreCase))
        {
            var text = limit is { } tokens
                ? string.Format(Lng.T("Das kostenlose Minutenkontingent ({0} Token) reicht für diesen Text gerade nicht."), tokens.ToString("N0", System.Globalization.CultureInfo.CurrentCulture))
                : Lng.T("Das kostenlose Minutenkontingent reicht für diesen Text gerade nicht.");
            return text + " " + (retry ?? Lng.T("Versuche es in einer Minute noch einmal."));
        }
        if (id.Contains("PerMinute", StringComparison.OrdinalIgnoreCase))
        {
            return Lng.T("Zu viele Anfragen in kurzer Zeit.") + " " + (retry ?? Lng.T("Versuche es in einer Minute noch einmal."));
        }
        var general = limit is { } any && model.Length > 0
            ? string.Format(Lng.T("Das kostenlose Kontingent für {0} ist erschöpft (Grenze: {1})."), model, any)
            : Lng.T("Das kostenlose Kontingent ist erschöpft.");
        general += " " + (retry ?? Lng.T("Versuche es später noch einmal."));
        return general + GoogleWording(error); // nicht eingeordnet: Googles eigenen Wortlaut zeigen – er nennt oft mehr
    }

    /// <summary>Googles eigener Wortlaut als Nachsatz („Google meldet: …“), leer ohne Meldung.</summary>
    private static string GoogleWording(ErrorInfo error)
    {
        if (string.IsNullOrWhiteSpace(error.Message)) { return string.Empty; }
        var original = WhitespaceRegex().Replace(error.Message, " ").Trim();
        return "\n\n" + Lng.T("Google meldet:") + " " + (original.Length > 600 ? original[..600] + "…" : original);
    }

    /// <summary>Wovon ein Fehler handelt (s. <see cref="GeminiFailure"/>) – ein Projekt ohne Guthaben ist kein Kontingentfehler, dort hilft kein
    /// anderes Modell.</summary>
    internal static GeminiFailure Classify(HttpStatusCode code, ErrorInfo error)
    {
        if (code == HttpStatusCode.PaymentRequired || IsBillingProblem(error)) { return GeminiFailure.Other; }
        if (code == HttpStatusCode.TooManyRequests || error.Status == "RESOURCE_EXHAUSTED") { return GeminiFailure.Quota; }
        if (code == HttpStatusCode.NotFound) { return GeminiFailure.ModelUnavailable; }
        // nur eine abgelehnte Denkstufe – jeder andere Fehler 400 (etwa ein zu langer Text) käme beim erneuten Senden genauso
        if (code == HttpStatusCode.BadRequest && error.Reason is not ("API_KEY_INVALID" or "API_KEY_SERVICE_BLOCKED")
            && error.Message?.Contains("thinking", StringComparison.OrdinalIgnoreCase) == true) { return GeminiFailure.InvalidRequest; }
        return (int)code >= 500 ? GeminiFailure.Overloaded : GeminiFailure.Other;
    }

    private static bool IsBillingProblem(ErrorInfo error) =>
        error.Message?.Contains("prepayment", StringComparison.OrdinalIgnoreCase) == true || error.Message?.Contains("credits are depleted", StringComparison.OrdinalIgnoreCase) == true;

    internal static string ErrorText(HttpStatusCode code, ErrorInfo error)
    {
        var (status, reason, message) = (error.Status, error.Reason, error.Message);
        if (reason is "API_KEY_INVALID" or "API_KEY_SERVICE_BLOCKED" || code == HttpStatusCode.Unauthorized) { return Lng.T("Der API-Schlüssel ist ungültig."); }
        // Projekt mit Rechnungskonto im Vorauszahlungsmodell und leerem Guthaben (laut Google-Doku „Billing“ Fehler 402, im Fehlerbericht vom
        // 14.11.2026 kam es als RESOURCE_EXHAUSTED mit „Your prepayment credits are depleted“): kein Kontingentproblem, und Google fällt
        // dabei nicht von selbst auf das kostenlose Kontingent zurück
        if (code == HttpStatusCode.PaymentRequired || IsBillingProblem(error))
        {
            return Lng.T("Das Guthaben des Google-Projekts ist aufgebraucht – das Projekt ist mit einem Rechnungskonto verbunden und nutzt nicht das kostenlose Kontingent.")
                + " " + Lng.T("Lade in AI Studio Guthaben auf, oder trenne das Projekt vom Rechnungskonto bzw. nimm einen Schlüssel aus einem Projekt ohne Rechnungskonto (ai.studio/projects).");
        }
        if (code == HttpStatusCode.TooManyRequests || status == "RESOURCE_EXHAUSTED") { return QuotaText(error); }
        if (code == HttpStatusCode.NotFound) { return Lng.T("Für deinen Schlüssel steht gerade kein passendes Gemini-Modell zur Verfügung.") + GoogleWording(error); }
        if (code == HttpStatusCode.Forbidden) { return Lng.T("Google verweigert den Zugriff mit diesem Schlüssel."); }
        if (status == "FAILED_PRECONDITION") { return Lng.T("Die Gemini API ist in deinem Land kostenlos nicht verfügbar."); }
        if ((int)code >= 500) { return Lng.T("Google ist gerade überlastet – versuche es später noch einmal.") + GoogleWording(error); }
        var detail = string.IsNullOrWhiteSpace(message) ? ((int)code).ToString(System.Globalization.CultureInfo.InvariantCulture) : message.Trim();
        return string.Format(Lng.T("Die Anfrage ist fehlgeschlagen: {0}"), detail.Length > 300 ? detail[..300] + "…" : detail);
    }

    /// <summary>„Quota exceeded for metric: …, limit: 20, model: gemini-2.5-flash“ im Fließtext einer Fehlermeldung.</summary>
    [GeneratedRegex(@"metric:\s*(?<metric>[\w./-]+)\s*,\s*limit:\s*(?<limit>\d+)(?:\s*,\s*model:\s*(?<model>[\w.-]+))?", RegexOptions.IgnoreCase)]
    private static partial Regex MessageQuotaRegex();

    /// <summary>„Please retry in 52.9s.“ im Fließtext einer Fehlermeldung.</summary>
    [GeneratedRegex(@"retry in\s*([\d.]+)\s*s", RegexOptions.IgnoreCase)]
    private static partial Regex MessageRetryRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
