using CleanWizard.Core.Enums;

namespace CleanWizard.Core.Interfaces;

public interface IWizardService
{
    IReadOnlyList<IWizardModule> Modules { get; }
    IReadOnlyList<IStep> AllSteps { get; }
    IStep? CurrentStep { get; }
    int CurrentIndex { get; }
    int TotalSteps { get; }
    ExpertMode CurrentMode { get; set; }

    bool CanGoNext { get; }
    bool CanGoPrevious { get; }

    void Next();
    void Previous();
    void SkipCurrentStep();
    void MarkCurrentStepLater();
    void GoToStep(int index);

    int CalculateScore();
    int MaxScore { get; }

    event EventHandler? StepChanged;
}
