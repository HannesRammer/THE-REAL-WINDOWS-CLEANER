# Changelog

Alle nennenswerten Änderungen an CleanWizard werden hier dokumentiert.

## [1.1.1] - 2026-03-22

### Added

- Klare Quick-Action-Priorisierung im Wizard: eine empfohlene Primäraktion oben, Secondary-Aktionen darunter.
- Testabsicherung für Primär-/Secondary-Reihenfolge bei Schritten mit mehreren Aktionen.

### Changed

- Vorher/Nachher-Metriken in der Zusammenfassung berechnen jetzt explizite Prozentwerte für Mini-Balken (CPU, Autostart, freier Speicher, RAM).

### Fixed

- Vorhandene Mini-Balken-Bindings in der Summary sind nun vollständig mit ViewModel-Properties verdrahtet.
- Doppelter CPU-Block in der Zusammenfassungsansicht entfernt.

## [1.1.0] - 2026-03-22

### Added

- Fortschritts-Speicherung mit Auto-Save, Backup-Rotation und Wiederherstellung aus bis zu drei Backup-Dateien.
- Wiederaufnahme-Dialog für gespeicherte Durchläufe beim App-Start.
- Robustere Last-Malware-Scan-Erkennung mit Quelle aus Defender oder Malwarebytes.
- Export der Zusammenfassung als TXT, JSON und Log.
- Geführter Notfallmodus sowie Sicherheitsbestätigungen für riskante Schritte.
- Smoke-Tests für Wizard-Navigation, Überspringen/Später/Erledigt und Expertenmodus-Sichtbarkeit.

### Changed

- Wizard-Schritte bieten kontextbezogene Tool-Launcher statt nur statischer Hinweise.
- System-Check und Zusammenfassung zeigen mehr Diagnose- und Vergleichsdaten.
- Modulnavigation und Schrittstatus sind stärker auf Wiederaufnahme und Fortschritt ausgelegt.

### Fixed

- Beschädigte Fortschrittsdateien blockieren den Start nicht mehr, da automatisch Backups geladen werden.
- Abstürze schreiben jetzt einen Crash-Report unter `%APPDATA%\CleanWizard\crash.log`.
- Protokollierung und Auto-Save dürfen den App-Flow nicht mehr unterbrechen.
