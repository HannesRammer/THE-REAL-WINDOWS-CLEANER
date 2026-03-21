using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Modules.WindowsTools;

public class TaskManagerAutostartStep : WizardStepBase
{
    public override string Id => "win_taskmanager_autostart";
    public override string Title => "Task-Manager Autostart";
    public override string Description => "Überprüfe den Autostart-Tab im Windows Task-Manager.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "⚙️";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Der Task-Manager ist das einfachste Windows-Werkzeug um Autostart-Programme zu verwalten. " +
        "Er zeigt auch den 'Startauswirkung' – wie sehr jedes Programm den Start verlangsamt.";

    public override string WhatItDoes =>
        "Der Autostart-Tab im Task-Manager zeigt alle Programme die beim Windows-Start starten. " +
        "Du kannst sie dort direkt deaktivieren, ohne zusätzliche Software zu installieren.";

    public override string Risks => "Geringes Risiko – zeigt nur Benutzer-Autostart, nicht kritische Systemdienste.";

    public override string WhatNotToDo =>
        "• Programme mit 'Windows' im Namen und hoher Startauswirkung trotzdem NICHT deaktivieren wenn unklar";

    public override string RecommendedApproach =>
        "1. Strg+Umschalt+Esc (Task-Manager öffnen)\n" +
        "2. Tab 'Autostart' auswählen\n" +
        "3. Spalte 'Startauswirkung' sortieren\n" +
        "4. Programme mit hoher Auswirkung prüfen\n" +
        "5. Nicht benötigte Programme: Rechtsklick → Deaktivieren";

    public override string SimpleExplanation =>
        "Du öffnest eine Liste aller Programme die beim PC-Start sofort losstarten. " +
        "Die Liste zeigt dir auch wie sehr jedes Programm deinen PC bremst – praktisch!";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "taskmgr",
            Label = "Task-Manager öffnen",
            Description = "Autostart-Tab im Task-Manager prüfen",
            ActionType = StepToolActionType.Executable,
            Target = "taskmgr.exe"
        },
        new()
        {
            Id = "startupapps_settings",
            Label = "Start-Apps in Einstellungen",
            Description = "Alternative Ansicht der Autostarts",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:startupapps"
        }
    };
}

public class DiskCleanupStep : WizardStepBase
{
    public override string Id => "win_disk_cleanup";
    public override string Title => "Datenträgerbereinigung";
    public override string Description => "Bereinige temporäre Dateien und gib Speicherplatz frei.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🗑️";
    public override int ScoreValue => 10;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Windows sammelt im Laufe der Zeit viele temporäre Dateien, Thumbnails, " +
        "alte Windows-Updates und Papierkorb-Inhalte. Diese belegen Speicherplatz " +
        "und können die Performance beeinflussen.";

    public override string WhatItDoes =>
        "Die Datenträgerbereinigung entfernt sicher:\n" +
        "• Temporäre Internetdateien\n" +
        "• Temporäre Windows-Dateien\n" +
        "• Thumbnail-Cache\n" +
        "• Papierkorb\n" +
        "• Alte Windows Update-Dateien (mit Admin-Option)";

    public override string Risks =>
        "Sehr geringes Risiko. Die Daten die bereinigt werden sind nur temporäre Dateien.";

    public override string WhatNotToDo =>
        "• Nicht unter 'Systemdateien bereinigen' die 'Windows-Installationsdateien' löschen, wenn du noch downgraden möchtest";

    public override string RecommendedApproach =>
        "1. Datenträgerbereinigung öffnen (cleanmgr.exe)\n" +
        "2. Laufwerk C: auswählen\n" +
        "3. 'Systemdateien bereinigen' für mehr Optionen\n" +
        "4. Alle Kästchen ankreuzen (außer du willst Rollback behalten)\n" +
        "5. OK → Löschen bestätigen";

