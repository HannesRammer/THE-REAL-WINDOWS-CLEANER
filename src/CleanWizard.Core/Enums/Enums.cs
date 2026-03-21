namespace CleanWizard.Core.Enums;

public enum StepStatus
{
    Pending,
    Completed,
    Skipped,
    Later
}

public enum StepDifficulty
{
    Easy,
    Medium,
    Advanced
}

public enum StepRiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum AppTheme
{
    Light,
    Dark
}

public enum ExpertMode
{
    Simple,
    Expert
}

public enum StepActionType
{
    OpenUrl,
    OpenSettings,
    OpenFolder
}

public enum StepActionPriority
{
    Primary,
    Secondary
}
