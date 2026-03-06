using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;

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
    public virtual bool IsSimpleModeStep => true;

    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string? UserNote { get; set; }
    public DateTime? CompletedAt { get; set; }
}
