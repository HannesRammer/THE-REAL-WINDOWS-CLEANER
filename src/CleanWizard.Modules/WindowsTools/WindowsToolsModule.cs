using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Modules.WindowsTools;

public class TaskManagerAutostartStep : WizardStepBase
{
    public override string Id => "win_taskmanager_autostart";
    public override string Title => "Autostart im Task-Manager";
    public override string Description => "Prüfe, welche Programme Windows direkt beim Start lädt.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "⚙️";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Der Task-Manager ist der einfachste Weg, um zusätzliche Startprogramme zu bewerten.";

    public override string WhatItDoes =>
        "Du siehst die Startprogramme, ihre Auswirkung und kannst unnötige Einträge direkt deaktivieren.";

    public override string Risks => "Geringes Risiko. Trotzdem solltest du Einträge nur ändern, wenn du sie zuordnen kannst.";

    public override string WhatNotToDo =>
        "• Unklare Programme nicht nur wegen hoher Auswirkung deaktivieren";

    public override string RecommendedApproach =>
        "1. Task-Manager öffnen\n" +
        "2. Tab 'Autostart' auswählen\n" +
        "3. Nach Startauswirkung sortieren\n" +
        "4. Nur bekannte Zusatzprogramme prüfen\n" +
        "5. Unnötige Einträge deaktivieren";

    public override string SimpleExplanation =>
        "Hier siehst du, was beim Start automatisch mitläuft und den PC bremsen kann.";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "taskmgr",
            Label = "Task-Manager öffnen",
            Description = "Öffnet den Task-Manager für die Autostart-Prüfung",
            ActionType = StepToolActionType.Executable,
            Target = "taskmgr.exe"
        },
        new()
        {
            Id = "startupapps_settings",
            Label = "Start-Apps in den Einstellungen",
            Description = "Öffnet die vereinfachte Windows-Ansicht für Startprogramme",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:startupapps"
        }
    };
}

public class DiskCleanupStep : WizardStepBase
{
    public override string Id => "win_disk_cleanup";
    public override string Title => "Datenträgerbereinigung";
    public override string Description => "Entferne temporäre Dateien und gib Speicherplatz frei.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🗑️";
    public override int ScoreValue => 10;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Temporäre Dateien wachsen mit der Zeit an und belegen unnötig Speicherplatz.";

    public override string WhatItDoes =>
        "Die Datenträgerbereinigung entfernt temporäre Dateien, Cache-Inhalte und auf Wunsch alte Update-Reste.";

    public override string Risks => "Geringes Risiko. Prüfe nur bewusst, ob du alte Windows-Dateien für eine Rückkehr benötigst.";

    public override string WhatNotToDo =>
        "• Alte Windows-Installationsdateien nicht löschen, wenn du noch eine Rückkehr zur vorherigen Version brauchst";

    public override string RecommendedApproach =>
        "1. Datenträgerbereinigung öffnen\n" +
        "2. Laufwerk C: auswählen\n" +
        "3. Bei Bedarf 'Systemdateien bereinigen' wählen\n" +
        "4. Nur die passenden Bereiche auswählen\n" +
        "5. Bereinigung bestätigen";

    public override string SimpleExplanation =>
        "Hier räumst du alte Restdateien weg, die nur Platz belegen.";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "cleanmgr",
            Label = "Datenträgerbereinigung öffnen",
            Description = "Öffnet die klassische Datenträgerbereinigung",
            ActionType = StepToolActionType.Executable,
            Target = "cleanmgr.exe"
        },
        new()
        {
            Id = "storage_settings",
            Label = "Speicher-Einstellungen",
            Description = "Öffnet Speicherübersicht und Speicheroptimierung",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:storagesense"
        }
    };
}

public class StorageOptimizationStep : WizardStepBase
{
    public override string Id => "win_storage_optimization";
    public override string Title => "Laufwerke optimieren";
    public override string Description => "Prüfe, ob Windows die Laufwerke korrekt optimiert.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "💾";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Windows verwendet je nach Laufwerkstyp unterschiedliche Optimierungen. Ein kurzer Check zeigt, ob alles normal läuft.";