    public override string SimpleExplanation =>
        "Das ist wie Staub saugen bei deinem PC – alte Krümel und Dreck werden weggefegt. " +
        "Dein PC wird ein bisschen aufgeräumter!";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "cleanmgr",
            Label = "Datenträgerbereinigung öffnen",
            Description = "Klassische Bereinigung starten",
            ActionType = StepToolActionType.Executable,
            Target = "cleanmgr.exe"
        },
        new()
        {
            Id = "storage_settings",
            Label = "Speicher-Einstellungen",
            Description = "Storage Sense und Speicherübersicht",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:storagesense"
        }
    };
}

public class StorageOptimizationStep : WizardStepBase
{
    public override string Id => "win_storage_optimization";
    public override string Title => "Speicheroptimierung (Defrag / TRIM)";
    public override string Description => "Optimiere deinen Datenträger – Defragmentierung für HDD, TRIM für SSD.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "💾";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Bei HDDs: Fragmentierte Dateien verlangsamen die Leserate. Defragmentierung ordnet sie neu an.\n" +
        "Bei SSDs: TRIM teilt dem Laufwerk mit welche Blöcke gelöscht wurden, für bessere Performance.";

    public override string WhatItDoes =>
        "Windows analysiert und optimiert automatisch das richtige Verfahren je nach Laufwerkstyp:\n" +
        "• HDD → Defragmentierung\n" +
        "• SSD → TRIM-Befehl\n" +
        "• Läuft normalerweise automatisch im Hintergrund";

    public override string Risks =>
        "WICHTIG: SSDs NIE manuell defragmentieren (nur TRIM)! " +
        "Windows erkennt den Typ automatisch.";

    public override string WhatNotToDo =>
        "• SSD NIEMALS mit alten Defragmentierungstools bearbeiten\n" +
        "• Defragmentierung nicht während der Arbeit ausführen";

    public override string RecommendedApproach =>
        "1. 'Laufwerke optimieren' öffnen (dfrgui.exe)\n" +
        "2. Status prüfen – wann war die letzte Optimierung?\n" +
        "3. 'Optimieren' klicken\n" +
        "4. Oder: Sicherstellen dass der geplante Task aktiv ist";

    public override string SimpleExplanation =>
        "Stell dir vor, dein Schreibtisch (Festplatte) ist unordentlich. " +
        "Windows räumt ihn auf, damit du schneller findest was du suchst!";

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
            Description = "Defrag/TRIM-Tool öffnen",
            ActionType = StepToolActionType.Executable,
            Target = "dfrgui.exe"
        }
    };
}

public class TroubleshootingStep : WizardStepBase
{
    public override string Id => "win_troubleshooting";
    public override string Title => "Windows Problembehandlung";
    public override string Description => "Nutze die Windows-integrierten Problembehandlungstools.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🔧";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Windows hat viele eingebaute Diagnose- und Reparaturtools die häufige Probleme " +
        "automatisch erkennen und beheben können.";

    public override string WhatItDoes =>
        "Diagnosetools:\n" +
        "• SFC (System File Checker): Prüft und repariert Systemdateien\n" +
        "• DISM: Repariert das Windows-Image\n" +
        "• Windows Problembehandlung: Automatische Diagnose";

    public override string Risks =>
        "Sehr geringes Risiko. Diese Tools können nur reparieren, nicht beschädigen.";

    public override string WhatNotToDo =>
        "• SFC/DISM nicht im laufenden System bei kritischen Fehlern – lieber aus der Recovery ausführen";

    public override string RecommendedApproach =>
        "1. Einstellungen → System → Problembehandlung öffnen\n" +
        "2. Oder: sfc /scannow in Admin-Eingabeaufforderung\n" +
        "3. DISM: DISM /Online /Cleanup-Image /RestoreHealth";

    public override string SimpleExplanation =>
        "Windows hat einen eingebauten Arzt! Der schaut nach ob alles in Ordnung ist " +
        "und repariert kleine Wunden automatisch.";

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
            Description = "Windows Problembehandlung in den Einstellungen",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:troubleshoot"
        },
        new()
        {
            Id = "cmd_admin",
            Label = "CMD als Admin",
            Description = "Für SFC/DISM-Befehle",
            ActionType = StepToolActionType.Executable,
            Target = "cmd.exe"
        }
    };
}

public class DriverUpdateStep : WizardStepBase
{
    public override string Id => "win_driver_update";
    public override string Title => "Treiberupdates prüfen";
    public override string Description => "Prüfe ob alle Treiber aktuell sind.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🔄";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => false;

