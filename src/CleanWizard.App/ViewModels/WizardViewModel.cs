using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.App.ViewModels;

/// <summary>
/// ViewModel für den Wizard – steuert die Schritt-Navigation.
/// </summary>
public partial class WizardViewModel : ViewModelBase
{
    private readonly IWizardService _wizardService;
    private readonly IProgressService _progressService;
    private readonly ILoggingService _loggingService;
    private readonly IToolLauncherService _toolLauncher;
    private CancellationTokenSource? _debouncedSaveCts;
    private readonly Dictionary<string, StepStateSnapshot> _undoByStepId = new();

    public event EventHandler? WizardCompleted;
    public event EventHandler? ProgressStateChanged;

    [ObservableProperty]
    private StepViewModel? _currentStepVm;

    [ObservableProperty]
    private string _progressText = "Schritt 1 von 1";

    [ObservableProperty]
    private double _progressPercent = 0;

    [ObservableProperty]
    private int _currentScore = 0;

    [ObservableProperty]
    private int _maxScore = 100;

    [ObservableProperty]
    private bool _showSimpleExplanation = false;

    public bool CanGoNext => _wizardService.CanGoNext;
    public bool CanGoPrevious => _wizardService.CanGoPrevious;
    public bool IsLastStep => !_wizardService.CanGoNext;
    public bool CanUndoCurrentStep
        => _wizardService.CurrentStep != null && _undoByStepId.ContainsKey(_wizardService.CurrentStep.Id);