    public override string WhatItDoes =>
        "Für HDDs wird Defragmentierung genutzt, für SSDs TRIM. Windows erledigt das meist automatisch.";

    public override string Risks =>
        "Geringes Risiko, solange du Windows die passende Optimierung automatisch wählen lässt.";

    public override string WhatNotToDo =>
        "• Keine alten Drittanbieter-Defrag-Tools für SSDs verwenden";

    public override string RecommendedApproach =>
        "1. 'Laufwerke optimieren' öffnen\n" +
        "2. Letzte Optimierung prüfen\n" +
        "3. Nur bei Bedarf manuell anstoßen";

    public override string SimpleExplanation =>
        "Windows hält die Laufwerke im Hintergrund in Ordnung. Hier prüfst du nur, ob das funktioniert.";

    public override string ExpertDetails =>
        "TRIM-Befehl manuell prüfen:\n" +
        "fsutil behavior query DisableDeleteNotify\n" +
        "Ergebnis 0 = TRIM aktiv (gut)\n" +
        "Ergebnis 1 = TRIM deaktiviert\n\n" +
        "TRIM aktivieren: fsutil behavior set DisableDeleteNotify 0";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "dfrgui",
            Label = "Laufwerke optimieren",
            Description = "Öffnet die Windows-Oberfläche für Defrag und TRIM",
            ActionType = StepToolActionType.Executable,
            Target = "dfrgui.exe"
        }
    };
}

public class TroubleshootingStep : WizardStepBase
{
    public override string Id => "win_troubleshooting";
    public override string Title => "Problembehandlung";
    public override string Description => "Nutze die integrierten Windows-Hilfen bei Fehlern oder ungewöhnlichem Verhalten.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🔧";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Die Windows-Bordmittel helfen bei typischen Systemproblemen, ohne zusätzliche Software zu installieren.";

    public override string WhatItDoes =>
        "Du öffnest die Windows-Problembehandlung oder startest bei Bedarf weitergehende Reparaturbefehle.";

    public override string Risks =>
        "Sehr geringes Risiko. Diese Tools können nur reparieren, nicht beschädigen.";

    public override string WhatNotToDo =>
        "• SFC und DISM nicht ohne Anlass oder ohne passende Rechte starten";

    public override string RecommendedApproach =>
        "1. Zuerst die Windows-Problembehandlung öffnen\n" +
        "2. Nur bei Bedarf SFC oder DISM nutzen";

    public override string SimpleExplanation =>
        "Windows bringt eigene Hilfen mit, um typische Probleme selbst zu prüfen und teils auch zu beheben.";

    public override string ExpertDetails =>
        "Vollständige Reparaturreihenfolge:\n" +
        "1. DISM /Online /Cleanup-Image /CheckHealth\n" +
        "2. DISM /Online /Cleanup-Image /ScanHealth\n" +
        "3. DISM /Online /Cleanup-Image /RestoreHealth\n" +
        "4. sfc /scannow\n\n" +
        "Logs unter: C:\\Windows\\Logs\\CBS\\CBS.log";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "troubleshoot_settings",
            Label = "Problembehandlung öffnen",
            Description = "Öffnet die Problembehandlung in den Einstellungen",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:troubleshoot"
        },
        new()
        {
            Id = "cmd_admin",
            Label = "Eingabeaufforderung öffnen",
            Description = "Öffnet die Eingabeaufforderung für SFC- oder DISM-Befehle",
            ActionType = StepToolActionType.Executable,
            Target = "cmd.exe"
        }
    };
}

public class DriverUpdateStep : WizardStepBase
{
    public override string Id => "win_driver_update";
    public override string Title => "Treiberupdates prüfen";
    public override string Description => "Prüfe nur die Treiber, bei denen wirklich Handlungsbedarf besteht.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🔄";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Veraltete oder fehlerhafte Treiber können zu Abstürzen, Darstellungsfehlern oder Leistungsproblemen führen.";

