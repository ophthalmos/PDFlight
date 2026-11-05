using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using PDFLight.Host;

namespace PDFLight.Script;

/// <summary>Hilfsprogramm: führt ein JavaScript eines PDF-Formulars aus (Jint, ein Interpreter ohne erzeugten Maschinencode). Es läuft
/// abgeschottet wie pdfhost, sieht aber keine PDF-Daten: Das Hauptprogramm schickt Skripttext und Feldzustände, zurück kommen der Wert des
/// Felds und was das Skript an anderen Feldern geändert hat. Jeder Lauf bekommt eine neue Engine mit festen Grenzen – nichts bleibt von
/// einem Skript zum nächsten. Von .NET sieht ein Skript nur, was hier ausdrücklich steht (<see cref="NewEngine"/>).</summary>
internal static class Program
{
    private const int AlertLimit = 3, AlertLength = 1000;
    private const int FieldReadOnly = 1, FieldRequired = 2; // Bits von /Ff
    private static readonly string[] TypeNames = ["text", "button", "checkbox", "radiobutton", "combobox", "listbox", "text", "signature"];
    private static readonly Prepared<Acornima.Ast.Script> Functions = Engine.PrepareScript(AcrobatFunctions.Source);

    private static int Main()
    {
        CrashGuard.Install();
        using var input = new BinaryReader(Console.OpenStandardInput());
        using var output = new BinaryWriter(new BufferedStream(Console.OpenStandardOutput(), 1 << 12));
        while (true)
        {
            byte command;
            try { command = input.ReadByte(); }
            catch (EndOfStreamException) { return 0; } // Hauptprogramm beendet oder Pipe geschlossen
            if (command == Protocol.Quit) { return 0; }
            try
            {
                if (command != Protocol.RunScript) { throw new InvalidDataException($"Unbekannter Befehl {command}."); }
                var result = Run(input);
                output.Write(Protocol.Ok);
                output.Write(result.Rc);
                output.Write(result.Value);
                output.Write(result.Writes.Count);
                foreach (var (name, value) in result.Writes) { output.Write(name); output.Write(value); }
                output.Write(result.Properties.Count);
                foreach (var (name, flags, display) in result.Properties) { output.Write(name); output.Write(flags); output.Write(display); }
                output.Write(result.Alerts.Count);
                foreach (var alert in result.Alerts) { output.Write(alert); }
            }
            catch (Exception ex) when (ex is not EndOfStreamException and not IOException) // Skriptfehler, Zeit- oder Speichergrenze
            {
                output.Write(Protocol.Error);
                output.Write(ex.GetType().Name + ": " + ex.Message);
            }
            output.Flush();
        }
    }

    private readonly record struct Result(bool Rc, string Value, List<(string Name, string Value)> Writes, List<(string Name, int Flags, int Display)> Properties, List<string> Alerts);

    private sealed record State(string Name, string Value, int Type, int Flags, int Display, string[] Options);

    private sealed record Handed(JsObject Field, JsValue Value, bool ReadOnly, bool Required, int Display, bool Hidden);

