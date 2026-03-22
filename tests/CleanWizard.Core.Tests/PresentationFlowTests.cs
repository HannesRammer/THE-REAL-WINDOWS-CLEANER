using CleanWizard.App.ViewModels;
using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Core.Tests;

public class PresentationFlowTests
{
    [Fact]
    public void StepViewModel_DetectsGuidedSetupFlow_AndSeparatesAdditionalActions()
    {
        var step = new FakeStep
        {
            ToolActions = new List<StepToolAction>
            {
                new() { Id = "check", Label = "Status prüfen", Description = "Prüft die Installation", ActionType = StepToolActionType.CheckInstalled, Target = "autoruns" },
                new() { Id = "install", Label = "Autoruns installieren", Description = "Installiert Autoruns", ActionType = StepToolActionType.InstallPackage, Target = "Microsoft.Sysinternals.Autoruns", Arguments = "autoruns|https://learn.microsoft.com/sysinternals/" },
                new() { Id = "open", Label = "Autoruns öffnen", Description = "Startet Autoruns", ActionType = StepToolActionType.Executable, Target = "autoruns64.exe" },
                new() { Id = "site", Label = "Offizielle Seite", Description = "Öffnet die Produktseite", ActionType = StepToolActionType.Url, Target = "https://learn.microsoft.com/sysinternals/" }
            }
        };

        var sut = new StepViewModel(step);

        Assert.True(sut.HasSetupFlow);
        Assert.NotNull(sut.SetupCheckAction);
        Assert.NotNull(sut.SetupInstallAction);
        Assert.NotNull(sut.SetupOpenAction);
        Assert.Null(sut.PrimaryToolAction);
        Assert.Single(sut.AdditionalToolActions);
        Assert.Equal("Offizielle Seite", sut.AdditionalToolActions[0].Label);
    }

    [Fact]
    public async Task SummaryViewModel_Refresh_PrioritizesLaterAndPendingStepsAsNextSteps()
    {
        var steps = new List<IStep>
        {
            new FakeStep { Id = "done", Title = "Bereits erledigt", Category = "Autoruns", Status = StepStatus.Completed, ScoreValue = 5 },
            new FakeStep { Id = "later", Title = "Später fortsetzen", Category = "Malwarebytes", Status = StepStatus.Later, ScoreValue = 3 },
            new FakeStep { Id = "pending", Title = "Noch offen", Category = "Windows-Werkzeuge", Status = StepStatus.Pending, ScoreValue = 8 },
            new FakeStep { Id = "skipped", Title = "Übersprungen", Category = "Autoruns", Status = StepStatus.Skipped, ScoreValue = 1 }
        };

        var sut = new SummaryViewModel(
            new FakeWizardService(steps),
            new FakeProgressService(),
            new FakeExportService(),
            new FakeLogger(),
            new FakePerformanceAnalyzer());

        await sut.RefreshAsync(null);

        Assert.True(sut.HasNextSteps);
        Assert.Equal("Sinnvolle nächste Schritte", sut.NextStepsTitle);
        Assert.Equal(3, sut.NextSteps.Count);
        Assert.Equal("Später fortsetzen", sut.NextSteps[0].Title);
        Assert.Equal("Noch offen", sut.NextSteps[1].Title);
        Assert.Equal("Übersprungen", sut.NextSteps[2].StatusText);
    }

    private sealed class FakeStep : IStep
    {
        public string Id { get; set; } = "step";
        public string Title { get; set; } = "Titel";
        public string Description { get; set; } = "Beschreibung";
        public string Category { get; set; } = "Bereich";
        public string? ImagePath { get; set; }
        public StepDifficulty Difficulty { get; set; } = StepDifficulty.Easy;
        public StepRiskLevel RiskLevel { get; set; } = StepRiskLevel.Low;
        public string Icon { get; set; } = "i";
        public int ScoreValue { get; set; }
        public string WhyImportant { get; set; } = "Warum";
        public string WhatItDoes { get; set; } = "Nutzen";
        public string Risks { get; set; } = "Risiken";
        public string WhatNotToDo { get; set; } = "Nicht tun";
        public string RecommendedApproach { get; set; } = "Empfohlen";
        public string SimpleExplanation { get; set; } = "Einfach";
        public string ExpertDetails { get; set; } = "Details";
        public IReadOnlyList<StepToolAction> ToolActions { get; set; } = Array.Empty<StepToolAction>();
        public IReadOnlyList<StepAction> Actions { get; set; } = Array.Empty<StepAction>();
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public string? UserNote { get; set; }
        public bool SafetyBackupConfirmed { get; set; }
        public bool SafetyImpactConfirmed { get; set; }
        public bool SafetyRecoveryConfirmed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSimpleModeStep { get; set; } = true;
    }

    private sealed class FakeWizardService : IWizardService
    {
        public FakeWizardService(IReadOnlyList<IStep> steps)
        {
            AllSteps = steps;
            Modules = Array.Empty<IWizardModule>();
        }

        public IReadOnlyList<IWizardModule> Modules { get; }
        public IReadOnlyList<IStep> AllSteps { get; }
        public IStep? CurrentStep => AllSteps.FirstOrDefault();
        public int CurrentIndex => 0;
        public int TotalSteps => AllSteps.Count;
        public ExpertMode CurrentMode { get; set; } = ExpertMode.Simple;
        public bool CanGoNext => false;
        public bool CanGoPrevious => false;
        public int MaxScore => 100;
        public event EventHandler? StepChanged
        {
            add { }
            remove { }
        }
        public void Next() { }
        public void Previous() { }
        public void SkipCurrentStep() { }
        public void MarkCurrentStepLater() { }
        public void GoToStep(int index) { }
        public int CalculateScore() => AllSteps.Where(s => s.Status == StepStatus.Completed).Sum(s => s.ScoreValue);
    }

    private sealed class FakeProgressService : IProgressService
    {
        public Task SaveAsync(WizardProgress progress) => Task.CompletedTask;
        public Task<WizardProgress?> LoadAsync() => Task.FromResult<WizardProgress?>(null);
    }

    private sealed class FakeExportService : IExportService
    {
        public Task ExportReportAsync(string filePath, WizardProgress progress, bool asJson = false) => Task.CompletedTask;
    }

    private sealed class FakeLogger : ILoggingService
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogToolLaunched(string toolName) { }
        public Task ExportAsync(string filePath) => Task.CompletedTask;
        public IReadOnlyList<string> GetEntries() => Array.Empty<string>();
    }

    private sealed class FakePerformanceAnalyzer : IPerformanceAnalyzer
    {
        public Task<PerformanceSnapshot> CaptureAsync() => Task.FromResult(new PerformanceSnapshot());
    }
}
