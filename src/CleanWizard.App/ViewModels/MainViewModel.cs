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

    [ObservableProperty]
    private List<ModuleProgressItem> _moduleProgressItems = new();

    [ObservableProperty]
    private bool _isSystemCheckActive;

    [ObservableProperty]
    private bool _isWizardActive;

    [ObservableProperty]
    private bool _isSummaryActive;

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
        SystemCheckViewModel.StartWizardRequested += (_, _) =>
        {
            WizardViewModel.SetEmergencyMode(SystemCheckViewModel.IsEmergencyMode);
            NavigateToWizard();
        };
        WizardViewModel.WizardCompleted += (_, _) => NavigateToSummary();
        WizardViewModel.ProgressStateChanged += (_, _) => RefreshModuleProgress();
        SummaryViewModel.RestartRequested += (_, _) => NavigateToSystemCheck();
        RefreshModuleProgress();
        UpdateActiveSection();
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
            {
                await SystemCheckViewModel.EnsureLoadedAsync();
                return;
            }

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

        await SystemCheckViewModel.EnsureLoadedAsync();
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
        WizardViewModel.SetEmergencyMode(false);
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
            step.SafetyBackupConfirmed = false;
            step.SafetyImpactConfirmed = false;
            step.SafetyRecoveryConfirmed = false;
            step.CompletedAt = null;
        }

        ShowResumePrompt = false;
        _loadedProgress = null;
        _hasResumeableProgress = false;
        WizardViewModel.ClearUndoHistory();
        WizardViewModel.SetEmergencyMode(false);
        _wizardService.GoToStep(0);
        WizardViewModel.RefreshStep();
        RefreshModuleProgress();
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
    private void NavigateWizard()
    {
        WizardViewModel.SetEmergencyMode(false);
        NavigateToWizard();
    }

    [RelayCommand]
    private void NavigateSummary() => NavigateToSummary();

    [RelayCommand]
    private void OpenModule(string moduleId)
    {
        var module = _wizardService.Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module == null)
            return;

        var visibleStepIds = module.Steps
            .Where(step => _wizardService.CurrentMode == ExpertMode.Expert || step.IsSimpleModeStep)
            .Select(step => step.Id)
            .ToList();

        if (visibleStepIds.Count == 0)
            return;

        var targetStepId = visibleStepIds
            .FirstOrDefault(id => _wizardService.AllSteps.Any(step => step.Id == id && step.Status == StepStatus.Pending))
            ?? visibleStepIds[0];

        var targetIndex = _wizardService.AllSteps
            .Select((step, index) => (step.Id, index))
            .Where(x => x.Id == targetStepId)
            .Select(x => x.index)
            .DefaultIfEmpty(-1)
            .First();

        if (targetIndex >= 0)
        {
            _wizardService.GoToStep(targetIndex);
            WizardViewModel.RefreshStep();
            WizardViewModel.SetEmergencyMode(false);
            NavigateToWizard();
        }
    }

    private void NavigateToWizard()
    {
        CurrentView = WizardViewModel;
        UpdateActiveSection();
        _loggingService.LogInfo("Wizard gestartet");
    }

    private void NavigateToSummary()
    {
        _ = NavigateToSummaryAsync();
    }

    private async Task NavigateToSummaryAsync()
    {
        await SummaryViewModel.RefreshAsync(SystemCheckViewModel.PerformanceSnapshot);
        CurrentView = SummaryViewModel;
        UpdateActiveSection();
        _loggingService.LogInfo("Zusammenfassung angezeigt");
    }

    private void NavigateToSystemCheck()
    {
        CurrentView = SystemCheckViewModel;
        UpdateActiveSection();
        _ = SystemCheckViewModel.EnsureLoadedAsync();
    }

    partial void OnCurrentViewChanged(ViewModelBase? value)
    {
        UpdateActiveSection();
    }

    private void UpdateActiveSection()
    {
        IsSystemCheckActive = ReferenceEquals(CurrentView, SystemCheckViewModel);
        IsWizardActive = ReferenceEquals(CurrentView, WizardViewModel);
        IsSummaryActive = ReferenceEquals(CurrentView, SummaryViewModel);
    }

    partial void OnIsSimpleModeChanged(bool value)
    {
        if (value)
        {
            IsExpertMode = false;
            ExpertMode = ExpertMode.Simple;
            _wizardService.CurrentMode = ExpertMode.Simple;
            WizardViewModel.RefreshStep();
            RefreshModuleProgress();
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
            RefreshModuleProgress();
        }
        else
        {
            IsSimpleMode = true;
            ExpertMode = ExpertMode.Simple;
            _wizardService.CurrentMode = ExpertMode.Simple;
            WizardViewModel.RefreshStep();
            RefreshModuleProgress();
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
                step.SafetyBackupConfirmed = saved.SafetyBackupConfirmed;
                step.SafetyImpactConfirmed = saved.SafetyImpactConfirmed;
                step.SafetyRecoveryConfirmed = saved.SafetyRecoveryConfirmed;
                step.CompletedAt = saved.CompletedAt;
            }
            else
            {
                step.Status = StepStatus.Pending;
                step.UserNote = null;
                step.SafetyBackupConfirmed = false;
                step.SafetyImpactConfirmed = false;
                step.SafetyRecoveryConfirmed = false;
                step.CompletedAt = null;
            }
        }

        var targetIndex = ResolveTargetIndex(progress);
        WizardViewModel.ClearUndoHistory();
        _wizardService.GoToStep(targetIndex);
        WizardViewModel.RefreshStep();
        RefreshModuleProgress();
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
                SafetyBackupConfirmed = s.SafetyBackupConfirmed,
                SafetyImpactConfirmed = s.SafetyImpactConfirmed,
                SafetyRecoveryConfirmed = s.SafetyRecoveryConfirmed,
                CompletedAt = s.CompletedAt,
                Score = s.Status == StepStatus.Completed ? s.ScoreValue : 0
            }).ToList()
        };
    }

    public void RefreshModuleProgress()
    {
        var items = _wizardService.Modules
            .Select(module =>
            {
                var relevantSteps = module.Steps
                    .Where(step => _wizardService.CurrentMode == ExpertMode.Expert || step.IsSimpleModeStep)
                    .ToList();

                var total = relevantSteps.Count;
                var completed = relevantSteps.Count(step => step.Status == StepStatus.Completed);
                var skipped = relevantSteps.Count(step => step.Status == StepStatus.Skipped);
                var later = relevantSteps.Count(step => step.Status == StepStatus.Later);
                var pending = relevantSteps.Count(step => step.Status == StepStatus.Pending);
                var percent = total > 0 ? (double)completed / total * 100 : 0;

                var statusText = pending == 0
                    ? "Fertig"
                    : completed > 0 || skipped > 0 || later > 0 ? "In Arbeit" : "Nicht gestartet";

                var statusColor = pending == 0
                    ? "#4CAF50"
                    : completed > 0 || skipped > 0 || later > 0 ? "#FF9800" : "#607D8B";

                return new ModuleProgressItem
                {
                    Id = module.Id,
                    Name = module.Name,
                    Icon = module.Icon,
                    CompletedSteps = completed,
                    TotalSteps = total,
                    CompletionPercent = percent,
                    StatusText = statusText,
                    StatusColor = statusColor
                };
            })
            .ToList();

        ModuleProgressItems = items;
    }
}

public class ModuleProgressItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public double CompletionPercent { get; set; }
    public string StatusText { get; set; } = "";
    public string StatusColor { get; set; } = "";
}
