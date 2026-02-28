using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;

namespace CleanWizard.Core.Services;

/// <summary>
/// Zentraler Wizard-Service – verwaltet Navigation und Fortschritt.
/// </summary>
public class WizardService : IWizardService
{
    private readonly List<IWizardModule> _modules;
    private List<IStep> _allSteps = new();
    private int _currentIndex = 0;
    private ExpertMode _currentMode = ExpertMode.Simple;

    public WizardService(IEnumerable<IWizardModule> modules)
    {
        _modules = modules.OrderBy(m => m.Order).ToList();
        RebuildStepList();
    }

    public IReadOnlyList<IWizardModule> Modules => _modules.AsReadOnly();
    public IReadOnlyList<IStep> AllSteps => _allSteps.AsReadOnly();

    public IStep? CurrentStep => _allSteps.Count > 0 && _currentIndex < _allSteps.Count
        ? _allSteps[_currentIndex]
        : null;

    public int CurrentIndex => _currentIndex;
    public int TotalSteps => _allSteps.Count;

    public ExpertMode CurrentMode
    {
        get => _currentMode;
        set
        {
            _currentMode = value;
            RebuildStepList();
            StepChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CanGoNext => _currentIndex < _allSteps.Count - 1;
    public bool CanGoPrevious => _currentIndex > 0;

    public event EventHandler? StepChanged;

    public void Next()
    {
        if (CanGoNext)
        {
            _currentIndex++;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Previous()
    {
        if (CanGoPrevious)
        {
            _currentIndex--;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SkipCurrentStep()
    {
        if (CurrentStep != null)
        {
            CurrentStep.Status = StepStatus.Skipped;
            Next();
        }
    }

    public void MarkCurrentStepLater()
    {
        if (CurrentStep != null)
        {
            CurrentStep.Status = StepStatus.Later;
            Next();
        }
    }

    public void GoToStep(int index)
    {
        if (index >= 0 && index < _allSteps.Count)
        {
            _currentIndex = index;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int CalculateScore()
    {
        return _allSteps
            .Where(s => s.Status == StepStatus.Completed)
            .Sum(s => s.ScoreValue);
    }

    public int MaxScore => _allSteps.Sum(s => s.ScoreValue);

    private void RebuildStepList()
    {
        _allSteps = _modules
            .SelectMany(m => m.Steps)
            .Where(s => _currentMode == ExpertMode.Expert || s.IsSimpleModeStep)
            .ToList();

        // Clamp index
        if (_currentIndex >= _allSteps.Count)
            _currentIndex = Math.Max(0, _allSteps.Count - 1);
    }
}