    private static Result Run(BinaryReader input)
    {
        var documentScripts = new string[input.ReadInt32()];
        for (var i = 0; i < documentScripts.Length; i++) { documentScripts[i] = input.ReadString(); }
        var states = new Dictionary<string, State>();
        var count = input.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var (name, value, type, flags, display) = (input.ReadString(), input.ReadString(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            var options = new string[input.ReadInt32()];
            for (var j = 0; j < options.Length; j++) { options[j] = input.ReadString(); }
            states[name] = new(name, value, type, flags, display, options);
        }
        var (target, script, eventValue, willCommit, language) = (input.ReadString(), input.ReadString(), input.ReadString(), input.ReadBoolean(), input.ReadString());

        var engine = NewEngine();
        var names = states.Keys.ToArray();
        var alerts = new List<string>();
        var handed = new Dictionary<string, Handed>(); // was getField herausgab: Änderungen daran gehen ans Formular
        JsValue GetField(string name)
        {
            if (handed.TryGetValue(name, out var known)) { return known.Field; }
            if (!states.TryGetValue(name, out var state)) { return JsValue.Null; }
            JsValue value = state.Type is 4 or 5 or 6 && IsPlainNumber(state.Value) ? new JsNumber(double.Parse(state.Value, CultureInfo.InvariantCulture)) : new JsString(state.Value);
            var (readOnly, required, hidden) = ((state.Flags & FieldReadOnly) != 0, (state.Flags & FieldRequired) != 0, state.Display == 1);
            var field = new JsObject(engine);
            field.Set("name", name);
            field.Set("value", value);
            field.Set("valueAsString", state.Value);
            field.Set("type", state.Type >= 0 && state.Type < TypeNames.Length ? TypeNames[state.Type] : "text");
            field.Set("readonly", readOnly);
            field.Set("required", required);
            field.Set("display", state.Display);
            field.Set("hidden", hidden);
            field.Set("pdflightOptions", new JsArray(engine, [.. state.Options.Select(o => (JsValue)new JsString(o))]));
            handed[name] = new(field, value, readOnly, required, state.Display, hidden);
            engine.Invoke("pdflightDecorate", field);
            return field;
        }
        engine.SetValue("getField", new ClrFunction(engine, "getField", (_, args) => args.Length > 0 ? GetField(args[0].ToString()) : JsValue.Null));
        engine.SetValue("numFields", names.Length);
        engine.SetValue("getNthFieldName", new ClrFunction(engine, "getNthFieldName", (_, args) =>
            args.Length > 0 && args[0].IsNumber() && args[0].AsNumber() is var n && n >= 0 && n < names.Length ? new JsString(names[(int)n]) : JsValue.Null));
        engine.SetValue("pdflightLanguage", language);
        engine.SetValue("pdflightAlert", new ClrFunction(engine, "pdflightAlert", (_, args) =>
        {
            if (alerts.Count < AlertLimit && args.Length > 0) { var text = args[0].ToString(); alerts.Add(text.Length > AlertLength ? text[..AlertLength] + "…" : text); }
            return new JsNumber(1); // „OK“: Rückfragen an den Benutzer stellt ein Skript hier nicht
        }));
        // Felder einer Gruppe: AFSimple_Calculate("SUM", ["Posten"]) meint auch Posten.1, Posten.2 …
        engine.SetValue("pdflightFieldsUnder", new ClrFunction(engine, "pdflightFieldsUnder", (_, args) =>
        {
            var prefix = args[0].ToString() + ".";
            return new JsArray(engine, [.. names.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).Select(k => (JsValue)new JsString(k))]);
        }));
        engine.Execute(in Functions);

        var formEvent = new JsObject(engine);
        formEvent.Set("value", eventValue);
        formEvent.Set("rc", true);
        formEvent.Set("willCommit", willCommit);
        formEvent.Set("change", string.Empty);
        formEvent.Set("changeEx", string.Empty);
        formEvent.Set("selStart", 0);
        formEvent.Set("selEnd", 0);
        formEvent.Set("modifier", false);
        formEvent.Set("shift", false);
        formEvent.Set("fieldFull", false);
        formEvent.Set("name", "Field");
        formEvent.Set("type", "Field");
        formEvent.Set("target", GetField(target));
        formEvent.Set("source", GetField(target));
        engine.SetValue("event", formEvent);

        foreach (var documentScript in documentScripts) { engine.Execute(documentScript); } // legen meist Hilfsfunktionen an
        engine.Execute(script);

        var rc = formEvent.Get("rc");
        var result = formEvent.Get("value");
        var writes = new List<(string, string)>();
        var properties = new List<(string, int, int)>();
        foreach (var (name, known) in handed)
        {
            var now = known.Field.Get("value");
            if (!now.Equals(known.Value)) { writes.Add((name, Text(now))); }
            var readOnly = known.Field.Get("readonly").AsBooleanOr(known.ReadOnly);
            var required = known.Field.Get("required").AsBooleanOr(known.Required);
            var hidden = known.Field.Get("hidden").AsBooleanOr(known.Hidden);
            var displayValue = known.Field.Get("display");
            var display = displayValue.IsNumber() ? (int)displayValue.AsNumber() : known.Display;
            if (hidden != known.Hidden) { display = hidden ? 1 : 0; }
            if (readOnly != known.ReadOnly || required != known.Required || display != known.Display)
            {
                var flags = (states[name].Flags & ~(FieldReadOnly | FieldRequired)) | (readOnly ? FieldReadOnly : 0) | (required ? FieldRequired : 0);
                properties.Add((name, flags, display));
            }
        }
        return new(!rc.IsBoolean() || rc.AsBoolean(), Text(result), writes, properties, alerts);
    }

