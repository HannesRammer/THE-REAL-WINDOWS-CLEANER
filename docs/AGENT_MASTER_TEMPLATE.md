# Agenten-Master-Template (Repo-unabhängig)

Diese Vorlage kannst du in jedes neue Projekt kopieren und nur die Platzhalter ersetzen.

## 1) Zielbild

Nutze Agenten nicht nach Menge, sondern nach klarer Verantwortung:

1. Hohe Geschwindigkeit durch Parallelisierung.
2. Hohe Qualität durch getrennte Umsetzung, Review und QA.
3. Saubere Integration durch einen zentralen Lead.

## 2) Standard-Setup

Empfohlenes Kernsetup:

1. `Lead` (`default`)
2. `Architektur/Contracts` (`explorer`)
3. `Feature Worker A` (`worker`)
4. `Feature Worker B` (`worker`)
5. `QA/Test Worker` (`worker`)
6. `Review/Security` (`explorer`)
7. `Release/CI Worker` (`worker`)

## 3) Rollen und Verantwortungen

| Rolle | Fokus | Haupt-Output |
|---|---|---|
| Lead | Scope, Priorisierung, Reihenfolge, Integration | Arbeitspakete, Merge-Reihenfolge, Abnahme |
| Architektur/Contracts | APIs, Modelle, Schnittstellen, Risiken | Contract-Definition, Migrationshinweise |
| Feature Worker A | Implementierung Paket A | Code + Tests im eigenen Dateibereich |
| Feature Worker B | Implementierung Paket B | Code + Tests im eigenen Dateibereich |
| QA/Test Worker | Teststrategie und Regression-Schutz | Unit/Integration/E2E-Tests |
| Review/Security | Fehler-, Risiko- und Sicherheitsprüfung | priorisierte Findings (P1/P2/P3) |
| Release/CI Worker | Build, Pipeline, Paketierung, Freigabe | Release-Readiness-Bericht |

## 4) Skalierung nach Projektgröße

1. Klein: `Lead + Worker + Review`
2. Mittel: `Lead + 2 Worker + QA + Review`
3. Groß: volles 7er-Setup

## 5) Iterationsablauf (Standard)

1. Lead zerlegt Tickets in 3-6 Pakete mit Datei-Ownership.
2. Architektur fixiert minimale Contracts (kurz, blockend).
3. Feature Worker arbeiten parallel in disjunkten Bereichen.
4. QA ergänzt parallel Tests für neue Logik und Flows.
5. Lead integriert Änderungen in definierter Reihenfolge.
6. Review/Security prüft integrierten Stand.
7. Release/CI führt finale Gates aus.

## 6) Verbindliche Arbeitsregeln

1. Jeder Worker bearbeitet nur zugewiesene Dateien/Module.
2. Keine stillen Contract-Änderungen ohne Architekturfreigabe.
3. Kein Merge ohne grüne Pflicht-Gates.
4. Findings aus Review werden vor Release abgearbeitet oder bewusst akzeptiert.
5. Jede Änderung bekommt mindestens einen Test oder eine begründete Testausnahme.

## 7) Pflicht-Gates vor Merge

1. Build erfolgreich.
2. Relevante Tests erfolgreich.
3. Lint/Format (falls vorhanden) erfolgreich.
4. Keine offenen P1-Findings.
5. Dokumentation für neue Bedien- oder API-Pfade aktualisiert.

## 8) Copy-Paste Prompts

### Lead

```text
Ziel: <kurzes Ziel>.
Kontext: <Produkt/Modul>.
Zerlege in 3-6 Arbeitspakete mit klarer Datei-Ownership.
Nenne Risiken, Abhängigkeiten, Integrationsreihenfolge und Done-Kriterien.
```

### Architektur/Contracts

```text
Prüfe betroffene Module und definiere minimale Contracts (Modelle, Interfaces, API/UI).
Markiere Breaking Changes und gib Migrationsschritte an.
Halte die Lösung so klein wie möglich, aber vollständig.
```

### Feature Worker

```text
Implementiere Arbeitspaket <ID> nur in diesen Dateien/Modulen: <Pfadliste>.
Du bist nicht allein im Code: keine fremden Änderungen zurücksetzen.
Ergänze passende Tests für neue Logik.
Liste am Ende alle geänderten Dateien auf.
```

### QA/Test Worker

```text
Erstelle/aktualisiere Tests für <Feature>.
Decke Happy Path, Edge Cases und Regressionen ab.
Melde verbleibende Testlücken explizit.
```

### Review/Security

```text
Reviewe den integrierten Stand auf Bugs, Regressionen, Sicherheits- und Stabilitätsrisiken.
Liefere Findings priorisiert nach P1/P2/P3 mit Datei und Zeilenbezug.
Nenne zusätzlich fehlende oder schwache Tests.
```

### Release/CI Worker

```text
Führe Build, Tests und CI-relevante Checks aus.
Melde klar: grün/rot je Gate, offene Blocker und Release-Readiness.
```

## 9) Anpassung nach Projekttyp

1. Desktop: Fokus auf ViewModels, Threading, UI-Flow, Crash-Safety.
2. Web-Frontend: Fokus auf Accessibility, Rendering, E2E-Flows.
3. Backend/API: Fokus auf Contracts, Migrationen, Lastverhalten, Fehlerpfade.
4. Mobile: Fokus auf Offline/Sync, Batterie, Gerätekompatibilität.
5. Library/SDK: Fokus auf API-Stabilität, Versionierung, Doku-Beispiele.
6. Data/ML: Fokus auf Datenqualität, Reproduzierbarkeit, Evaluationsmetriken.

## 10) Start-Checkliste für neue Repos

1. Projektziel und Nicht-Ziele definieren.
2. Pflicht-Gates und Teststrategie festlegen.
3. Rollen aktivieren (mindestens Lead, Worker, Review).
4. Datei-Ownership für aktuelle Iteration vergeben.
5. Erste Iteration mit maximal 2 parallelen Feature-Workern starten.

## 11) Kurzvorlage für jede neue Iteration

```text
Iteration: <Name>
Ziel: <Outcome>
Scope: <in/out>
Arbeitspakete:
- P1: <Beschreibung> | Owner: <Rolle> | Dateien: <Pfadliste>
- P2: <Beschreibung> | Owner: <Rolle> | Dateien: <Pfadliste>
- P3: <Beschreibung> | Owner: <Rolle> | Dateien: <Pfadliste>
Gates: Build, Tests, Lint, Review
Done wenn: <konkret messbar>
```
