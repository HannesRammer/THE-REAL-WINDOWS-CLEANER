using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.App.ViewModels;

/// <summary>
/// Basis-ViewModel für alle ViewModels der App.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}

/// <summary>
/// Haupt-ViewModel – steuert die Navigation zwischen den Hauptbereichen.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IWizardService _wizardService;
    private readonly IProgressService _progressService;
    private readonly ILoggingService _loggingService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.Dark;

    [ObservableProperty]
    private ExpertMode _expertMode = ExpertMode.Simple;

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private bool _isSimpleMode = true;

    [ObservableProperty]
    private bool _isExpertMode = false;

    public SystemCheckViewModel SystemCheckViewModel { get; }
    public WizardViewModel WizardViewModel { get; }
    public SummaryViewModel SummaryViewModel { get; }
    private bool _hasInitialized;
    private WizardProgress? _loadedProgress;
    private bool _hasResumeableProgress;

    [ObservableProperty]
    private bool _showResumePrompt;

    [ObservableProperty]
    private string _resumeInfoText = "Es wurde ein gespeicherter Fortschritt gefunden.";

    public MainViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        ILoggingService loggingService,
        SystemCheckViewModel systemCheckViewModel,
        WizardViewModel wizardViewModel,
        SummaryViewModel summaryViewModel)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _loggingService = loggingService;
        SystemCheckViewModel = systemCheckViewModel;
        WizardViewModel = wizardViewModel;
        SummaryViewModel = summaryViewModel;

        // Start with system check
        CurrentView = SystemCheckViewModel;

        // Wire up navigation
        SystemCheckViewModel.StartWizardRequested += (_, _) => NavigateToWizard();
        WizardViewModel.WizardCompleted += (_, _) => NavigateToSummary();
        SummaryViewModel.RestartRequested += (_, _) => NavigateToSystemCheck();
    }

    public async Task InitializeAsync()
    {
        if (_hasInitialized)
            return;

        _hasInitialized = true;

        try
        {
            var progress = await _progressService.LoadAsync();
            if (progress == null)
                return;

            _loadedProgress = progress;
            _hasResumeableProgress = progress.Steps.Any(s =>
                s.Status != StepStatus.Pending ||
                !string.IsNullOrWhiteSpace(s.Note));

            if (_hasResumeableProgress)
            {
                var savedAt = progress.LastSavedAt ?? progress.CreatedAt;
                ResumeInfoText = $"Gespeicherter Stand vom {savedAt:dd.MM.yyyy HH:mm} gefunden.";
                ShowResumePrompt = true;
                _loggingService.LogInfo($"Gespeicherter Fortschritt erkannt: {progress.Steps.Count} Schritte");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogWarning($"Konnte Fortschritt nicht laden: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ContinueSavedProgress()
    {
        if (!_hasResumeableProgress || _loadedProgress == null)
        {
            ShowResumePrompt = false;
            return;
        }

        ApplyProgress(_loadedProgress);
        NavigateToWizard();
        ShowResumePrompt = false;
        _loggingService.LogInfo("Fortschritt wiederhergestellt");
    }

    [RelayCommand]
    private async Task StartFreshProgressAsync()
    {
        foreach (var step in _wizardService.AllSteps)
        {
            step.Status = StepStatus.Pending;
            step.UserNote = null;
            step.CompletedAt = null;
        }

        ShowResumePrompt = false;
        _loadedProgress = null;
        _hasResumeableProgress = false;
        _wizardService.GoToStep(0);
        WizardViewModel.RefreshStep();
        await SaveProgressOnExitAsync();
        _loggingService.LogInfo("Gespeicherter Fortschritt verworfen und zurückgesetzt");
    }

    public async Task SaveProgressOnExitAsync()
    {
        try
        {
            await _progressService.SaveAsync(BuildProgress());
        }
        catch (Exception ex)
        {
            _loggingService.LogWarning($"Auto-Save beim Beenden fehlgeschlagen: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NavigateSystemCheck() => NavigateToSystemCheck();

    [RelayCommand]
    private void NavigateWizard() => NavigateToWizard();

    [RelayCommand]
    private void NavigateSummary() => NavigateToSummary();

    private void NavigateToWizard()
    {
        CurrentView = WizardViewModel;
        _loggingService.LogInfo("Wizard gestartet");
    }

    private void NavigateToSummary()
    {
        SummaryViewModel.Refresh();
        CurrentView = SummaryViewModel;
        _loggingService.LogInfo("Zusammenfassung angezeigt");
    }

    private void NavigateToSystemCheck()
    {
        CurrentView = SystemCheckViewModel;
    }

    partial void OnIsSimpleModeChanged(bool value)
    {
        if (value)
        {
            IsExpertMode = false;
            ExpertMode = ExpertMode.Simple;
            _wizardService.CurrentMode = ExpertMode.Simple;
            WizardViewModel.RefreshStep();
        }
    }

    partial void OnIsExpertModeChanged(bool value)
    {
        if (value)
        {
            IsSimpleMode = false;
            ExpertMode = ExpertMode.Expert;
            _wizardService.CurrentMode = ExpertMode.Expert;
            WizardViewModel.RefreshStep();
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        CurrentTheme = value ? AppTheme.Dark : AppTheme.Light;
        ApplyTheme(CurrentTheme);
    }

    private static void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        var dict = app.Resources.MergedDictionaries;
        var colorDict = dict.FirstOrDefault(d => d.Source?.OriginalString.Contains("Colors") == true);
        if (colorDict != null) dict.Remove(colorDict);

        var source = theme == AppTheme.Dark
            ? new Uri("Styles/Colors.Dark.xaml", UriKind.Relative)
            : new Uri("Styles/Colors.xaml", UriKind.Relative);

        dict.Insert(0, new System.Windows.ResourceDictionary { Source = source });
    }

    private void ApplyProgress(WizardProgress progress)
    {
        if (progress.Mode == ExpertMode.Expert)
        {
            IsExpertMode = true;
        }
        else
        {
            IsSimpleMode = true;
        }

        var progressByStepId = progress.Steps
            .GroupBy(s => s.StepId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var step in _wizardService.AllSteps)
        {
            if (progressByStepId.TryGetValue(step.Id, out var saved))
            {
                step.Status = saved.Status;
                step.UserNote = saved.Note;
                step.CompletedAt = saved.CompletedAt;
            }
            else
            {
                step.Status = StepStatus.Pending;
                step.UserNote = null;
                step.CompletedAt = null;
            }
        }

        var targetIndex = ResolveTargetIndex(progress);
        _wizardService.GoToStep(targetIndex);
        WizardViewModel.RefreshStep();
    }

    private int ResolveTargetIndex(WizardProgress progress)
    {
        if (_wizardService.AllSteps.Count == 0)
            return 0;

        if (!string.IsNullOrWhiteSpace(progress.CurrentStepId))
        {
            for (var i = 0; i < _wizardService.AllSteps.Count; i++)
            {
                if (_wizardService.AllSteps[i].Id == progress.CurrentStepId)
                    return i;
            }
        }

        for (var i = 0; i < _wizardService.AllSteps.Count; i++)
        {
            if (_wizardService.AllSteps[i].Status == StepStatus.Pending)
                return i;
        }

        return 0;
    }

    private WizardProgress BuildProgress()
    {
        return new WizardProgress
        {
            CreatedAt = DateTime.Now,
            CurrentStepId = _wizardService.CurrentStep?.Id,
            TotalScore = _wizardService.CalculateScore(),
            Mode = _wizardService.CurrentMode,
            Steps = _wizardService.AllSteps.Select(s => new StepProgress
            {
                StepId = s.Id,
                Status = s.Status,
                Note = s.UserNote,
                CompletedAt = s.CompletedAt,
                Score = s.Status == StepStatus.Completed ? s.ScoreValue : 0
            }).ToList()
        };
    }
}
