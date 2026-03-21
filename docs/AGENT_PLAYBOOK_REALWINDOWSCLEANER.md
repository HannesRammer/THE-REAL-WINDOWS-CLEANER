# Agent-Playbook fuer theRealWindowsCleaner

Diese Version ist projekt-spezifisch und direkt fuer das aktuelle Repo nutzbar.

## 1) Ziel

Schnelle, sichere Iterationen fuer die naechsten Backlog-Punkte bei stabiler Qualitaet.

## 2) Empfohlenes aktives Setup

1. `Lead` (`default`): Planung, Priorisierung, Integrationsreihenfolge.
2. `Contract-Agent` (`explorer`): Modell-/Interface-Aenderungen zuerst festziehen.
3. `Worker A` (`worker`): Malware-Scan-Erkennung robuster machen.
4. `Worker B` (`worker`): Tool-Action-Priorisierung (Primary/Secondary).
5. `Worker C` (`worker`): Vorher/Nachher-Mini-Balken im Summary-Bereich.
6. `Worker D` (`worker`): UI-Smoke-Tests fuer Wizard-Flow.
7. `Review-Agent` (`explorer`): Regression/Security/Code-Risiken.
8. `QA-Release-Agent` (`worker`): finale Builds, Tests, CI-Readiness.

## 3) Ownership mit echten Pfaden

### Lead

- `docs/COPILOT_ISSUES.md`
- `README.md`

### Contract-Agent

- `src/CleanWizard.Core/Models/Models.cs`
- `src/CleanWizard.Core/Interfaces/IServices.cs`
- `src/CleanWizard.Core/Interfaces/IStep.cs`

### Worker A (Malware-Scan-Quelle + Fallbacks)

- `src/CleanWizard.Infrastructure/Services/SystemInfoService.cs`
- `src/CleanWizard.App/ViewModels/SystemCheckViewModel.cs`
- `src/CleanWizard.Core/Models/Models.cs` (nur falls neue Source-Felder noetig)

### Worker B (Tool-Action-Priorisierung)

- `src/CleanWizard.Core/Models/Models.cs`
- `src/CleanWizard.App/ViewModels/WizardViewModel.cs`
- `src/CleanWizard.App/Views/WizardView.xaml`
- `src/CleanWizard.Modules/Autoruns/AutorunsModule.cs`
- `src/CleanWizard.Modules/Malwarebytes/MalwarebytesModule.cs`
- `src/CleanWizard.Modules/WindowsTools/WindowsToolsModule.cs`

### Worker C (Mini-Balkendiagramme)

- `src/CleanWizard.App/ViewModels/SummaryViewModel.cs`
- `src/CleanWizard.App/Views/SummaryView.xaml`
- `src/CleanWizard.App/Styles/Styles.xaml`

### Worker D (UI-Tests Wizard-Flows)

- `tests/CleanWizard.Core.Tests/WizardServiceTests.cs`
- `tests/CleanWizard.Core.Tests/ToolActionAndPerformanceTests.cs`
- `tests/CleanWizard.Core.Tests/JsonProgressServiceTests.cs`
- Optional neues Testprojekt: `tests/CleanWizard.App.UITests/`

### Review-Agent

- Gesamtreview ueber alle geaenderten Dateien der Iteration

### QA-Release-Agent

- `.github/workflows/build.yml`
- `CleanWizard.sln`
- `CleanWizard.slnx`

## 4) Iterationsreihenfolge (empfohlen)

1. Lead finalisiert Scope pro Issue.
2. Contract-Agent definiert gemeinsame Daten- und UI-Contracts.
3. Worker A/B/C/D arbeiten parallel in getrennten Dateibereichen.
4. Lead merged in Reihenfolge: Contract -> A -> B -> C -> D.
5. Review-Agent liefert priorisierte Findings (P1/P2/P3).
6. QA-Release-Agent faehrt Abschluss-Gates.

## 5) Pflicht-Gates

1. `dotnet restore .\CleanWizard.sln`
2. `dotnet build .\CleanWizard.sln --configuration Release --no-restore`
3. `dotnet test .\CleanWizard.sln --configuration Release --no-build`
4. Keine offenen P1-Findings.
5. Kurz-Doku in `README.md` oder `docs/COPILOT_ISSUES.md` aktualisiert.

## 6) Prompt-Vorlagen fuer dieses Repo

### Prompt: Worker A

```text
Implementiere den Backlog-Punkt "Malware-Scan-Erkennung robuster machen".
Arbeite nur in:
- src/CleanWizard.Infrastructure/Services/SystemInfoService.cs
- src/CleanWizard.App/ViewModels/SystemCheckViewModel.cs
- ggf. src/CleanWizard.Core/Models/Models.cs
Ziel: Defender + Malwarebytes Fallbacks, Quelle im UI anzeigen, "Nicht erkannt" als klarer Zustand.
Ergaenze oder passe Tests an und liste alle geaenderten Dateien auf.
```

### Prompt: Worker B

```text
Implementiere "schrittabhaengige Tool-Sets verfeinern" mit klarer Primary/Secondary-Priorisierung.
Arbeite nur in:
- src/CleanWizard.Core/Models/Models.cs
- src/CleanWizard.App/ViewModels/WizardViewModel.cs
- src/CleanWizard.App/Views/WizardView.xaml
- src/CleanWizard.Modules/*
Ziel: Relevante Aktionen oben, keine irrelevanten Buttons im Schritt.
Ergaenze oder passe Tests an und liste alle geaenderten Dateien auf.
```

### Prompt: Worker C

```text
Implementiere Mini-Balkendiagramme fuer Vorher/Nachher-Werte (CPU, Autostart, RAM, freier Speicher).
Arbeite nur in:
- src/CleanWizard.App/ViewModels/SummaryViewModel.cs
- src/CleanWizard.App/Views/SummaryView.xaml
- src/CleanWizard.App/Styles/Styles.xaml
Ziel: visuell klar "besser/schlechter", Textwerte bleiben erhalten.
Ergaenze oder passe Tests an und liste alle geaenderten Dateien auf.
```

### Prompt: Worker D

```text
Erweitere Tests fuer Wizard-Navigation und Expertenmodus-Sichtbarkeit.
Arbeite nur im tests-Bereich.
Ziel: Smoke-Tests fuer Weiter/Zurueck/Ueberspringen/Spaeter/Erledigt plus Mode-Sichtbarkeit.
Ergaenze oder passe Tests an und liste alle geaenderten Dateien auf.
```