    public WizardViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        ILoggingService loggingService,
        IToolLauncherService toolLauncher)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _loggingService = loggingService;
        _toolLauncher = toolLauncher;

        _wizardService.StepChanged += OnStepChanged;
        RefreshStep();
    }

    private void OnStepChanged(object? sender, EventArgs e)
    {
        RefreshStep();
    }

    public void RefreshStep()
    {
        var step = _wizardService.CurrentStep;
        if (step != null)
        {
            if (CurrentStepVm != null)
                CurrentStepVm.StepChanged -= OnCurrentStepVmStepChanged;

            CurrentStepVm = new StepViewModel(step);
            CurrentStepVm.StepChanged += OnCurrentStepVmStepChanged;
        }

        ProgressText = $"Schritt {_wizardService.CurrentIndex + 1} von {_wizardService.TotalSteps}";
        ProgressPercent = _wizardService.TotalSteps > 0
            ? (double)(_wizardService.CurrentIndex + 1) / _wizardService.TotalSteps * 100
            : 0;

        CurrentScore = _wizardService.CalculateScore();
        MaxScore = _wizardService.MaxScore;
        ShowSimpleExplanation = false;

        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
    }

    private void OnCurrentStepVmStepChanged(object? sender, StepStateChangedEventArgs e)
    {
        _undoByStepId[e.StepId] = e.PreviousState;
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
        ProgressStateChanged?.Invoke(this, EventArgs.Empty);
        DebounceSaveProgress();
    }

    private void DebounceSaveProgress()
    {
        _debouncedSaveCts?.Cancel();
        _debouncedSaveCts?.Dispose();

        _debouncedSaveCts = new CancellationTokenSource();
        var token = _debouncedSaveCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(900, token);
                await SaveProgressAsync();
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (_wizardService.CurrentStep != null)
        {
            // Auto-save progress
            await SaveProgressAsync();
        }

        if (_wizardService.CanGoNext)
        {
            _wizardService.Next();
        }
        else
        {
            WizardCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void Previous()
    {
        _wizardService.Previous();
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        var previousStep = _wizardService.CurrentStep;
        if (previousStep != null)
            RegisterUndoState(previousStep);

        _wizardService.SkipCurrentStep();
        _loggingService.LogInfo($"Schritt übersprungen: {previousStep?.Id}");
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
        ProgressStateChanged?.Invoke(this, EventArgs.Empty);
        await SaveProgressAsync();
    }

    [RelayCommand]
    private async Task MarkLaterAsync()
    {
        var previousStep = _wizardService.CurrentStep;
        if (previousStep != null)
            RegisterUndoState(previousStep);

        _wizardService.MarkCurrentStepLater();
        _loggingService.LogInfo($"Schritt auf später: {previousStep?.Id}");
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
        ProgressStateChanged?.Invoke(this, EventArgs.Empty);
        await SaveProgressAsync();
    }

    [RelayCommand]
    private async Task MarkCompletedAsync()
    {
        var step = _wizardService.CurrentStep;
        if (step != null)
        {
            if (CurrentStepVm != null && !CurrentStepVm.CanMarkCompleted)
            {
                _loggingService.LogWarning($"Schritt nicht bestätigt (Sicherheitscheck fehlt): {step.Id}");
                return;
            }

            RegisterUndoState(step);
            step.Status = StepStatus.Completed;
            step.CompletedAt = DateTime.Now;
            if (CurrentStepVm != null)
            {
                step.UserNote = CurrentStepVm.UserNote;
            }
            _loggingService.LogInfo($"Schritt erledigt: {step.Id}");
            CurrentScore = _wizardService.CalculateScore();
            OnPropertyChanged(nameof(CanUndoCurrentStep));
            UndoLastChangeCommand.NotifyCanExecuteChanged();
            ProgressStateChanged?.Invoke(this, EventArgs.Empty);
            await SaveProgressAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanUndoCurrentStep))]
    private async Task UndoLastChangeAsync()
    {
        var step = _wizardService.CurrentStep;
        if (step == null)
            return;

        if (!_undoByStepId.TryGetValue(step.Id, out var previousState))
            return;

        if (CurrentStepVm != null)
        {
            CurrentStepVm.ApplyState(previousState);
        }
        else
        {
            step.Status = previousState.Status;
            step.UserNote = previousState.UserNote;
            step.CompletedAt = previousState.CompletedAt;
        }

        _undoByStepId.Remove(step.Id);
        CurrentScore = _wizardService.CalculateScore();
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
        ProgressStateChanged?.Invoke(this, EventArgs.Empty);
        await SaveProgressAsync();
        _loggingService.LogInfo($"Letzte Änderung rückgängig: {step.Id}");
    }

    [RelayCommand]
    private void ToggleSimpleExplanation()
    {
        ShowSimpleExplanation = !ShowSimpleExplanation;
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        _toolLauncher.OpenUrl(url);
        _loggingService.LogToolLaunched(url);
    }

    [RelayCommand]
    private void OpenSettings(string settingsUri)
    {
        _toolLauncher.OpenSettings(settingsUri);
    }

    [RelayCommand]
    private void OpenFolder(string path)
    {
        _toolLauncher.OpenFolder(path);
    }

    private async Task SaveProgressAsync()
    {
        var progress = BuildProgress();
        await _progressService.SaveAsync(progress);
    }

    private WizardProgress BuildProgress()
    {
        var progress = new WizardProgress
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
        return progress;
    }

    public void ClearUndoHistory()
    {
        _undoByStepId.Clear();
        OnPropertyChanged(nameof(CanUndoCurrentStep));
        UndoLastChangeCommand.NotifyCanExecuteChanged();
    }

    private void RegisterUndoState(IStep step)
    {
        _undoByStepId[step.Id] = new StepStateSnapshot(step.Status, step.UserNote, step.CompletedAt);
    }
}

/// <summary>
/// ViewModel für einen einzelnen Wizard-Schritt.
/// </summary>
public partial class StepViewModel : ViewModelBase
{
    private readonly IStep _step;
    private bool _isUpdatingState;
    public event EventHandler<StepStateChangedEventArgs>? StepChanged;

    public StepViewModel(IStep step)
    {
        _step = step;
        _isUpdatingState = true;
        IsSafetyAcknowledged = !RequiresSafetyAcknowledgement || step.Status == StepStatus.Completed;
        UserNote = step.UserNote ?? string.Empty;
        IsCompleted = step.Status == StepStatus.Completed;
        IsSkipped = step.Status == StepStatus.Skipped;
        IsLater = step.Status == StepStatus.Later;
        _isUpdatingState = false;
    }

    public string Id => _step.Id;
    public string Title => _step.Title;
    public string Description => _step.Description;
    public string Category => _step.Category;
    public string Icon => _step.Icon;
    public int ScoreValue => _step.ScoreValue;

    public string WhyImportant => _step.WhyImportant;
    public string WhatItDoes => _step.WhatItDoes;
    public string Risks => _step.Risks;
    public string WhatNotToDo => _step.WhatNotToDo;
    public string RecommendedApproach => _step.RecommendedApproach;
    public string SimpleExplanation => _step.SimpleExplanation;
    public string ExpertDetails => _step.ExpertDetails;
    public bool RequiresSafetyAcknowledgement
        => _step.RiskLevel is Core.Enums.StepRiskLevel.High or Core.Enums.StepRiskLevel.Critical;
    public bool CanMarkCompleted => !RequiresSafetyAcknowledgement || IsSafetyAcknowledged;

