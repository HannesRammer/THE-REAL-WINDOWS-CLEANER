using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Modules.Autoruns;

internal static class AutorunsActions
{
    public static readonly StepAction Download = new(
        "Autoruns herunterladen", "⬇️", StepActionType.OpenUrl,
        "https://learn.microsoft.com/sysinternals/downloads/autoruns", StepActionPriority.Primary);

    public static readonly StepAction AutostartSettings = new(
        "Autostart-Einstellungen", "⚙️", StepActionType.OpenSettings,
        "ms-settings:startupapps", StepActionPriority.Secondary);
}

public class AutorunsOverviewStep : WizardStepBase
{
    public override string Id => "autoruns_overview";
    public override string Title => "Autostart-Übersicht";
    public override string Description => "Verschaffe dir einen Überblick über alle Autostart-Programme mit Microsoft Autoruns.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🚀";
    public override int ScoreValue => 15;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Viele Programme tragen sich beim Start automatisch in Windows ein und verlangsamen so den PC-Start und laufen dauerhaft im Hintergrund. Mit Autoruns erhältst du eine vollständige Übersicht über ALLE Autostart-Einträge – viel mehr als der normale Task-Manager zeigt.";

    public override string WhatItDoes =>
        "Autoruns listet alle Programme auf, die sich beim Windows-Start automatisch starten. " +
        "Es zeigt Registry-Einträge, geplante Aufgaben, Browsererweiterungen, Dienste und vieles mehr. " +
        "Du kannst Einträge deaktivieren, ohne sie zu löschen.";

    public override string Risks =>
        "Geringes Risiko wenn du nur schaust. " +
        "Beim Deaktivieren von Einträgen: Systemkritische Dienste keinesfalls deaktivieren!";

    public override string WhatNotToDo =>
        "• Keine Einträge von Windows selbst deaktivieren (z.B. SecurityHealth, Windows Defender)\n" +
        "• Keine Treiberdienste deaktivieren\n" +
        "• Nie alle Einträge auf einmal deaktivieren";

    public override string RecommendedApproach =>
        "1. Autoruns von der offiziellen Microsoft-Seite herunterladen\n" +
        "2. Als Administrator starten\n" +
        "3. Tab 'Autorun-Eintrag' öffnen\n" +
        "4. Einträge nach Herausgeber sortieren\n" +
        "5. Unbekannte Einträge googeln bevor du sie deaktivierst\n" +
        "6. Verdächtige Einträge (kein gültiger Herausgeber) näher untersuchen";

    public override string SimpleExplanation =>
        "Stell dir vor, dein PC ist ein Restaurant. Autoruns zeigt dir alle Kellner, die beim Öffnen des Restaurants " +
        "sofort anfangen zu arbeiten – auch wenn du sie gar nicht gerufen hast. " +
        "Viele davon brauchst du vielleicht gar nicht!";

    public override string ExpertDetails =>
        "Registry-Pfade die Autoruns überwacht:\n" +
        "• HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run\n" +
        "• HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run\n" +
        "• HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce\n" +
        "• HKLM\\System\\CurrentControlSet\\Services\n" +
        "• Geplante Aufgaben in Task Scheduler\n" +
        "• Browser-Erweiterungen\n" +
        "• AppInit_DLLs (hochriskant wenn befüllt!)";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "autoruns_download",
            Label = "Autoruns herunterladen",
            Description = "Offizielle Microsoft Sysinternals-Quelle öffnen",
            ActionType = StepToolActionType.Url,
            Target = "https://learn.microsoft.com/sysinternals/downloads/autoruns"
        },
        new()
        {
            Id = "startup_settings",
            Label = "Autostart-Einstellungen",
            Description = "Windows Start-Apps öffnen",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:startupapps"
        }
    };
}

public class AutorunsScanStep : WizardStepBase
{
    public override string Id => "autoruns_scan";
    public override string Title => "Autostart scannen";
    public override string Description => "Scanne alle Autostart-Einträge und markiere verdächtige Programme.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🔍";
    public override int ScoreValue => 15;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Ein gezielter Scan deckt unnötige, verdächtige oder schädliche Autostart-Einträge auf. " +
        "Jedes deaktivierte, unnötige Programm spart Startzeit und RAM.";

    public override string WhatItDoes =>
        "Du gehst jeden Tab in Autoruns durch und überprüfst die Einträge systematisch. " +
        "Typische Kandidaten zum Deaktivieren: Cloud-Clients, Spiele-Launcher, Update-Dienste von Software die du selten nutzt.";

    public override string Risks =>
        "Mittleres Risiko: Wenn du falsche Einträge deaktivierst, kann Software nicht mehr funktionieren. " +
        "Im schlimmsten Fall muss Windows neu gestartet und der Eintrag reaktiviert werden.";

    public override string WhatNotToDo =>
        "• Keine Einträge mit Verlag 'Microsoft Corporation' deaktivieren\n" +
        "• Keine Antivirussoftware deaktivieren\n" +
        "• Nie Einträge in 'Winlogon' oder 'Boot Execute' ändern ohne Expertenwissen";

