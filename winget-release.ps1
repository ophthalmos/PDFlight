# winget-release.ps1
# Reicht ein neues PDFlight-Release an das WinGet-Community-Repository weiter.
# Voraussetzung: wingetcreate ist installiert (winget install wingetcreate)
# Voraussetzung: Die Setup-Datei ist bereits unter der URL öffentlich erreichbar.
# Die erste Einreichung (neues Paket WilhelmHappe.PDFlight) lief über "wingetcreate submit" mit fertigen Manifesten;
# dieses Skript ist für alle folgenden Versionen.

$url = "https://www.netradio.info/download/PDFlightSetup.exe"

Write-Host ""
Write-Host "WinGet-Release fuer WilhelmHappe.PDFlight"
Write-Host "------------------------------------------"
Write-Host "Format: x.y.z  (Beispiel: 1.2.0) - wie AppVersion in Installer.iss"
Write-Host ""

do {
    $version = (Read-Host "Versionsnummer eingeben").Trim()
    $valid = $version -match '^\d+\.\d+\.\d+$'
    if (-not $valid) {
        Write-Warning "Ungueltig. Bitte im Format x.y.z eingeben (z. B. 1.2.0)."
    }
} while (-not $valid)

Write-Host ""
Write-Host "Starte wingetcreate fuer Version $version ..."
Write-Host ""

wingetcreate update WilhelmHappe.PDFlight `
    --version $version `
    --urls "$url|x64" `
    --out "$env:TEMP\winget-manifests" `
    --submit

# --out: wingetcreate legt die erzeugten Manifeste sonst als Ordner "manifests" im Projektverzeichnis ab.
#        Sie werden nur fuer den PR ans winget-pkgs-Repository gebraucht und hier nicht benoetigt.

pause
