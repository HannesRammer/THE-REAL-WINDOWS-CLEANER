# THE-REAL-WINDOWS-CLEANER
Eine modulare Windows‑App, die Nutzer mit einem interaktiven Wizard durch Systembereinigung führt – inklusive Autoruns‑Analyse, Malware‑Scan, Windows‑Optimierungen, Fortschrittsspeicherung, Expertenmodus, Warnsystem, System‑Check und visuell unterstützten Schritt‑für‑Schritt‑Anleitungen

## Safety First

- Schritte mit hohem/kritischem Risiko benötigen eine explizite Sicherheitsbestätigung, bevor sie als erledigt markiert werden können.
- Fortschritt wird automatisch gespeichert und mit bis zu 3 Backup-Dateien abgesichert (`progress.backup.1..3.json`).
- Tool-Startfehler werden direkt im Wizard als Inline-Hinweis angezeigt.
- Pro Modul sind Screenshot-Platzhalter unter `src/CleanWizard.App/Assets` eingebunden.
- Die App führt Nutzer an, automatisiert aber **keine riskanten Systemeingriffe**.
- Bei kritischem Systemzustand startet ein geführter **Notfallmodus** mit Quick-Fix-Schritten.
- High/Critical-Schritte verlangen eine 3-Punkte-Sicherheitscheckliste vor Abschluss.

## Feature Scope

- Geführter Wizard für:
  - Autoruns (Sysinternals)
  - Malwarebytes Free
  - Windows-Bordmittel
- Check-Modus pro Schritt:
  - Erledigt / Übersprungen / Später
  - Notizen
  - Undo der letzten Änderung
- Zusammenfassung:
  - Score + Schrittstatus
  - Vorher/Nachher-Kennzahlen (Autostart, Speicher, RAM, CPU)
  - Export: TXT, JSON, Log
- Schrittbezogene Tool-Launcher:
  - Pro Schritt passende Windows-Tools/Settings/Downloads
  - Nur Start/Öffnen, keine automatische Bereinigung

## Architektur (Kurz)

- `src/CleanWizard.App`: WPF UI + ViewModels
- `src/CleanWizard.Core`: Domänenmodelle, Interfaces, Wizard-Logik
- `src/CleanWizard.Infrastructure`: Dateispeicherung, Logging, System-/Tool-Services
- `src/CleanWizard.Modules`: Inhaltliche Wizard-Module und Schritte
- `tests/CleanWizard.Core.Tests`: Unit-Tests

## Persistenz

- Fortschritt: `%AppData%/CleanWizard/progress.json`
- Backup-Rotation:
  - `progress.backup.1.json`
  - `progress.backup.2.json`
  - `progress.backup.3.json`
- Bei defekter Hauptdatei wird automatisch aus Backups geladen.

## CI

- Workflow: `.github/workflows/build.yml`
- Führt auf `windows-latest` aus:
  - Restore
  - Build
  - Tests
- Optionaler Release-Artefakt-Workflow:
  - `.github/workflows/release-artifact.yml`
  - erzeugt ein self-contained `win-x64` ZIP-Artefakt

## Development Setup

- Install .NET SDK 8.x
- Build the solution:

```powershell
dotnet build .\CleanWizard.sln
```

- Run tests:

```powershell
dotnet test .\CleanWizard.sln
```

- Run the WPF app:

```powershell
dotnet run --project .\src\CleanWizard.App\CleanWizard.App.csproj
```

## Copilot Backlog

- Priorisierte Copilot-Issues: `docs/COPILOT_ISSUES.md`
- Issue-Template: `.github/ISSUE_TEMPLATE/copilot_task.yml`
