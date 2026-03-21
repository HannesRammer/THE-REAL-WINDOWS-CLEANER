using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Services;

namespace CleanWizard.Core.Tests;

/// <summary>
/// Smoke-Tests für Wizard-Flows: Weiter / Zurück / Überspringen / Später / Erledigt
/// sowie Sichtbarkeit im Expertenmodus.
/// </summary>
public class WizardFlowSmokeTests
{
    // -------------------------------------------------------------------------
    // Navigation: Weiter (Next)
    // -------------------------------------------------------------------------

    [Fact]
    public void Next_AdvancesToNextStep()
    {
        var sut = BuildService("s1", "s2", "s3");

        sut.Next();

        Assert.Equal(1, sut.CurrentIndex);
        Assert.Equal("s2", sut.CurrentStep?.Id);
    }

    [Fact]
    public void Next_FiresStepChangedEvent()
    {
        var sut = BuildService("s1", "s2");
        var raised = false;
        sut.StepChanged += (_, _) => raised = true;

        sut.Next();

        Assert.True(raised);
    }

    [Fact]
    public void Next_DoesNotAdvanceBeyondLastStep()
    {
        var sut = BuildService("s1");

        sut.Next(); // already at last step

        Assert.Equal(0, sut.CurrentIndex);
        Assert.Equal("s1", sut.CurrentStep?.Id);
    }

    [Fact]
    public void CanGoNext_IsFalseOnLastStep()
    {
        var sut = BuildService("s1");

        Assert.False(sut.CanGoNext);
    }

    // -------------------------------------------------------------------------
    // Navigation: Zurück (Previous)
    // -------------------------------------------------------------------------

    [Fact]
    public void Previous_ReturnsToPreviousStep()
    {
        var sut = BuildService("s1", "s2", "s3");
        sut.Next(); // at index 1

        sut.Previous();

        Assert.Equal(0, sut.CurrentIndex);
        Assert.Equal("s1", sut.CurrentStep?.Id);
    }

    [Fact]
    public void Previous_FiresStepChangedEvent()
    {
        var sut = BuildService("s1", "s2");
        sut.Next();
        var raised = false;
        sut.StepChanged += (_, _) => raised = true;

        sut.Previous();

        Assert.True(raised);
    }

    [Fact]
    public void Previous_DoesNotGoBeforeFirstStep()
    {
        var sut = BuildService("s1", "s2");

        sut.Previous(); // already at first step

        Assert.Equal(0, sut.CurrentIndex);
        Assert.Equal("s1", sut.CurrentStep?.Id);
    }

    [Fact]
    public void CanGoPrevious_IsFalseOnFirstStep()
    {
        var sut = BuildService("s1", "s2");

        Assert.False(sut.CanGoPrevious);
    }

    // -------------------------------------------------------------------------
    // Schritt-Aktion: Überspringen (Skip)
    // -------------------------------------------------------------------------

    [Fact]
    public void Skip_MarksCurrentStepSkipped_AndAdvances()
    {
        var steps = MakeSteps("s1", "s2");
        var sut = BuildServiceFromSteps(steps);

        sut.SkipCurrentStep();

        Assert.Equal(StepStatus.Skipped, steps[0].Status);
        Assert.Equal(1, sut.CurrentIndex);
        Assert.Equal("s2", sut.CurrentStep?.Id);
    }

    [Fact]
    public void Skip_FiresStepChangedEvent()
    {
        var sut = BuildService("s1", "s2");
        var raised = false;
        sut.StepChanged += (_, _) => raised = true;

        sut.SkipCurrentStep();

        Assert.True(raised);
    }

    // -------------------------------------------------------------------------
    // Schritt-Aktion: Später (Later)
    // -------------------------------------------------------------------------

    [Fact]
    public void Later_MarksCurrentStepLater_AndAdvances()
    {
        var steps = MakeSteps("s1", "s2");
        var sut = BuildServiceFromSteps(steps);

        sut.MarkCurrentStepLater();

        Assert.Equal(StepStatus.Later, steps[0].Status);
        Assert.Equal(1, sut.CurrentIndex);
        Assert.Equal("s2", sut.CurrentStep?.Id);
    }

    [Fact]
    public void Later_FiresStepChangedEvent()
    {
        var sut = BuildService("s1", "s2");
        var raised = false;
        sut.StepChanged += (_, _) => raised = true;

        sut.MarkCurrentStepLater();

        Assert.True(raised);
    }

    // -------------------------------------------------------------------------
    // Schritt-Aktion: Erledigt (Done / Completed)
    // -------------------------------------------------------------------------

    [Fact]
    public void Done_MarkingStepCompleted_AffectsScore()
    {
        var steps = MakeSteps("s1", "s2");
        steps[0].ScoreValue = 15;
        var sut = BuildServiceFromSteps(steps);

        steps[0].Status = StepStatus.Completed;

        Assert.Equal(15, sut.CalculateScore());
    }

