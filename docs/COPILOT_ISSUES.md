# Copilot Issue Backlog

Diese Liste ist so geschrieben, dass jeder Block direkt als GitHub-Issue genutzt werden kann.

## 1) [P1] User-Feedback bei Tool-Launch-Fehlern in der UI
**Ziel**  
Wenn `OpenUrl`, `OpenSettings`, `OpenFolder` oder `LaunchExecutable` fehlschlagen, soll der Nutzer eine klare Fehlermeldung in der Oberfläche sehen.

**Scope**  
- `ToolLauncherService` liefert Ergebnisobjekt (`success/errorMessage`) statt nur Logging.  
- `WizardViewModel` zeigt Fehlertext in einer Statusbox.
- Keine MessageBox-Spam, nur ruhige Inline-Meldung.

**Akzeptanzkriterien**  
- Fehlerfälle sind sichtbar im Wizard.  
- Logs bleiben erhalten.  
- Build + Tests grün.

## 2) [P1] Screenshot-/Asset-Bereiche pro Modul sichtbar machen
**Ziel**  
Der Wizard soll pro Modul Platzhalterbilder/Screenshots zeigen können.

**Scope**  
- `Assets/` strukturieren (`autoruns`, `malwarebytes`, `windows-tools`).  
- Pro Schritt optional `ImagePath`.  
- UI rendert Bildbereich nur wenn vorhanden.

**Akzeptanzkriterien**  
- Mindestens 1 Platzhalterbild pro Modul angezeigt.  
- Kein Crash bei fehlendem Bildpfad.

## 3) [P1] Vorher/Nachher-Vergleich in Summary verbessern
**Ziel**  
Vorher/Nachher-Kennzahlen klarer und konsistent in der Zusammenfassung anzeigen.

**Scope**  
- Snapshot vor Wizardstart und nach Abschluss speichern.  
- Karten/Balken für Autostart-Anzahl, Speicher, Prozesse.  
- Prozent-/Trendmarkierung.

**Akzeptanzkriterien**  
- Summary zeigt beide Werte + Differenz.  
- Werte bleiben über Neustart konsistent.

## 4) [P1] Notfallmodus-Flow als eigener geführter Pfad
**Ziel**  
Wenn System kritisch ist, nur Quick-Fix-Reihenfolge mit minimalen Schritten.

**Scope**  
- Notfall-Preset (gefilterte Schrittliste).  
- Deutliche Kennzeichnung im UI.  
- Wechsel zurück in Normalmodus möglich.

**Akzeptanzkriterien**  
- Kritische Systeme landen im Quick-Fix-Flow.  
- Keine Experten-Überladung im Notfallmodus.

## 5) [P2] Tests für Progress-Backup-Rotation und Fallback
**Ziel**  
Sicherstellen, dass `progress.json` + `backup.1..3` robust funktionieren.

**Scope**  
- Testbarer Dateipfad für `JsonProgressService` (DI/Options).  
- Unit-Tests für Rotation, beschädigte Hauptdatei, Fallback-Reihenfolge.

**Akzeptanzkriterien**  
- Tests simulieren defekte `progress.json` und laden korrekt aus Backup.  
- Maximal 3 Backup-Dateien bleiben erhalten.

## 6) [P2] Logging-Export in UI integrieren
**Ziel**  
Nutzer soll Log-Datei direkt exportieren können.

**Scope**  
- Button in Summary: `Log exportieren`.  
- Zeitstempel und Ereignistyp im Export.

**Akzeptanzkriterien**  
- Export-Datei wird erstellt und enthält Schritt-/Tool-Ereignisse.

## 7) [P2] Safety-Checkliste für riskante Schritte erweitern
**Ziel**  
High/Critical-Schritte nicht nur bestätigen, sondern kurz prüfen lassen.

**Scope**  
- 2-3 Pflicht-Checkboxen (z. B. „Backup gemacht“, „Eintrag verstanden“).  
- Erst danach `Erledigt` aktiv.

**Akzeptanzkriterien**  
- Ohne Checkliste bleibt `Erledigt` disabled.  
- Zustand wird im Progress gespeichert.

## 8) [P2] Modul-Fortschritt klickbar machen
**Ziel**  
In Sidebar auf Modul klicken und zum ersten offenen Schritt springen.

**Scope**  
- Modul-Klickcommand in `MainViewModel`.  
- Mapping Modul -> erster Pending-Schritt.

**Akzeptanzkriterien**  
- Klick navigiert korrekt.  
- Kein Effekt wenn Modul bereits vollständig abgeschlossen.

## 9) [P3] README auf „Release Ready“ erweitern
**Ziel**  
Vollständige Projekt-Doku für neue Contributor.

**Scope**  
- Architekturdiagramm (kurz), Build/Test, Feature-Flags, Grenzen der App.  
- „Die App automatisiert keine riskanten Systemeingriffe“-Hinweis prominent.

**Akzeptanzkriterien**  
- Neuer Entwickler kann Repo klonen, bauen, starten, verstehen.

## 10) [P3] Optionaler Installer-Workflow (MSIX oder Self-contained ZIP)
**Ziel**  
Einfachere Verteilung für Nicht-Entwickler.

**Scope**  
- Release-Workflow mit Build-Artefakt.  
- Signierung optional ausklammern.

**Akzeptanzkriterien**  
- GitHub Action erzeugt installierbares Artefakt.