    public override string RecommendedApproach =>
        "Typische Programme die man sicher deaktivieren kann:\n" +
        "• Spotify, Discord, Steam (nur Autostart, nicht das Programm selbst)\n" +
        "• Adobe Updater, Creative Cloud\n" +
        "• Google Update, Software-Updater von Drittanbietern\n" +
        "• OneDrive (falls nicht genutzt)\n" +
        "• Skype, Teams (falls nicht täglich genutzt)";

    public override string SimpleExplanation =>
        "Du schaust dir alle Kellner an (aus dem letzten Schritt) und sagst: " +
        "'Du und du, ihr müsst nicht sofort anfangen – ich rufe euch wenn ich euch brauche!' " +
        "Das macht das Restaurant-Öffnen viel schneller.";

    public override string ExpertDetails =>
        "Geisteinträge erkennen:\n" +
        "• Einträge mit rotem Hintergrund in Autoruns = Datei nicht vorhanden\n" +
        "• Diese 'Geistereinträge' können bedenkenlos gelöscht werden\n" +
        "• VirusTotal-Integration in Autoruns: Rechtsklick → Check VirusTotal\n" +
        "• Einträge ohne digitale Signatur sind verdächtig\n\n" +
        "Browser-Addons prüfen:\n" +
        "• Tab 'Internet Explorer' enthält auch Edge-Erweiterungen\n" +
        "• Unbekannte BHOs (Browser Helper Objects) können Malware sein";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "autoruns_launch",
            Label = "Autoruns starten",
            Description = "Versucht autoruns64.exe/autoruns.exe aus dem PATH zu starten",
            ActionType = StepToolActionType.Executable,
            Target = "autoruns64.exe",
            SafetyHint = "Nur Einträge deaktivieren, die du sicher zuordnen kannst."
        },
        new()
        {
            Id = "task_manager",
            Label = "Task-Manager öffnen",
            Description = "Alternative Sicht auf Autostart-Programme",
            ActionType = StepToolActionType.Executable,
            Target = "taskmgr.exe"
        }
    };
}

public class AutorunsCleanupStep : WizardStepBase
{
    public override string Id => "autoruns_cleanup";
    public override string Title => "Autostart bereinigen";
    public override string Description => "Deaktiviere unnötige Autostart-Einträge für eine schnellere Startzeit.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🧹";
    public override int ScoreValue => 20;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Jedes nicht benötigte Autostart-Programm verlangsamt den PC-Start und belegt dauerhaft RAM. " +
        "Eine Bereinigung kann den Start um Minuten beschleunigen.";

    public override string WhatItDoes =>
        "Du deaktivierst (nicht löschst) Einträge die du identifiziert hast. " +
        "Deaktivierte Einträge bleiben sichtbar und können jederzeit reaktiviert werden.";

    public override string Risks =>
        "Wenn du nicht sicher bist was ein Eintrag tut, lieber stehen lassen. " +
        "Fehlerhaftes Deaktivieren kann dazu führen, dass Software nicht mehr funktioniert.";

    public override string WhatNotToDo =>
        "• Einträge nicht löschen (nur deaktivieren)\n" +
        "• Kein Massendeaktivieren ohne Überprüfung\n" +
        "• Sicherheitssoftware NICHT deaktivieren";

    public override string RecommendedApproach =>
        "1. Nur Programme deaktivieren die du kennst und nicht beim Start brauchst\n" +
        "2. Nach jeder Änderung: PC neu starten und testen\n" +
        "3. Bei Problemen: Autoruns öffnen und Eintrag reaktivieren\n" +
        "4. Vorher einen Screenshot der Einträge machen als Backup";

    public override string SimpleExplanation =>
        "Du schickst die nicht benötigten Kellner nach Hause – aber gibst ihnen deine Telefonnummer, " +
        "falls du sie doch noch brauchst. Sie können jederzeit zurückkommen!";

    public override string ExpertDetails =>
        "Autoruns Backup erstellen:\n" +
        "File → Save → Autoruns-Backup.arn\n\n" +
        "Geistereinträge (rote Einträge) löschen:\n" +
        "• Rechtsklick → Delete\n" +
        "• Diese Einträge zeigen auf nicht mehr existierende Dateien\n\n" +
        "Nach der Bereinigung:\n" +
        "• Bootzeit messen mit: winsat formal\n" +
        "• Event Viewer überprüfen ob Fehler auftreten";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "autoruns_launch_cleanup",
            Label = "Autoruns erneut öffnen",
            Description = "Bereinigung und Re-Check durchführen",
            ActionType = StepToolActionType.Executable,
            Target = "autoruns64.exe",
            SafetyHint = "Nicht löschen, nur deaktivieren."
        },
        new()
        {
            Id = "startup_folder",
            Label = "Autostart-Ordner",
            Description = "Benutzer-Autostart-Ordner in Explorer öffnen",
            ActionType = StepToolActionType.Executable,
            Target = "shell:startup"
        }
    };
}

public class AutorunsModule : IWizardModule
{
    public string Id => "autoruns";
    public string Name => "Autoruns – Autostart-Analyse";
    public string Description => "Analysiere und bereinige alle Autostart-Programme mit Microsoft Sysinternals Autoruns.";
    public string Icon => "🚀";
    public int Order => 1;

    public IReadOnlyList<IStep> Steps { get; }

    public AutorunsModule()
    {
        Steps = new List<IStep>
        {
            new AutorunsOverviewStep(),
            new AutorunsScanStep(),
            new AutorunsCleanupStep()
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;
}
