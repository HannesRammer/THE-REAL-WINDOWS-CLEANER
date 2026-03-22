# QA-Checkliste fuer Releases

Diese Liste ist fuer manuelle Release-Checks gedacht. Haken erst setzen, wenn der Punkt auf einem frischen Build pruefbar bestanden wurde.

## Vorbereitung

- [ ] `dotnet restore .\CleanWizard.sln`
- [ ] `dotnet build .\CleanWizard.sln --configuration Release --no-restore`
- [ ] `dotnet test .\CleanWizard.sln --configuration Release --no-build`
- [ ] App startet im Release-Build ohne Fehlermeldung
- [ ] GitHub-Release-Assets sind fuer den Tag-Build verfuegbar
- [ ] `THE-REAL-WINDOWS-CLEANER-win-x64.zip` und `.sha256` koennen aus dem Release geladen werden
- [ ] SHA256 der ZIP-Datei stimmt mit der `.sha256`-Datei ueberein

## Basisfluss

- [ ] System-Check erscheint beim Start
- [ ] Wizard laesst sich vom System-Check aus starten
- [ ] Navigation weiter/zurueck funktioniert
- [ ] Schritte koennen als erledigt markiert werden
- [ ] Ueberspringen und Spaeter setzen den Status korrekt
- [ ] Zusammenfassung wird am Ende erreicht

## Persistenz und Recovery

- [ ] Fortschritt wird nach aussen sichtbar gespeichert
- [ ] `%APPDATA%\CleanWizard\progress.json` wird angelegt oder aktualisiert
- [ ] `progress.backup.1.json` bis `progress.backup.3.json` werden rotierend erzeugt
- [ ] Bei vorhandenem gespeicherten Stand erscheint der Wiederaufnahme-Hinweis
- [ ] Gespeicherter Stand kann fortgesetzt werden
- [ ] Gespeicherter Stand kann verworfen und neu gestartet werden
- [ ] Eine defekte `progress.json` fuehrt nicht zu einem Blocker, sondern zu Backup-Wiederherstellung

## Logging und Fehlerbilder

- [ ] `%APPDATA%\CleanWizard\logs.txt` wird bei normalen Aktionen befuellt
- [ ] Crash-Report landet bei einem fatalen Fehler unter `%APPDATA%\CleanWizard\crash.log`
- [ ] Fehlermeldungen bleiben fuer den Nutzer lesbar
- [ ] Tool-Startfehler werden im UI sichtbar gemeldet

## Funktionscheck

- [ ] Letzter Malware-Scan zeigt ein Datum oder klar „Nicht erkannt“
- [ ] Die angezeigte Scan-Quelle ist nachvollziehbar
- [ ] Tool-Launcher oeffnen die erwarteten Ziele
- [ ] Zusammenfassung zeigt Vorher/Nachher-Werte
- [ ] Log-Export erzeugt eine TXT-Datei auf dem Desktop

## Abschluss

- [ ] Keine offenen P1/P2-Findings aus dem Release-Check
- [ ] README und Changelog passen zum ausgelieferten Stand
- [ ] Release-Artefakt ist erstellt und validiert