    public override string WhatItDoes => "Du prüfst den Gerätestatus und nutzt bevorzugt Windows Update oder den Gerätehersteller.";

    public override string Risks =>
        "Mittleres Risiko. Neue Treiber können neue Probleme verursachen.";

    public override string WhatNotToDo =>
        "• Nicht alle Treiber gleichzeitig aktualisieren\n" +
        "• Keine dubiosen Treiber-Updater verwenden\n" +
        "• Im Zweifel direkt beim Hersteller prüfen";

    public override string RecommendedApproach =>
        "1. Geräte-Manager öffnen\n" +
        "2. Nach gelben Ausrufezeichen suchen\n" +
        "3. Optionale Treiberupdates in Windows Update prüfen\n" +
        "4. Grafiktreiber nur bei Bedarf direkt vom Hersteller laden";

    public override string SimpleExplanation =>
        "Treiber verbinden Windows mit der Hardware. Wenn einer Probleme macht, lohnt sich eine gezielte Aktualisierung.";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "devmgmt",
            Label = "Geräte-Manager öffnen",
            Description = "Öffnet den Geräte-Manager zur Prüfung von Warnsymbolen",
            ActionType = StepToolActionType.Executable,
            Target = "devmgmt.msc"
        },
        new()
        {
            Id = "optional_updates",
            Label = "Optionale Updates",
            Description = "Öffnet optionale Treiberupdates in Windows Update",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate-optionalupdates"
        }
    };
}

public class WindowsUpdateStep : WizardStepBase
{
    public override string Id => "win_update";
    public override string Title => "Windows Update";
    public override string Description => "Prüfe, ob wichtige Windows-Updates fehlen.";
    public override string Category => "Windows-Werkzeuge";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🪟";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Aktuelle Windows-Updates schließen bekannte Sicherheitslücken und beheben viele Alltagsfehler.";

    public override string WhatItDoes =>
        "Windows Update prüft Sicherheits-, Qualitäts- und optionale Systemupdates.";

    public override string Risks =>
        "Geringes Risiko. Größere Updates sollten trotzdem nicht in einem ungünstigen Moment gestartet werden.";

    public override string WhatNotToDo =>
        "• Updates nicht dauerhaft aussetzen\n" +
        "• Sicherheitsupdates nicht lange aufschieben";

    public override string RecommendedApproach =>
        "1. Windows Update öffnen\n" +
        "2. Nach Updates suchen\n" +
        "3. Alle verfügbaren Updates installieren\n" +
        "4. Windows bei Bedarf neu starten";

    public override string SimpleExplanation =>
        "Updates halten Windows sicher und zuverlässig. Hier prüfst du, ob dein System auf aktuellem Stand ist.";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "windows_update",
            Label = "Windows Update öffnen",
            Description = "Öffnet Windows Update für die Prüfung auf neue Updates",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate"
        },
        new()
        {
            Id = "update_history",
            Label = "Updateverlauf",
            Description = "Öffnet den Verlauf der zuletzt installierten Updates",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate-history"
        }
    };
}

public class WindowsToolsModule : IWizardModule
{
    public string Id => "windows_tools";
    public string Name => "Windows-Werkzeuge";
    public string Description => "Nutze die integrierten Windows-Funktionen für Updates, Speicher und einfache Wartung.";
    public string Icon => "🪟";
    public int Order => 3;

    public IReadOnlyList<IStep> Steps { get; }

    public WindowsToolsModule()
    {
        Steps = new List<IStep>
        {
            new WindowsUpdateStep(),
            new TaskManagerAutostartStep(),
            new DiskCleanupStep(),
            new StorageOptimizationStep(),
            new TroubleshootingStep(),
            new DriverUpdateStep()
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;
}