    public override string WhyImportant =>
        "Veraltete Treiber können zu Instabilität, schlechter Performance und Sicherheitslücken führen. " +
        "Besonders Grafiktreiber sollten aktuell sein.";

    public override string WhatItDoes =>
        "Du prüfst ob Treiber-Updates verfügbar sind und installierst sie wenn nötig.";

    public override string Risks =>
        "Mittleres Risiko: Neue Treiber können manchmal Probleme verursachen. " +
        "Vorher einen Wiederherstellungspunkt erstellen!";

    public override string WhatNotToDo =>
        "• NICHT alle Treiber auf einmal aktualisieren\n" +
        "• NICHT über dubiose Drittanbieter-Software (z.B. 'Driver Booster') aktualisieren\n" +
        "• Immer direkt vom Gerätehersteller";

    public override string RecommendedApproach =>
        "1. Geräte-Manager öffnen (devmgmt.msc)\n" +
        "2. Nach gelben Ausrufezeichen suchen\n" +
        "3. Windows Update → Optionale Updates → Treiberupdates prüfen\n" +
        "4. Grafiktreiber: Direkt von NVIDIA/AMD/Intel Website\n" +
        "5. Vorher: Systemwiederherstellungspunkt erstellen!";

    public override string SimpleExplanation =>
        "Treiber sind wie Übersetzer zwischen Windows und deiner Hardware. " +
        "Veraltete Übersetzer können Missverständnisse verursachen – " +
        "neue Versionen 'sprechen' besser miteinander!";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "devmgmt",
            Label = "Geräte-Manager öffnen",
            Description = "Treiberstatus und Warnsymbole prüfen",
            ActionType = StepToolActionType.Executable,
            Target = "devmgmt.msc"
        },
        new()
        {
            Id = "optional_updates",
            Label = "Optionale Updates",
            Description = "Treiberupdates in Windows Update",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate-optionalupdates"
        }
    };
}

public class WindowsUpdateStep : WizardStepBase
{
    public override string Id => "win_update";
    public override string Title => "Windows Update";
    public override string Description => "Stelle sicher dass alle Windows-Updates installiert sind.";
    public override string Category => "Windows-Tools";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🪟";
    public override int ScoreValue => 5;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Windows Updates enthalten wichtige Sicherheitspatches und Fehlerkorrekturen. " +
        "Ein nicht gepatchtes System ist anfällig für bekannte Angriffe.";

    public override string WhatItDoes =>
        "Windows Update sucht und installiert:\n" +
        "• Sicherheitsupdates\n" +
        "• Qualitätsupdates\n" +
        "• Feature Updates";

    public override string Risks =>
        "Geringes Risiko. Manchmal können Updates temporär Probleme verursachen, " +
        "aber das Risiko von ungepatchten Sicherheitslücken ist größer.";

    public override string WhatNotToDo =>
        "• Updates nicht dauerhaft deaktivieren\n" +
        "• Nicht zu lange warten – spätestens alle 2 Wochen prüfen";

    public override string RecommendedApproach =>
        "1. Einstellungen → Windows Update öffnen\n" +
        "2. 'Nach Updates suchen' klicken\n" +
        "3. Alle verfügbaren Updates installieren\n" +
        "4. PC neu starten wenn gefordert";

    public override string SimpleExplanation =>
        "Windows bekommt regelmäßig Verbesserungen und Sicherheitsreparaturen von Microsoft. " +
        "Das ist so wie ein Impfschutz für deinen PC!";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "windows_update",
            Label = "Windows Update öffnen",
            Description = "Update-Suche direkt starten",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate"
        },
        new()
        {
            Id = "update_history",
            Label = "Updateverlauf",
            Description = "Installationsverlauf prüfen",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:windowsupdate-history"
        }
    };
}

public class WindowsToolsModule : IWizardModule
{
    public string Id => "windows_tools";
    public string Name => "Windows-Bordmittel";
    public string Description => "Optimiere Windows mit den integrierten Tools – kein zusätzlicher Download nötig.";
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
