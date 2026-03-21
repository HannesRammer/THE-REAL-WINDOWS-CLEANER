# AGENTS.md (Standard fuer alle Projekte, optimiert)

## Ziel

Maximale Geschwindigkeit bei stabiler Qualitaet durch klare Rollen, harte Gates und eindeutige Ownership.

## Sprache und Prinzipien

1. Standard-Sprache ist Deutsch.
2. Fokus auf Ziel und Ergebnis, nicht auf Wortlaut.
3. Wenn es eine technisch bessere Option gibt: anwenden und kurz begruenden.
4. Keine vagen Aufgaben. Jede Aufgabe bekommt Scope, Owner, Dateien und Done-Kriterien.

## Moduswahl

Es gibt genau zwei Modi:

1. `NEUES_REPO` fuer Greenfield oder komplett neue Struktur.
2. `BESTEHENDES_REPO` fuer Feature, Bugfix, Refactor oder Erweiterung.

Entscheidung:

1. Kein stabiler Prozess oder keine klare Modulstruktur vorhanden -> `NEUES_REPO`.
2. Bereits laufender Code mit bestehenden Modulen/Tickets -> `BESTEHENDES_REPO`.

## Standard-Topologie

### Minimal (klein, schnell)

1. `Lead` (`default`): Planung, Priorisierung, Integration.
2. `Worker` (`worker`): Umsetzung in klar zugewiesenem Bereich.
3. `Review` (`explorer`): Risiken, Regressionen, Testluecken.

### Erweitert (optimal fuer parallele Entwicklung)

1. `Lead` (`default`)
2. `Contract` (`explorer`) fuer Modelle/API/UI-Vertraege
3. `Worker A` (`worker`)
4. `Worker B` (`worker`)
5. `QA/Test` (`worker`)
6. `Review/Security` (`explorer`)
7. `Release/CI` (`worker`)

## Betriebsregeln (verbindlich)

1. Pro Ticket klare Datei-Ownership, keine Ueberschneidung ohne explizite Freigabe.
2. Keine stillen Contract-Aenderungen.
3. Keine Integration ohne gruene Pflicht-Gates.
4. Findings werden als `P1/P2/P3` priorisiert.
5. Offene `P1` blockieren Merge immer.
6. Jede Aenderung hat Tests oder eine dokumentierte Testausnahme.

## Ablauf je Modus

### Modus `NEUES_REPO`

1. Starte mit `docs/AGENT_MASTER_TEMPLATE.md`.
2. Definiere Architekturgrenzen, Teststrategie, Gates, Ownership.
3. Erzeuge daraus `docs/AGENT_PLAYBOOK_<PROJEKT>.md`.
4. Ab Iteration 2 arbeitet das Team nur noch nach Playbook.

### Modus `BESTEHENDES_REPO`

1. Nutze direkt `docs/AGENT_PLAYBOOK_<PROJEKT>.md`, falls vorhanden.
2. Falls nicht vorhanden: aus Template sofort ein Playbook erstellen.
3. Iteration immer in Reihenfolge: `Contract -> Worker parallel -> Review -> Release`.
4. Bei Backlog- oder Strukturwechsel Playbook sofort nachziehen.

## Pflicht-Gates vor Merge

Diese Gates muessen an den Stack angepasst sein. Mindestens:

1. Build erfolgreich.
2. Relevante Tests erfolgreich.
3. Lint/Format (falls vorhanden) erfolgreich.
4. Keine offenen `P1`.
5. Aenderungen an Bedienung/API sind dokumentiert.

Beispiel fuer dieses Repo (.NET):

1. `dotnet restore .\CleanWizard.sln`
2. `dotnet build .\CleanWizard.sln --configuration Release --no-restore`
3. `dotnet test .\CleanWizard.sln --configuration Release --no-build`

## Parallelisierungsregel

1. Nur Aufgaben parallelisieren, deren Dateibereiche disjunkt sind.
2. Maximum ohne Overhead in der Regel: 2-4 aktive Worker.
3. Bei Konflikten: Lead priorisiert Integration statt weiterer Parallelisierung.

## Start-Protokoll je Iteration (Pflicht)

1. Ziel und Scope (`in/out`) in 3-5 Zeilen festhalten.
2. 2-6 Arbeitspakete mit Owner und Dateipfaden definieren.
3. Done-Kriterien und Gates pro Paket festlegen.
4. Erst danach Implementierung starten.

## Dokument-Prioritaet

1. `AGENTS.md` ist globaler Standard.
2. `docs/AGENT_MASTER_TEMPLATE.md` ist universeller Bauplan.
3. `docs/AGENT_PLAYBOOK_<PROJEKT>.md` ist operative Wahrheit im konkreten Repo.

Konfliktregel:

1. Projektspezifisches Playbook gewinnt vor Template und AGENTS.md.
