# Copilot Issue Backlog

Diese Liste enthält nur noch die echten Restpunkte nach der aktuellen Umsetzungsrunde.

## Status

- Erledigt: Last-Malware-Scan mit Fallback-Kette und Quellenanzeige im System-Check.
- Erledigt: Smoke-Tests für Wizard-Navigation und Expertenmodus-Sichtbarkeit.
- Offen: Feinschliff bei schrittabhängigen Tool-Sets und deren Priorisierung.
- Offen: Vorher/Nachher-Werte als Mini-Balkendiagramme visualisieren.
- Bewusst nicht Teil dieses Hardening-Pakets: #10 und #11 bleiben offen.

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

## Offen

## 2) [P2] Schrittabhängige Tool-Sets weiter verfeinern
**Ziel**
Pro Schritt nur exakt relevante Aktionen und eine bessere Reihenfolge der Actions (Primary vor Secondary).

**Scope**
- Action-Priorisierung im UI.
- Optional kleine Icons pro Action-Type.

**Akzeptanzkriterien**
- Keine irrelevanten Buttons im Schritt.
- Primäraktion ist immer oben sichtbar.

## 3) [P3] Visualisierung Vorher/Nachher als Mini-Balkendiagramme
**Ziel**
Vergleichswerte nicht nur als Text, sondern als kleine Balken darstellen.

**Scope**
- Native WPF-Visuals, keine große Zusatzbibliothek.
- CPU, Autostart, RAM, freier Speicher.

**Akzeptanzkriterien**
- Pro Metrik klare visuelle Richtung.
- Die bisherigen Textwerte bleiben erhalten.
