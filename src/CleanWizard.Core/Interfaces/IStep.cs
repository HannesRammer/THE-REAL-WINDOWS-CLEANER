using CleanWizard.Core.Enums;

namespace CleanWizard.Core.Interfaces;

public interface IStep
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    string Category { get; }
    string? ImagePath { get; }
    StepDifficulty Difficulty { get; }
    StepRiskLevel RiskLevel { get; }
    string Icon { get; }
    int ScoreValue { get; }

    // Knowledge panel
    string WhyImportant { get; }
    string WhatItDoes { get; }
    string Risks { get; }
    string WhatNotToDo { get; }
    string RecommendedApproach { get; }
    string SimpleExplanation { get; }
    string ExpertDetails { get; }

    // Check-mode state
    StepStatus Status { get; set; }
    string? UserNote { get; set; }
    bool SafetyBackupConfirmed { get; set; }
    bool SafetyImpactConfirmed { get; set; }
    bool SafetyRecoveryConfirmed { get; set; }
    DateTime? CompletedAt { get; set; }
    bool IsSimpleModeStep { get; }
}