    [Fact]
    public void Done_OnAllSteps_ScoreEqualsMaxScore()
    {
        var steps = MakeSteps("s1", "s2", "s3");
        steps[0].ScoreValue = 10;
        steps[1].ScoreValue = 20;
        steps[2].ScoreValue = 30;
        var sut = BuildServiceFromSteps(steps);

        foreach (var s in steps)
            s.Status = StepStatus.Completed;

        Assert.Equal(sut.MaxScore, sut.CalculateScore());
    }

    [Fact]
    public void Done_OnlyCompletedStepsCountTowardsScore()
    {
        var steps = MakeSteps("s1", "s2", "s3");
        steps[0].ScoreValue = 10;
        steps[1].ScoreValue = 20;
        steps[2].ScoreValue = 30;
        var sut = BuildServiceFromSteps(steps);

        steps[0].Status = StepStatus.Completed;
        steps[1].Status = StepStatus.Skipped;
        steps[2].Status = StepStatus.Later;

        Assert.Equal(10, sut.CalculateScore());
    }

    // -------------------------------------------------------------------------
    // Expertenmodus-Sichtbarkeit
    // -------------------------------------------------------------------------

    [Fact]
    public void SimpleMode_HidesExpertOnlySteps()
    {
        var sut = BuildServiceMixed(); // s1 simple, s2 expert-only

        Assert.Equal(ExpertMode.Simple, sut.CurrentMode);
        Assert.Equal(1, sut.TotalSteps);
        Assert.Equal("s1", sut.AllSteps[0].Id);
    }

    [Fact]
    public void ExpertMode_ShowsAllSteps()
    {
        var sut = BuildServiceMixed();

        sut.CurrentMode = ExpertMode.Expert;

        Assert.Equal(2, sut.TotalSteps);
        Assert.Contains(sut.AllSteps, s => s.Id == "s1");
        Assert.Contains(sut.AllSteps, s => s.Id == "s2");
    }

    [Fact]
    public void SwitchingBackToSimpleMode_HidesExpertStepsAgain()
    {
        var sut = BuildServiceMixed();
        sut.CurrentMode = ExpertMode.Expert;

        sut.CurrentMode = ExpertMode.Simple;

        Assert.Equal(1, sut.TotalSteps);
        Assert.Equal("s1", sut.AllSteps[0].Id);
    }

    [Fact]
    public void ExpertModeSwitch_FiresStepChangedEvent()
    {
        var sut = BuildServiceMixed();
        var raised = false;
        sut.StepChanged += (_, _) => raised = true;

        sut.CurrentMode = ExpertMode.Expert;

        Assert.True(raised);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static WizardService BuildService(params string[] stepIds)
    {
        var steps = stepIds.Select(id => new SmokeTestStep(id)).ToArray();
        return BuildServiceFromSteps(steps);
    }

    private static WizardService BuildServiceFromSteps(SmokeTestStep[] steps)
    {
        var module = new SmokeTestModule("m1", 1, steps);
        return new WizardService(new[] { module });
    }

    /// <summary>
    /// Builds a service with one simple-mode step ("s1") and one expert-only step ("s2").
    /// </summary>
    private static WizardService BuildServiceMixed()
    {
        var steps = new[]
        {
            new SmokeTestStep("s1", isSimpleModeStep: true),
            new SmokeTestStep("s2", isSimpleModeStep: false)
        };
        return BuildServiceFromSteps(steps);
    }

    private static SmokeTestStep[] MakeSteps(params string[] ids)
        => ids.Select(id => new SmokeTestStep(id)).ToArray();

    private sealed class SmokeTestModule : IWizardModule
    {
        public string Id { get; }
        public string Name => Id;
        public string Description => Id;
        public string Icon => "T";
        public int Order { get; }
        public IReadOnlyList<IStep> Steps { get; }

        public SmokeTestModule(string id, int order, IReadOnlyList<IStep> steps)
        {
            Id = id;
            Order = order;
            Steps = steps;
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }

    private sealed class SmokeTestStep : IStep
    {
        public string Id { get; }
        public string Title => Id;
        public string Description => Id;
        public string Category => "Smoke";
        public string? ImagePath => null;
        public StepDifficulty Difficulty => StepDifficulty.Easy;
        public StepRiskLevel RiskLevel => StepRiskLevel.Low;
        public string Icon => "T";
        public int ScoreValue { get; set; } = 10;
        public string WhyImportant => "";
        public string WhatItDoes => "";
        public string Risks => "";
        public string WhatNotToDo => "";
        public string RecommendedApproach => "";
        public string SimpleExplanation => "";
        public string ExpertDetails => "";
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public string? UserNote { get; set; }
        public bool SafetyBackupConfirmed { get; set; }
        public bool SafetyImpactConfirmed { get; set; }
        public bool SafetyRecoveryConfirmed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSimpleModeStep { get; }

        public SmokeTestStep(string id, bool isSimpleModeStep = true)
        {
            Id = id;
            IsSimpleModeStep = isSimpleModeStep;
        }
    }
}
