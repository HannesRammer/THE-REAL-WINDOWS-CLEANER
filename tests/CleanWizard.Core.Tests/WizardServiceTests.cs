using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;
using CleanWizard.Core.Services;

namespace CleanWizard.Core.Tests;

public class WizardServiceTests
{
    [Fact]
    public void Constructor_SortsModulesByOrder_AndBuildsSimpleSteps()
    {
        var moduleB = new TestModule("b", 2, new[]
        {
            new TestStep("b1", isSimpleModeStep: true),
            new TestStep("b2", isSimpleModeStep: false)
        });
        var moduleA = new TestModule("a", 1, new[]
        {
            new TestStep("a1", isSimpleModeStep: true),
            new TestStep("a2", isSimpleModeStep: true)
        });

        var sut = new WizardService(new[] { moduleB, moduleA });

        Assert.Equal(new[] { "a", "b" }, sut.Modules.Select(m => m.Id));
        Assert.Equal(new[] { "a1", "a2", "b1" }, sut.AllSteps.Select(s => s.Id));
    }

    [Fact]
    public void ExpertMode_IncludesExpertOnlySteps()
    {
        var module = new TestModule("m1", 1, new[]
        {
            new TestStep("s1", isSimpleModeStep: true),
            new TestStep("s2", isSimpleModeStep: false)
        });

        var sut = new WizardService(new[] { module });

        Assert.Equal(1, sut.TotalSteps);
        sut.CurrentMode = ExpertMode.Expert;
        Assert.Equal(2, sut.TotalSteps);
        Assert.Equal(new[] { "s1", "s2" }, sut.AllSteps.Select(s => s.Id));
    }

    [Fact]
    public void SkipCurrentStep_MarksSkipped_AndMovesNext()
    {
        var steps = new[] { new TestStep("s1"), new TestStep("s2") };
        var module = new TestModule("m1", 1, steps);
        var sut = new WizardService(new[] { module });

        sut.SkipCurrentStep();

        Assert.Equal(StepStatus.Skipped, steps[0].Status);
        Assert.Equal(1, sut.CurrentIndex);
        Assert.Equal("s2", sut.CurrentStep?.Id);
    }

    [Fact]
    public void MarkCurrentStepLater_MarksLater_AndMovesNext()
    {
        var steps = new[] { new TestStep("s1"), new TestStep("s2") };
        var module = new TestModule("m1", 1, steps);
        var sut = new WizardService(new[] { module });

        sut.MarkCurrentStepLater();

        Assert.Equal(StepStatus.Later, steps[0].Status);
        Assert.Equal(1, sut.CurrentIndex);
        Assert.Equal("s2", sut.CurrentStep?.Id);
    }

    [Fact]
    public void CalculateScore_CountsCompletedOnly()
    {
        var steps = new[]
        {
            new TestStep("s1", score: 10) { Status = StepStatus.Completed },
            new TestStep("s2", score: 20) { Status = StepStatus.Later },
            new TestStep("s3", score: 30) { Status = StepStatus.Completed }
        };
        var module = new TestModule("m1", 1, steps);
        var sut = new WizardService(new[] { module });

        var score = sut.CalculateScore();

        Assert.Equal(40, score);
        Assert.Equal(60, sut.MaxScore);
    }

    [Fact]
    public void ModeSwitch_ClampsCurrentIndex_WhenStepListShrinks()
    {
        var module = new TestModule("m1", 1, new[]
        {
            new TestStep("s1", isSimpleModeStep: true),
            new TestStep("s2", isSimpleModeStep: false)
        });
        var sut = new WizardService(new[] { module });
        sut.CurrentMode = ExpertMode.Expert;
        sut.GoToStep(1);

        sut.CurrentMode = ExpertMode.Simple;

        Assert.Equal(0, sut.CurrentIndex);
        Assert.Equal("s1", sut.CurrentStep?.Id);
    }

    [Fact]
    public void Step_WithNoActions_ReturnsEmptyActionsList()
    {
        var step = new TestStep("s1");

        Assert.Empty(step.Actions);
    }

