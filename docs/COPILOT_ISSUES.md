# Copilot Issue Backlog

Diese Liste enthält den finalen Umsetzungsstand.

## Status

- Erledigt: Last-Malware-Scan mit Fallback-Kette und Quellenanzeige im System-Check.
- Erledigt: Smoke-Tests für Wizard-Navigation und Expertenmodus-Sichtbarkeit.
- Erledigt: Schrittabhängige Tool-Sets priorisiert (Primary sichtbar zuerst, Secondary darunter).
- Erledigt: Vorher/Nachher-Mini-Balkendiagramme vollständig verdrahtet (CPU, Autostart, RAM, freier Speicher).

## Erledigt

## 1) [x] Malware-Scan-Erkennung robuster machen
**Umgesetzt**
- Fallback-Kette: Defender-Eventlog, Defender-Registry, Malwarebytes-Reports.
- Quelle im System-Check wird angezeigt.
- Wenn nichts gefunden wird, bleibt der Zustand klar leer bzw. „Nicht erkannt“.

## 4) [x] UI-Tests für Wizard-Flows
**Umgesetzt**
- Smoke-Tests für Weiter, Zurück, Überspringen, Später und Erledigt sind vorhanden.
- Expertenmodus-Sichtbarkeit ist ebenfalls abgesichert.

## 2) [x] Schrittabhängige Tool-Sets finalisiert
**Umgesetzt**
- Im Wizard ist die Primäraktion pro Schritt als „Empfohlen“ sichtbar und steht immer zuerst.
- Weitere Aktionen werden als Secondary-Aktionen darunter dargestellt.
- Testabdeckung prüft: erste Aktion = `Primary`, alle weiteren = `Secondary`.

## 3) [x] Vorher/Nachher-Mini-Balkendiagramme finalisiert
**Umgesetzt**
- Prozentwerte für CPU, Autostart, freien Speicher und genutzten RAM werden im Summary-ViewModel berechnet.
- Balken zeigen pro Metrik Vorher/Nachher mit klarer Richtung.
- Bestehende Textwerte bleiben erhalten.
