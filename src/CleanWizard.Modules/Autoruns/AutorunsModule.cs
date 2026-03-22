using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Modules.Autoruns;

internal static class AutorunsActions
{
    public static readonly StepAction Download = new(
        "Autoruns öffnen", "⬇️", StepActionType.OpenUrl,
        "https://learn.microsoft.com/sysinternals/downloads/autoruns", StepActionPriority.Primary);

    public static readonly StepAction AutostartSettings = new(
        "Autostart-Einstellungen", "⚙️", StepActionType.OpenSettings,
        "ms-settings:startupapps", StepActionPriority.Secondary);
}

public class AutorunsOverviewStep : WizardStepBase
{
    public override string Id => "autoruns_overview";
    public override string Title => "Autoruns vorbereiten";
    public override string Description => "Prüfe, ob Autoruns verfügbar ist, und öffne das Tool aus der App heraus.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Easy;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Low;
    public override string Icon => "🚀";
    public override int ScoreValue => 15;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Autoruns zeigt deutlich mehr Startpunkte als der Task-Manager. Damit erkennst du schneller, was beim Systemstart mitläuft.";

    public override string WhatItDoes =>
        "Du prüfst den Installationsstatus, installierst Autoruns bei Bedarf und öffnest anschließend direkt das Tool.";

    public override string Risks =>
        "Das reine Prüfen und Öffnen ist unkritisch. Änderungen an Systemeinträgen solltest du erst im nächsten Schritt vornehmen.";

    public override string WhatNotToDo =>
        "• Nicht sofort Einträge deaktivieren\n" +
        "• Nicht pauschal alles als unnötig einstufen";

    public override string RecommendedApproach =>
        "1. Status prüfen\n" +
        "2. Autoruns bei Bedarf installieren\n" +
        "3. Tool öffnen\n" +
        "4. Erst danach mit der Prüfung der Einträge beginnen";

    public override string SimpleExplanation =>
        "Autoruns zeigt dir alles, was beim Start automatisch mitläuft. So siehst du auf einen Blick, was wirklich nötig ist.";

    public override string ExpertDetails =>
        "Wichtige Bereiche in Autoruns:\n" +
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
            Id = "autoruns_check",
            Label = "Status prüfen",
            Description = "Prüft, ob Autoruns bereits verfügbar ist",
            ActionType = StepToolActionType.CheckInstalled,
            Target = "autoruns"
        },
        new()
        {
            Id = "autoruns_install",
            Label = "Autoruns installieren",
            Description = "Installiert Autoruns per winget, mit offizieller Download-Seite als Fallback",
            ActionType = StepToolActionType.InstallPackage,
            Target = "Microsoft.Sysinternals.Autoruns",
            Arguments = "autoruns|https://learn.microsoft.com/sysinternals/downloads/autoruns"
        },
        new()
        {
            Id = "autoruns_open",
            Label = "Autoruns öffnen",
            Description = "Startet Autoruns, wenn das Tool installiert ist",
            ActionType = StepToolActionType.Executable,
            Target = "autoruns64.exe"
        },
        new()
        {
            Id = "autoruns_page",
            Label = "Offizielle Seite öffnen",
            Description = "Öffnet die Microsoft-Seite zu Autoruns im Browser",
            ActionType = StepToolActionType.Url,
            Target = "https://learn.microsoft.com/sysinternals/downloads/autoruns"
        },
        new()
        {
            Id = "startup_settings",
            Label = "Windows-Start-Apps öffnen",
            Description = "Öffnet die vereinfachte Windows-Ansicht für Autostart-Programme",
            ActionType = StepToolActionType.SettingsUri,
            Target = "ms-settings:startupapps"
        }
    };
}

public class AutorunsScanStep : WizardStepBase
{
    public override string Id => "autoruns_scan";
    public override string Title => "Autostart prüfen";
    public override string Description => "Gehe die Einträge geordnet durch und markiere nur klar erkennbare Kandidaten.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🔍";
    public override int ScoreValue => 15;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Ein geordneter Blick auf Autostart-Einträge spart Startzeit und reduziert unnötige Hintergrundprozesse.";

    public override string WhatItDoes =>
        "Du sichtest die relevanten Einträge und trennst unkritische Zusatzprogramme von systemnahen Komponenten.";

    public override string Risks =>
        "Wenn du falsche Einträge deaktivierst, können Programme oder Treiber Probleme machen.";

    public override string WhatNotToDo =>
        "• Keine Einträge von Microsoft deaktivieren, wenn du dir nicht sicher bist\n" +
        "• Keine Sicherheitssoftware deaktivieren\n" +
        "• Keine Bereiche wie Winlogon oder Boot Execute anfassen";

