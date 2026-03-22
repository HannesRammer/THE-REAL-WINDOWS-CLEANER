using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Modules;

/// <summary>
/// Basisklasse für alle Wizard-Schritte.
/// </summary>
public abstract class WizardStepBase : IStep
{
    public abstract string Id { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    public abstract string Category { get; }
    public virtual string? ImagePath => Category switch
    {
        "Autoruns" => "Assets/autoruns-placeholder.png",
        "Malwarebytes" => "Assets/malwarebytes-placeholder.png",
        "Windows-Tools" => "Assets/windows-tools-placeholder.png",
        _ => null
    };
    public abstract StepDifficulty Difficulty { get; }
    public abstract StepRiskLevel RiskLevel { get; }
    public abstract string Icon { get; }
    public abstract int ScoreValue { get; }
    public abstract string WhyImportant { get; }
    public abstract string WhatItDoes { get; }
    public abstract string Risks { get; }
    public abstract string WhatNotToDo { get; }
    public abstract string RecommendedApproach { get; }
    public abstract string SimpleExplanation { get; }
    public virtual string ExpertDetails => string.Empty;
    public virtual IReadOnlyList<StepToolAction> ToolActions => Array.Empty<StepToolAction>();
    public virtual bool IsSimpleModeStep => true;
    public virtual IReadOnlyList<StepAction> Actions =>
        ToolActions
            .Select((action, index) => new StepAction(
                action.Label,
                ActionIcon(action.ActionType),
                MapActionType(action.ActionType),
                action.Target,
                index == 0 ? StepActionPriority.Primary : StepActionPriority.Secondary))
            .ToList();

    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string? UserNote { get; set; }
    public bool SafetyBackupConfirmed { get; set; }
    public bool SafetyImpactConfirmed { get; set; }
    public bool SafetyRecoveryConfirmed { get; set; }
    public DateTime? CompletedAt { get; set; }

    private static StepActionType MapActionType(StepToolActionType actionType) => actionType switch
    {
        StepToolActionType.Url => StepActionType.OpenUrl,
        StepToolActionType.SettingsUri => StepActionType.OpenSettings,
        StepToolActionType.FolderPath => StepActionType.OpenFolder,
        StepToolActionType.Executable => StepActionType.LaunchExecutable,
        StepToolActionType.CheckInstalled => StepActionType.LaunchExecutable,
        StepToolActionType.InstallPackage => StepActionType.LaunchExecutable,
        _ => StepActionType.OpenUrl
    };

    private static string ActionIcon(StepToolActionType actionType) => actionType switch
    {
        StepToolActionType.Url => "🌐",
        StepToolActionType.SettingsUri => "⚙️",
        StepToolActionType.FolderPath => "📁",
        StepToolActionType.Executable => "🚀",
        StepToolActionType.CheckInstalled => "🔎",
        StepToolActionType.InstallPackage => "⬇️",
        _ => "▶"
    };
}
