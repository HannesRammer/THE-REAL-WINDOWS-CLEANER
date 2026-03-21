# Copilot Issue Backlog (v1.1+)

Diese Liste enthält nur noch offene Restpunkte nach der aktuellen Umsetzungsrunde.

## Status
- v1.1 Kern-Features sind implementiert (schrittbezogene Tool-Launcher, CPU-Vergleich, erweiterter System-Check, Expertenmodus-Sichtbarkeit).
- Die folgenden Issues sind bewusst für Copilot als nächste Iteration offen.

## 1) [P2] Malware-Scan-Erkennung robuster machen
**Ziel**
Last-Malware-Scan zuverlässiger erkennen (Defender + Malwarebytes), inkl. klarer Quelle im UI.

**Scope**
- Fallback-Kette: Defender-Eventlog, Malwarebytes-Reports, optional Registry.
- Quelle im System-Check anzeigen (z. B. „Defender“ oder „Malwarebytes“).

**Akzeptanzkriterien**
- Bei mindestens einer verfügbaren Quelle wird ein Datum + Quelle angezeigt.
- Wenn nichts erkennbar ist: explizit „Nicht erkannt“.

## 2) [P2] Schrittabhängige Tool-Sets weiter verfeinern
**Ziel**
Pro Schritt nur exakt relevante Aktionen und bessere Reihenfolge (Primary/Secondary).

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
- Native WPF-Visuals (keine große Zusatzbibliothek).
- CPU, Autostart, RAM, freier Speicher.

**Akzeptanzkriterien**
- Pro Metrik klare visuelle Richtung (besser/schlechter).
- Kein Verlust der bisherigen Textwerte.

## 4) [P3] UI-Tests für Wizard-Flows
**Ziel**
Stabilität gegen Regressions bei Navigation und Statuswechseln erhöhen.

**Scope**
- Smoke-Tests für: Weiter/Zurück/Überspringen/Später/Erledigt.
- Test für Expertenmodus-Sichtbarkeit.

**Akzeptanzkriterien**
- Kernnavigation ist automatisiert abgesichert.
- Änderungen an Wizard-Flow brechen Tests frühzeitig.