    public override string RecommendedApproach =>
        "1. Nach Hersteller und Nutzen sortieren\n" +
        "2. Nur Zusatzprogramme prüfen, die du kennst\n" +
        "3. Unklare Einträge lieber stehen lassen\n" +
        "4. Änderungen einzeln und nachvollziehbar durchführen";

    public override string SimpleExplanation =>
        "Du entscheidest, welche Zusatzprogramme wirklich sofort starten müssen und welche erst später gebraucht werden.";

    public override string ExpertDetails =>
        "Zusätzliche Hinweise:\n" +
        "• Einträge mit rotem Hintergrund in Autoruns = Datei nicht vorhanden\n" +
        "• Solche verwaisten Einträge sind meist unkritisch\n" +
        "• VirusTotal-Integration in Autoruns: Rechtsklick → Check VirusTotal\n" +
        "• Einträge ohne digitale Signatur sind verdächtig\n\n" +
        "Browser-Erweiterungen prüfen:\n" +
        "• Unbekannte Add-ons sind eher Kandidaten als signierte Systemkomponenten";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "autoruns_launch",
            Label = "Autoruns öffnen",
            Description = "Öffnet Autoruns für die Prüfung der Einträge",
            ActionType = StepToolActionType.Executable,
            Target = "autoruns64.exe",
            SafetyHint = "Nur Einträge ändern, die du sicher zuordnen kannst."
        },
        new()
        {
            Id = "task_manager",
            Label = "Task-Manager öffnen",
            Description = "Öffnet die vereinfachte Windows-Sicht auf Autostart-Programme",
            ActionType = StepToolActionType.Executable,
            Target = "taskmgr.exe"
        }
    };
}

public class AutorunsCleanupStep : WizardStepBase
{
    public override string Id => "autoruns_cleanup";
    public override string Title => "Autostart aufräumen";
    public override string Description => "Deaktiviere nur klar unnötige Einträge und prüfe das Ergebnis danach.";
    public override string Category => "Autoruns";
    public override StepDifficulty Difficulty => StepDifficulty.Medium;
    public override StepRiskLevel RiskLevel => StepRiskLevel.Medium;
    public override string Icon => "🧹";
    public override int ScoreValue => 20;
    public override bool IsSimpleModeStep => true;

    public override string WhyImportant =>
        "Weniger Autostart bedeutet meist einen ruhigeren Start und weniger unnötige Last im Hintergrund.";

    public override string WhatItDoes =>
        "Du deaktivierst Einträge, die du zuvor bewusst ausgewählt hast. Deaktivierte Einträge lassen sich bei Bedarf wieder einschalten.";

    public override string Risks =>
        "Unsichere Änderungen können dazu führen, dass einzelne Programme oder Funktionen nicht mehr wie erwartet starten.";

    public override string WhatNotToDo =>
        "• Nicht löschen, sondern zuerst nur deaktivieren\n" +
        "• Nicht mehrere unklare Einträge auf einmal ändern\n" +
        "• Keine Sicherheitssoftware ausschalten";

    public override string RecommendedApproach =>
        "1. Nur bekannte Zusatzprogramme deaktivieren\n" +
        "2. Danach Windows normal starten und prüfen\n" +
        "3. Bei Problemen den letzten Eintrag wieder aktivieren\n" +
        "4. Änderungen Schritt für Schritt durchführen";

    public override string SimpleExplanation =>
        "Du schaltest nur das ab, was du nicht sofort brauchst. Wenn etwas fehlt, kannst du es wieder einschalten.";

    public override string ExpertDetails =>
        "Für tiefergehende Analyse:\n" +
        "• Änderungen vorher dokumentieren\n" +
        "• Verwaiste Einträge separat prüfen\n" +
        "• Nach größeren Änderungen Ereignisanzeige und Startverhalten beobachten";

    public override IReadOnlyList<StepToolAction> ToolActions => new List<StepToolAction>
    {
        new()
        {
            Id = "autoruns_launch_cleanup",
            Label = "Autoruns erneut öffnen",
            Description = "Öffnet Autoruns für Bereinigung und Kontrolle",
            ActionType = StepToolActionType.Executable,
            Target = "autoruns64.exe",
            SafetyHint = "Im Zweifel deaktivieren statt löschen."
        },
        new()
        {
            Id = "startup_folder",
            Label = "Autostart-Ordner",
            Description = "Öffnet den Autostart-Ordner des aktuellen Benutzers",
            ActionType = StepToolActionType.Executable,
            Target = "shell:startup"
        }
    };
}

public class AutorunsModule : IWizardModule
{
    public string Id => "autoruns";
    public string Name => "Autoruns";
    public string Description => "Prüfe und reduziere zusätzliche Autostart-Einträge mit Microsoft Autoruns.";
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