    public string DifficultyText => _step.Difficulty switch
    {
        Core.Enums.StepDifficulty.Easy => "Einfach",
        Core.Enums.StepDifficulty.Medium => "Mittel",
        Core.Enums.StepDifficulty.Advanced => "Fortgeschritten",
        _ => "Unbekannt"
    };

    public string RiskLevelText => _step.RiskLevel switch
    {
        Core.Enums.StepRiskLevel.Low => "Geringes Risiko",
        Core.Enums.StepRiskLevel.Medium => "Mittleres Risiko",
        Core.Enums.StepRiskLevel.High => "Hohes Risiko",
        Core.Enums.StepRiskLevel.Critical => "Kritisch",
        _ => "Unbekannt"
    };

    public string RiskColor => _step.RiskLevel switch
    {
        Core.Enums.StepRiskLevel.Low => "#4CAF50",
        Core.Enums.StepRiskLevel.Medium => "#FF9800",
        Core.Enums.StepRiskLevel.High => "#FF5722",
        Core.Enums.StepRiskLevel.Critical => "#F44336",
        _ => "#9E9E9E"
    };

    [ObservableProperty]
    private string _userNote = string.Empty;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isSkipped;

    [ObservableProperty]
    private bool _isLater;

    [ObservableProperty]
    private bool _isSafetyAcknowledged;

    partial void OnUserNoteChanged(string value)
    {
        if (_isUpdatingState)
            return;

        var previousState = CaptureState();
        _step.UserNote = value;
        StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
    }

    partial void OnIsCompletedChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        _isUpdatingState = true;
        try
        {
            if (value && !CanMarkCompleted)
            {
                IsCompleted = false;
                return;
            }

            var previousState = CaptureState();
            if (value)
            {
                _step.Status = StepStatus.Completed;
                _step.CompletedAt = DateTime.Now;
                IsSkipped = false;
                IsLater = false;
            }
            else if (!IsSkipped && !IsLater)
            {
                _step.Status = StepStatus.Pending;
                _step.CompletedAt = null;
            }

            StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    partial void OnIsSafetyAcknowledgedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanMarkCompleted));
    }

    partial void OnIsSkippedChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        _isUpdatingState = true;
        try
        {
            var previousState = CaptureState();
            if (value)
            {
                _step.Status = StepStatus.Skipped;
                _step.CompletedAt = null;
                IsCompleted = false;
                IsLater = false;
            }
            else if (!IsCompleted && !IsLater)
            {
                _step.Status = StepStatus.Pending;
            }

            StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    partial void OnIsLaterChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        _isUpdatingState = true;
        try
        {
            var previousState = CaptureState();
            if (value)
            {
                _step.Status = StepStatus.Later;
                _step.CompletedAt = null;
                IsCompleted = false;
                IsSkipped = false;
            }
            else if (!IsCompleted && !IsSkipped)
            {
                _step.Status = StepStatus.Pending;
            }

            StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    public void ApplyState(StepStateSnapshot state)
    {
        _step.Status = state.Status;
        _step.UserNote = state.UserNote;
        _step.CompletedAt = state.CompletedAt;

        _isUpdatingState = true;
        try
        {
            UserNote = state.UserNote ?? string.Empty;
            IsCompleted = state.Status == StepStatus.Completed;
            IsSkipped = state.Status == StepStatus.Skipped;
            IsLater = state.Status == StepStatus.Later;
            IsSafetyAcknowledged = !RequiresSafetyAcknowledgement || state.Status == StepStatus.Completed;
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    private StepStateSnapshot CaptureState()
        => new(_step.Status, _step.UserNote, _step.CompletedAt);
}

public sealed class StepStateChangedEventArgs : EventArgs
{
    public string StepId { get; }
    public StepStateSnapshot PreviousState { get; }

    public StepStateChangedEventArgs(string stepId, StepStateSnapshot previousState)
    {
        StepId = stepId;
        PreviousState = previousState;
    }
}

public readonly record struct StepStateSnapshot(StepStatus Status, string? UserNote, DateTime? CompletedAt);