    private static bool AsBooleanOr(this JsValue value, bool fallback) => value.IsBoolean() ? value.AsBoolean() : fallback;

    private static string Text(JsValue value) => value.IsNull() || value.IsUndefined() ? string.Empty : value.ToString();

    /// <summary>Wie Acrobat liefert field.value Zahlen als Zahl, sonst zählte value + value als Textverkettung. Führende Nullen und Daten bleiben Text.</summary>
    private static bool IsPlainNumber(string text) =>
        text.Length is > 0 and < 16 && double.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _)
        && text[^1] != '.' && (text.TrimStart('-') is var digits && (digits.Length == 1 || digits[0] != '0' || digits[1] == '.'));

    /// <summary>Engine mit festen Grenzen. Sie sind Pflicht: Ohne LimitRecursion endet jede Rekursion im Stapelüberlauf. Die Regex-Grenze steht
    /// sonst auf 10 s. Ein Überlauf auf anderen Wegen (rekursives toString, Proxy) beendet den Prozess; das fängt ScriptClient ab.</summary>
    private static Engine NewEngine() => new(options =>
    {
        options.Culture(CultureInfo.InvariantCulture);
        options.TimeoutInterval(TimeSpan.FromSeconds(2));
        options.LimitRecursion(256);
        options.LimitMemory(64_000_000);
        options.RegexTimeoutInterval(TimeSpan.FromSeconds(1));
    });
}

