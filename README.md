# THE-REAL-WINDOWS-CLEANER
Eine modulare Windows‑App, die Nutzer mit einem interaktiven Wizard durch Systembereinigung führt – inklusive Autoruns‑Analyse, Malware‑Scan, Windows‑Optimierungen, Fortschrittsspeicherung, Expertenmodus, Warnsystem, System‑Check und visuell unterstützten Schritt‑für‑Schritt‑Anleitungen

## Safety First

- Schritte mit hohem/kritischem Risiko benötigen eine explizite Sicherheitsbestätigung, bevor sie als erledigt markiert werden können.
- Fortschritt wird automatisch gespeichert und mit bis zu 3 Backup-Dateien abgesichert (`progress.backup.1..3.json`).

## Development Setup

- Install .NET SDK 8.x
- Build the solution:

```powershell
dotnet build .\CleanWizard.sln
```

- Run the WPF app:

```powershell
dotnet run --project .\src\CleanWizard.App\CleanWizard.App.csproj
```