    [Fact]
    public void Step_Actions_SupportPriorityFiltering()
    {
        var actions = new[]
        {
            new StepAction("Primary Action", "🚀", StepActionType.OpenUrl, "https://example.com", StepActionPriority.Primary),
            new StepAction("Secondary Action", "⚙️", StepActionType.OpenSettings, "ms-settings:test", StepActionPriority.Secondary)
        };
        var step = new TestStepWithActions("s1", actions);

        var primary = step.Actions.Where(a => a.Priority == StepActionPriority.Primary).ToList();
        var secondary = step.Actions.Where(a => a.Priority == StepActionPriority.Secondary).ToList();

        Assert.Single(primary);
        Assert.Equal("Primary Action", primary[0].Label);
        Assert.Single(secondary);
        Assert.Equal("Secondary Action", secondary[0].Label);
    }

    [Fact]
    public void StepAction_Properties_AreSetCorrectly()
    {
        var action = new StepAction("Download", "⬇️", StepActionType.OpenUrl, "https://example.com", StepActionPriority.Primary);

        Assert.Equal("Download", action.Label);
        Assert.Equal("⬇️", action.Icon);
        Assert.Equal(StepActionType.OpenUrl, action.ActionType);
        Assert.Equal("https://example.com", action.Parameter);
        Assert.Equal(StepActionPriority.Primary, action.Priority);
    }

    private sealed class TestStepWithActions : IStep
    {
        private readonly IReadOnlyList<StepAction> _actions;

        public TestStepWithActions(string id, IReadOnlyList<StepAction> actions)
        {
            Id = id;
            _actions = actions;
        }

        public string Id { get; }
        public string Title => Id;
        public string Description => Id;
        public string Category => "Test";
        public string? ImagePath => null;
        public StepDifficulty Difficulty => StepDifficulty.Easy;
        public StepRiskLevel RiskLevel => StepRiskLevel.Low;
        public string Icon => "I";
        public int ScoreValue => 10;
        public string WhyImportant => "";
        public string WhatItDoes => "";
        public string Risks => "";
        public string WhatNotToDo => "";
        public string RecommendedApproach => "";
        public string SimpleExplanation => "";
        public string ExpertDetails => "";
        public IReadOnlyList<StepToolAction> ToolActions => Array.Empty<StepToolAction>();
        public IReadOnlyList<StepAction> Actions => _actions;
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public string? UserNote { get; set; }
        public bool SafetyBackupConfirmed { get; set; }
        public bool SafetyImpactConfirmed { get; set; }
        public bool SafetyRecoveryConfirmed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSimpleModeStep => true;
    }

    private sealed class TestModule : IWizardModule
    {
        public string Id { get; }
        public string Name => Id;
        public string Description => Id;
        public string Icon => "I";
        public int Order { get; }
        public IReadOnlyList<IStep> Steps { get; }

        public TestModule(string id, int order, IReadOnlyList<IStep> steps)
        {
            Id = id;
            Order = order;
            Steps = steps;
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }

    private sealed class TestStep : IStep
    {
        public string Id { get; }
        public string Title => Id;
        public string Description => Id;
        public string Category => "Test";
        public string? ImagePath => null;
        public StepDifficulty Difficulty => StepDifficulty.Easy;
        public StepRiskLevel RiskLevel => StepRiskLevel.Low;
        public string Icon => "I";
        public int ScoreValue { get; }
        public string WhyImportant => "";
        public string WhatItDoes => "";
        public string Risks => "";
        public string WhatNotToDo => "";
        public string RecommendedApproach => "";
        public string SimpleExplanation => "";
        public string ExpertDetails => "";
        public IReadOnlyList<StepToolAction> ToolActions => Array.Empty<StepToolAction>();
        public IReadOnlyList<StepAction> Actions => Array.Empty<StepAction>();
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public string? UserNote { get; set; }
        public bool SafetyBackupConfirmed { get; set; }
        public bool SafetyImpactConfirmed { get; set; }
        public bool SafetyRecoveryConfirmed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSimpleModeStep { get; }

        public TestStep(string id, int score = 10, bool isSimpleModeStep = true)
        {
            Id = id;
            ScoreValue = score;
            IsSimpleModeStep = isSimpleModeStep;
        }
    }
}