/// <summary>Was Formularskripte von Acrobat erwarten, soweit umgesetzt. getField und event stehen in <see cref="Program"/>, hier das Übrige:
/// util, app.alert, color, display, Datums-, Uhrzeit-, Zahlen-, Prozent- und Sonderformate, Bereichsprüfung und AFSimple_Calculate.
/// Bewusst nicht: alles, was die Datei verlässt oder das Programm steuert (app.launchURL, app.execMenuItem, submitForm, mailDoc, getURL,
/// importDataObject, exportDataObject, print …). Solche Aufrufe gibt es hier nicht; das Skript bricht ab, das Hauptprogramm meldet es.
/// Farben, Schriftgrößen und Ähnliches dürfen Skripte setzen, es bleibt ohne Wirkung. negStyle 1 zeigt deshalb ein Minus statt roter Schrift.
/// Tastenanschläge prüft das Hauptprogramm nur beim Übernehmen (willCommit), nicht beim Tippen.</summary>
internal static class AcrobatFunctions
{
    public const string Source = """
        var display = { visible: 0, hidden: 1, noPrint: 2, noView: 3 };
        var color = { transparent: ['T'], black: ['G', 0], white: ['G', 1], red: ['RGB', 1, 0, 0], green: ['RGB', 0, 1, 0], blue: ['RGB', 0, 0, 1],
          cyan: ['CMYK', 1, 0, 0, 0], magenta: ['CMYK', 0, 1, 0, 0], yellow: ['CMYK', 0, 0, 1, 0], dkGray: ['G', 0.25], gray: ['G', 0.5], ltGray: ['G', 0.75] };
        var border = { s: 'solid', d: 'dashed', b: 'beveled', i: 'inset', u: 'underline' };
        var style = { ch: 'check', cr: 'cross', di: 'diamond', ci: 'circle', st: 'star', sq: 'square' };
        var font = { Times: 'Times-Roman', Helv: 'Helvetica', HelvB: 'Helvetica-Bold', Cour: 'Courier', Symbol: 'Symbol', ZapfD: 'ZapfDingbats' };
        var console = { println: function () { }, show: function () { }, hide: function () { }, clear: function () { } };
        var app = {
          viewerVersion: 11, viewerType: 'Reader', platform: 'WIN', language: 'DEU', calculate: true, runtimeHighlight: false, focusRect: true,
          alert: function (message) { return pdflightAlert(message != null && typeof message == 'object' ? message.cMsg : message); },
          beep: function () { }
        };
        var dirty = true;
        function calculateNow() { }
        var pdflightMonths = {
          de: ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'],
          en: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
          fr: ['janvier', 'février', 'mars', 'avril', 'mai', 'juin', 'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre'],
          es: ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre']
        };
        var pdflightDays = {
          de: ['Sonntag', 'Montag', 'Dienstag', 'Mittwoch', 'Donnerstag', 'Freitag', 'Samstag'],
          en: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
          fr: ['dimanche', 'lundi', 'mardi', 'mercredi', 'jeudi', 'vendredi', 'samedi'],
          es: ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado']
        };
        function pdflightDecorate(field) {
          field.numItems = field.pdflightOptions.length;
          field.getItemAt = function (index) { return this.pdflightOptions[index]; };
          field.isBoxChecked = function (index) { return this.value == this.pdflightOptions[index || 0]; };
          field.checkThisBox = function (index, checked) { this.value = checked === false ? 'Off' : this.pdflightOptions[index || 0]; };
          field.getArray = function () {
            var names = pdflightFieldsUnder(this.name), result = [];
            for (var i = 0; i < names.length; i++) { result.push(getField(names[i])); }
            return result.length > 0 ? result : [this];
          };
          field.setFocus = function () { };
        }
        var util = {
          printd: function (format, date) {
            if (format === 0) { format = 'D:yyyymmddHHMMss'; } else if (format === 1) { format = 'yyyy.mm.dd HH:MM:ss'; } else if (format === 2) { format = 'm/d/yy h:MM:ss tt'; }
            var pad = function (n) { return (n < 10 ? '0' : '') + n; };
            var months = pdflightMonths[pdflightLanguage] || pdflightMonths.en, days = pdflightDays[pdflightLanguage] || pdflightDays.en;
            var hours12 = function () { return date.getHours() % 12 == 0 ? 12 : date.getHours() % 12; };
            return String(format).replace(/yyyy|yy|mmmm|mmm|mm|m|dddd|ddd|dd|d|HH|H|hh|h|MM|M|ss|s|tt|t/g, function (token) {
              switch (token) {
                case 'yyyy': return String(date.getFullYear());
                case 'yy': return pad(date.getFullYear() % 100);
                case 'mmmm': return months[date.getMonth()];
                case 'mmm': return months[date.getMonth()].substring(0, 3);
                case 'mm': return pad(date.getMonth() + 1);
                case 'm': return String(date.getMonth() + 1);
                case 'dddd': return days[date.getDay()];
                case 'ddd': return days[date.getDay()].substring(0, 3);
                case 'dd': return pad(date.getDate());
                case 'd': return String(date.getDate());
                case 'HH': return pad(date.getHours());
                case 'H': return String(date.getHours());
                case 'hh': return pad(hours12());
                case 'h': return String(hours12());
                case 'MM': return pad(date.getMinutes());
                case 'M': return String(date.getMinutes());
                case 'ss': return pad(date.getSeconds());
                case 's': return String(date.getSeconds());
                case 'tt': return date.getHours() < 12 ? 'am' : 'pm';
                default: return date.getHours() < 12 ? 'a' : 'p';
              }
            });
          },
          scand: function (format, text) { return pdflightParseDate(format, text); },
          printf: function (format) {
            var args = arguments, next = 1;
            return String(format).replace(/%(,\d)?([+ 0#]*)(\d+)?(?:\.(\d+))?([dfsx%])/g, function (all, sep, flags, width, precision, type) {
              if (type == '%') { return '%'; }
              var value = args[next++], text;
              // Jint liefert für Gruppen ohne Treffer '' statt undefined, deshalb hier nur „gibt es sie?“
              if (type == 's') { text = String(value); if (precision) { text = text.substring(0, +precision); } }
              else if (type == 'x') { text = (Math.trunc(+value) >>> 0).toString(16); }
              else {
                var number = +value, style = sep ? +sep.charAt(1) : 1;
                text = pdflightFormatNumber(type == 'd' ? Math.trunc(number) : number, type == 'd' ? 0 : (precision ? +precision : 6), style);
                if (number < 0) { text = '-' + text; } else if (flags.indexOf('+') >= 0) { text = '+' + text; } else if (flags.indexOf(' ') >= 0) { text = ' ' + text; }
              }
              if (width && text.length < +width) {
                var fill = flags.indexOf('0') >= 0 && type != 's' ? '0' : ' ';
                var sign = fill == '0' && /^[+ -]/.test(text) ? text.charAt(0) : '';
                text = sign + new Array(+width - text.length + 1).join(fill) + text.substring(sign.length);
              }
              return text;
            });
          },
          printx: function (mask, text) {
            var source = String(text), position = 0, result = '', mode = 0, m = String(mask);
            var take = function (test) {
              while (position < source.length && !test(source.charAt(position))) { position++; }
              return position < source.length ? source.charAt(position++) : '';
            };
            var apply = function (c) { return mode == 1 ? c.toUpperCase() : mode == 2 ? c.toLowerCase() : c; };
            for (var i = 0; i < m.length; i++) {
              var c = m.charAt(i);
              if (c == '?') { result += apply(take(function () { return true; })); }
              else if (c == 'X') { result += apply(take(function (x) { return /[0-9A-Za-zÄÖÜäöüß]/.test(x); })); }
              else if (c == 'A') { result += apply(take(function (x) { return /[A-Za-zÄÖÜäöüß]/.test(x); })); }
              else if (c == '9') { result += take(function (x) { return /\d/.test(x); }); }
              else if (c == '*') { result += apply(source.substring(position)); position = source.length; }
              else if (c == '\\') { i++; result += m.charAt(i); }
              else if (c == '>') { mode = 1; } else if (c == '<') { mode = 2; } else if (c == '=') { mode = 0; }
              else { result += c; }
            }
            return result;
          }
        };
        // Text nach einem Datumsformat lesen. Trennzeichen sind beliebig und dürfen fehlen, Monatsnamen zählen in allen vier Sprachen.
        function pdflightParseDate(format, text) {
          var kinds = [];
          var pattern = String(format).replace(/yyyy|yy|mmmm|mmm|mm|m|dddd|ddd|dd|d|HH|H|hh|h|MM|M|ss|s|tt|t|[^a-zA-Z]+|[a-zA-Z]/g, function (token) {
            if (/^(yyyy|yy)$/.test(token)) { kinds.push('y'); return '(\\d{1,4})'; }
            if (/^(mmmm|mmm)$/.test(token)) { kinds.push('n'); return '([^\\d\\s.,\\/:-]+)'; }
            if (/^(dddd|ddd)$/.test(token)) { kinds.push('w'); return '([^\\d\\s.,\\/:-]+)'; }
            if (/^(mm|m)$/.test(token)) { kinds.push('m'); return '(\\d{1,2})'; }
            if (/^(dd|d)$/.test(token)) { kinds.push('d'); return '(\\d{1,2})'; }
            if (/^(HH|H)$/.test(token)) { kinds.push('H'); return '(\\d{1,2})'; }
            if (/^(hh|h)$/.test(token)) { kinds.push('h'); return '(\\d{1,2})'; }
            if (/^(MM|M)$/.test(token)) { kinds.push('M'); return '(\\d{1,2})'; }
            if (/^(ss|s)$/.test(token)) { kinds.push('s'); return '(\\d{1,2})'; }
            if (/^(tt|t)$/.test(token)) { kinds.push('t'); return '\\s*([ap]\\.?m?\\.?)'; }
            return '[\\s.,\\/:-]*';
          });
          var match = new RegExp('^\\s*' + pattern + '\\s*$', 'i').exec(String(text));
          if (!match) { return null; }
          var now = new Date(), year = now.getFullYear(), month = now.getMonth(), day = now.getDate(), hour = 0, minute = 0, second = 0, pm = null;
          var hasDate = kinds.indexOf('y') >= 0 || kinds.indexOf('m') >= 0 || kinds.indexOf('n') >= 0;
          if (hasDate && kinds.indexOf('d') < 0) { day = 1; }
          for (var i = 0; i < kinds.length; i++) {
            var part = match[i + 1], n = parseInt(part, 10);
            switch (kinds[i]) {
              case 'y': year = part.length <= 2 ? (n < 50 ? 2000 + n : 1900 + n) : n; break;
              case 'm': month = n - 1; break;
              case 'n': month = pdflightMonthIndex(part); if (month < 0) { return null; } break;
              case 'd': day = n; break;
              case 'H': case 'h': hour = n; break;
              case 'M': minute = n; break;
              case 's': second = n; break;
              case 't': pm = /^p/i.test(part); break;
            }
          }
          if (kinds.indexOf('h') >= 0 && (hour < 1 || hour > 12)) { return null; }
          if (pm != null) { hour = hour % 12 + (pm ? 12 : 0); }
          if (hour > 23 || minute > 59 || second > 59) { return null; }
          var date = new Date(year, month, day, hour, minute, second);
          return date.getFullYear() == year && date.getMonth() == month && date.getDate() == day ? date : null;
        }
        function pdflightMonthIndex(text) {
          var wanted = String(text).toLowerCase().replace(/\.$/, '');
          for (var language in pdflightMonths) {
            for (var i = 0; i < 12; i++) {
              var name = pdflightMonths[language][i].toLowerCase();
              if (name == wanted || (wanted.length >= 3 && name.indexOf(wanted) == 0)) { return i; }
            }
          }
          return -1;
        }
        function AFParseDateEx(text, format) { return pdflightParseDate(format, text); }
        function AFDate_KeystrokeEx(format) {
          if (!event.willCommit || event.value === '') { return; }
          var date = pdflightParseDate(format, event.value);
          event.rc = date != null;
          if (date != null) { event.value = util.printd(format, date); }
        }
        function AFDate_FormatEx(format) {
          if (event.value === '') { return; }
          var date = pdflightParseDate(format, event.value);
          if (date != null) { event.value = util.printd(format, date); }
        }
        var pdflightDateFormats = ['m/d', 'm/d/yy', 'mm/dd/yy', 'mm/yy', 'd-mmm', 'd-mmm-yy', 'dd-mmm-yy', 'yy-mm-dd', 'mmm-yy', 'mmmm-yy', 'mmm d, yyyy', 'mmmm d, yyyy', 'm/d/yy h:MM tt', 'm/d/yy HH:MM'];
        var pdflightTimeFormats = ['HH:MM', 'h:MM tt', 'HH:MM:ss', 'h:MM:ss tt'];
        function AFDate_Keystroke(index) { AFDate_KeystrokeEx(pdflightDateFormats[index]); }
        function AFDate_Format(index) { AFDate_FormatEx(pdflightDateFormats[index]); }
        function AFTime_KeystrokeEx(format) { AFDate_KeystrokeEx(format); }
        function AFTime_FormatEx(format) { AFDate_FormatEx(format); }
        function AFTime_Keystroke(index) { AFDate_KeystrokeEx(pdflightTimeFormats[index]); }
        function AFTime_Format(index) { AFDate_FormatEx(pdflightTimeFormats[index]); }
        // Text in Zahl. decimal = das Dezimalzeichen des Felds; ohne Angabe (AFMakeNumber) gilt: Von zwei verschiedenen Zeichen ist das letzte
        // das Dezimalzeichen, ein einzelnes auch, mehrfach dasselbe trennt Tausender. Tausender müssen in Dreiergruppen stehen, sonst null.
        function pdflightParseNumber(text, decimal, currency) {
          var s = String(text).replace(/[\s ']/g, '');
          if (currency) { s = s.split(String(currency).replace(/[\s ]/g, '')).join(''); }
          var negative = /^\(.*\)$/.test(s);
          if (negative) { s = s.substring(1, s.length - 1); }
          var sign = /^([+-])|([+-])$/.exec(s);
          if (sign) { negative = negative || (sign[1] || sign[2]) == '-'; s = s.replace(/^[+-]|[+-]$/, ''); }
          if (!/^(\d[\d.,]*|[.,]\d+)$/.test(s)) { return null; }
          if (decimal === undefined) {
            var lastDot = s.lastIndexOf('.'), lastComma = s.lastIndexOf(',');
            if (lastDot >= 0 && lastComma >= 0) { decimal = lastDot > lastComma ? '.' : ','; }
            else if (lastDot >= 0) { decimal = s.indexOf('.') == lastDot ? '.' : ','; }
            else { decimal = s.indexOf(',') == lastComma ? ',' : '.'; }
          }
          else if (s.indexOf(decimal) < 0 && s.split(decimal == '.' ? ',' : '.').length == 2 && !/^\d{1,3}[.,]\d{3}$/.test(s)) {
            decimal = decimal == '.' ? ',' : '.'; // „1234.5“ im Feld mit Dezimalkomma: So zeigt das Feld seinen Rohwert beim Hineinklicken
          }
          var thousands = decimal == '.' ? ',' : '.';
          var parts = s.split(decimal);
          if (parts.length > 2) { return null; }
          if (parts[0].indexOf(thousands) >= 0) {
            var groups = parts[0].split(thousands);
            for (var i = 0; i < groups.length; i++) {
              if (i == 0 ? !/^\d{1,3}$/.test(groups[i]) : !/^\d{3}$/.test(groups[i])) { return null; }
            }
            parts[0] = groups.join('');
          }
          if (parts.length == 2 && parts[1].indexOf(thousands) >= 0) { return null; }
          var number = parseFloat((parts[0] || '0') + '.' + (parts[1] || '0'));
          return negative ? -number : number;
        }
        function pdflightDecimal(sepStyle) { return sepStyle == 2 || sepStyle == 3 ? ',' : '.'; }
        function pdflightFormatNumber(number, nDec, sepStyle) {
          var fixed = Math.abs(number).toFixed(nDec).split('.');
          var thousands = sepStyle == 0 ? ',' : sepStyle == 2 ? '.' : sepStyle == 4 ? "'" : '';
          if (thousands) { fixed[0] = fixed[0].replace(/\B(?=(\d{3})+$)/g, thousands); }
          return fixed.length > 1 ? fixed[0] + pdflightDecimal(sepStyle) + fixed[1] : fixed[0];
        }
        function AFMakeNumber(value) {
          if (typeof value == 'number') { return value; }
          // auch aus einem Anzeigetext wie „1.234,50 €“: Felder mit mehreren Widgets enthalten ihn, und Skripte lesen oft fremde Felder
          var number = pdflightParseNumber(String(value).replace(/[^\d.,()+\-]/g, ''));
          return number == null ? 0 : number;
        }
        function AFNumber_Keystroke(nDec, sepStyle, negStyle, currStyle, strCurrency, bCurrencyPrepend) {
          if (!event.willCommit || event.value === '') { return; }
          var number = pdflightParseNumber(event.value, pdflightDecimal(sepStyle), strCurrency);
          event.rc = number != null;
          if (number != null) { event.value = String(number); }
        }
        function AFNumber_Format(nDec, sepStyle, negStyle, currStyle, strCurrency, bCurrencyPrepend) {
          if (event.value === '') { return; }
          var number = typeof event.value == 'number' ? event.value : pdflightParseNumber(event.value, undefined, strCurrency);
          if (number == null) { return; } // keine Zahl: stehen lassen, die Anzeige soll keinen Wert verschwinden lassen
          var text = pdflightFormatNumber(number, nDec, sepStyle);
          if (strCurrency) { text = bCurrencyPrepend ? strCurrency + text : text + strCurrency; }
          var isNegative = number < 0 && parseFloat(Math.abs(number).toFixed(nDec)) != 0;
          if (isNegative) { text = negStyle >= 2 ? '(' + text + ')' : '-' + text; }
          event.value = text;
        }
        function AFPercent_Keystroke(nDec, sepStyle) {
          if (!event.willCommit || event.value === '') { return; }
          var typed = String(event.value), percent = typed.indexOf('%') >= 0;
          var number = pdflightParseNumber(typed.replace('%', ''), pdflightDecimal(sepStyle));
          event.rc = number != null;
          if (number != null) { event.value = String(percent ? parseFloat((number / 100).toFixed(12)) : number); } // „50 %“ = 0.5; ohne Zeichen: 0.5 zeigt 50 %
        }
        function AFPercent_Format(nDec, sepStyle, bPercentPrepend) {
          if (event.value === '') { return; }
          var number = typeof event.value == 'number' ? event.value : pdflightParseNumber(event.value);
          if (number == null) { return; }
          var text = pdflightFormatNumber(number * 100, nDec, sepStyle);
          text = bPercentPrepend ? '%' + text : text + '%';
          event.value = number < 0 && parseFloat(Math.abs(number * 100).toFixed(nDec)) != 0 ? '-' + text : text;
        }
        function AFRange_Validate(bGreaterThan, nGreaterThan, bLessThan, nLessThan) {
          if (event.value === '') { return; }
          var number = AFMakeNumber(event.value);
          if ((bGreaterThan && number < nGreaterThan) || (bLessThan && number > nLessThan)) { event.rc = false; }
        }
        // Maske: 9 Ziffer, A Buchstabe, O Buchstabe oder Ziffer, X beliebig, alles andere steht fest. null = passt nicht.
        function pdflightMask(mask, text) {
          var source = String(text), position = 0, result = '';
          for (var i = 0; i < mask.length; i++) {
            var c = mask.charAt(i), test = c == '9' ? /\d/ : c == 'A' ? /[A-Za-zÄÖÜäöüß]/ : c == 'O' ? /[0-9A-Za-zÄÖÜäöüß]/ : c == 'X' ? /./ : null;
            if (test == null) { if (source.charAt(position) == c) { position++; } result += c; continue; }
            if (position >= source.length || !test.test(source.charAt(position))) { return null; }
            result += source.charAt(position++);
          }
          return position == source.length ? result : null;
        }
        function pdflightSpecialMask(psf, text) {
          var digits = String(text).replace(/\D/g, '').length;
          switch (psf) {
            case 0: return '99999';
            case 1: return '99999-9999';
            case 2: return digits == 7 ? '999-9999' : '(999) 999-9999';
            default: return '999-99-9999';
          }
        }
        function AFSpecial_KeystrokeEx(mask) {
          if (!event.willCommit || event.value === '') { return; }
          var result = pdflightMask(mask, event.value);
          event.rc = result != null;
          if (result != null) { event.value = result; }
        }
        function AFSpecial_Keystroke(psf) {
          if (!event.willCommit || event.value === '') { return; }
          var plain = psf == 2 ? String(event.value).replace(/[\s()-]/g, '') : String(event.value).replace(/[\s-]/g, '');
          AFSpecial_KeystrokeEx(pdflightSpecialMask(psf, plain));
        }
        function AFSpecial_Format(psf) {
          if (event.value === '') { return; }
          var plain = String(event.value).replace(/[\s()-]/g, '');
          var result = pdflightMask(pdflightSpecialMask(psf, plain), plain);
          if (result != null) { event.value = result; }
        }
        function AFMergeChange(formEvent) { return formEvent.value; }
        function AFSimple_Calculate(cFunction, cFields) {
          var names = typeof cFields == 'string' ? cFields.split(',') : cFields;
          var numbers = [];
          for (var i = 0; i < names.length; i++) {
            var name = String(names[i]).replace(/^\s+|\s+$/g, '');
            var field = getField(name);
            if (field != null) { numbers.push(AFMakeNumber(field.value)); continue; }
            var group = pdflightFieldsUnder(name);
            for (var j = 0; j < group.length; j++) { numbers.push(AFMakeNumber(getField(group[j]).value)); }
          }
          var result = cFunction == 'PRD' ? 1 : 0; // leere Felder zählen wie bei Acrobat als 0, auch bei AVG und MIN
          for (var k = 0; k < numbers.length; k++) {
            if (cFunction == 'PRD') { result *= numbers[k]; }
            else if (cFunction == 'MIN') { result = k == 0 ? numbers[k] : Math.min(result, numbers[k]); }
            else if (cFunction == 'MAX') { result = k == 0 ? numbers[k] : Math.max(result, numbers[k]); }
            else { result += numbers[k]; }
          }
          if (cFunction == 'AVG' && numbers.length > 0) { result /= numbers.length; }
          event.value = parseFloat(result.toFixed(10)); // ohne 0.30000000000000004
        }
        """;
}
