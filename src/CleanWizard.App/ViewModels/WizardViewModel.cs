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
    private readonly IToolSetupService _toolSetupService;
    private CancellationTokenSource? _debouncedSaveCts;
    private readonly Dictionary<string, StepStateSnapshot> _undoByStepId = new();
    private static readonly HashSet<string> EmergencyStepIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "autoruns_scan",
        "autoruns_cleanup",
        "malware_scan",
        "win_taskmanager_autostart",
        "win_disk_cleanup",
        "win_update"
    };

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

    [ObservableProperty]
    private string _toolFeedbackMessage = string.Empty;

    [ObservableProperty]
    private string _toolFeedbackBackground = "#E8F5E9";

    [ObservableProperty]
    private string _toolFeedbackBorder = "#4CAF50";

    [ObservableProperty]
    private string _toolFeedbackForeground = "#1B5E20";

    [ObservableProperty]
    private bool _isEmergencyModeActive;

    [ObservableProperty]
    private ToolSetupState _toolSetupState = ToolSetupState.Unknown;

    [ObservableProperty]
    private string _toolSetupStateText = "";

    [ObservableProperty]
    private string _toolSetupStateColor = "#9E9E9E";

    [ObservableProperty]
    private bool _isToolActionBusy;

    public bool CanGoNext => IsEmergencyModeActive
        ? TryGetNextEmergencyIndex(_wizardService.CurrentIndex, out _)
        : _wizardService.CanGoNext;
    public bool CanGoPrevious => IsEmergencyModeActive
        ? TryGetPreviousEmergencyIndex(_wizardService.CurrentIndex, out _)
        : _wizardService.CanGoPrevious;
    public bool IsLastStep => !CanGoNext;
    public bool IsExpertMode => _wizardService.CurrentMode == ExpertMode.Expert;
    public bool CanUndoCurrentStep
        => _wizardService.CurrentStep != null && _undoByStepId.ContainsKey(_wizardService.CurrentStep.Id);

    public WizardViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        ILoggingService loggingService,
        IToolLauncherService toolLauncher,
        IToolSetupService toolSetupService)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _loggingService = loggingService;
        _toolLauncher = toolLauncher;
        _toolSetupService = toolSetupService;

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

        if (IsEmergencyModeActive)
        {
            var emergencyIndices = GetEmergencyIndices();
            var currentEmergencyPosition = emergencyIndices.FindIndex(i => i == _wizardService.CurrentIndex);
            var position = currentEmergencyPosition >= 0 ? currentEmergencyPosition + 1 : 1;
            var total = emergencyIndices.Count > 0 ? emergencyIndices.Count : 1;

            ProgressText = $"Notfallmodus: Schritt {position} von {total}";
            ProgressPercent = (double)position / total * 100;
        }
        else
        {
            ProgressText = $"Schritt {_wizardService.CurrentIndex + 1} von {_wizardService.TotalSteps}";
            ProgressPercent = _wizardService.TotalSteps > 0
                ? (double)(_wizardService.CurrentIndex + 1) / _wizardService.TotalSteps * 100
                : 0;
        }

        CurrentScore = _wizardService.CalculateScore();
        MaxScore = _wizardService.MaxScore;
        ShowSimpleExplanation = false;
        ToolFeedbackMessage = string.Empty;
        SetToolSetupState(ToolSetupState.Unknown, string.Empty);

        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(IsExpertMode));
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

        if (IsEmergencyModeActive)
        {
            if (TryGetNextEmergencyIndex(_wizardService.CurrentIndex, out var nextEmergencyIndex))
            {
                _wizardService.GoToStep(nextEmergencyIndex);
            }
            else
            {
                WizardCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (_wizardService.CanGoNext)
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
        if (IsEmergencyModeActive)
        {
            if (TryGetPreviousEmergencyIndex(_wizardService.CurrentIndex, out var previousEmergencyIndex))
            {
                _wizardService.GoToStep(previousEmergencyIndex);
            }
        }
        else
        {
            _wizardService.Previous();
        }
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        var previousStep = _wizardService.CurrentStep;
        if (previousStep != null)
            RegisterUndoState(previousStep);

        if (previousStep != null)
        {
            previousStep.Status = StepStatus.Skipped;
        }

        if (IsEmergencyModeActive)
        {
            if (TryGetNextEmergencyIndex(_wizardService.CurrentIndex, out var nextEmergencyIndex))
            {
                _wizardService.GoToStep(nextEmergencyIndex);
            }
        }
        else
        {
            _wizardService.Next();
        }

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

        if (previousStep != null)
        {
            previousStep.Status = StepStatus.Later;
        }

        if (IsEmergencyModeActive)
        {
            if (TryGetNextEmergencyIndex(_wizardService.CurrentIndex, out var nextEmergencyIndex))
            {
                _wizardService.GoToStep(nextEmergencyIndex);
            }
        }
        else
        {
            _wizardService.Next();
        }

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
            step.SafetyBackupConfirmed = previousState.SafetyBackupConfirmed;
            step.SafetyImpactConfirmed = previousState.SafetyImpactConfirmed;
            step.SafetyRecoveryConfirmed = previousState.SafetyRecoveryConfirmed;
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
        var success = _toolLauncher.OpenUrl(url);
        SetToolFeedback(
            success,
            success ? "Link wurde geöffnet." : "Link konnte nicht geöffnet werden.");
    }

    [RelayCommand]
    private void OpenSettings(string settingsUri)
    {
        var success = _toolLauncher.OpenSettings(settingsUri);
        SetToolFeedback(
            success,
            success ? "Windows-Einstellungen wurden geöffnet." : "Windows-Einstellungen konnten nicht geöffnet werden.");
    }

    [RelayCommand]
    private void OpenFolder(string path)
    {
        var success = _toolLauncher.OpenFolder(path);
        SetToolFeedback(
            success,
            success ? "Ordner wurde geöffnet." : "Ordner konnte nicht geöffnet werden.");
    }

    [RelayCommand]
    private async Task RunToolAction(StepToolAction? action)
    {
        if (action == null || IsToolActionBusy)
            return;

        IsToolActionBusy = true;
        try
        {
            switch (action.ActionType)
            {
                case StepToolActionType.CheckInstalled:
                {
                    var availability = _toolSetupService.CheckAvailability(action.Target);
                    SetToolSetupState(
                        availability.IsInstalled ? ToolSetupState.Installed : ToolSetupState.NotInstalled,
                        availability.Message);
                    SetToolFeedback(true, $"{action.Label}: {availability.Message}");
                    return;
                }
                case StepToolActionType.InstallPackage:
                {
                    var (toolId, fallbackUrl) = ParseInstallArguments(action.Arguments);
                    SetToolSetupState(ToolSetupState.Installing, "Installiere...");
                    SetToolFeedback(true, "Installation gestartet. Bitte warten...");
                    var install = await _toolSetupService.InstallAsync(toolId, action.Target, fallbackUrl);
                    if (install.Success)
                    {
                        SetToolSetupState(ToolSetupState.Installed, "Installiert");
                        SetToolFeedback(true, install.Message);
                    }
                    else
                    {
                        SetToolSetupState(ToolSetupState.Error, "Fehler");
                        SetToolFeedback(false, install.Message);
                    }

                    return;
                }
            }

            var success = action.ActionType switch
            {
                StepToolActionType.Url => _toolLauncher.OpenUrl(action.Target),
                StepToolActionType.SettingsUri => _toolLauncher.OpenSettings(action.Target),
                StepToolActionType.FolderPath => _toolLauncher.OpenFolder(action.Target),
                StepToolActionType.Executable when action.Target.Equals("autoruns64.exe", StringComparison.OrdinalIgnoreCase)
                    => _toolSetupService.Launch("autoruns"),
                StepToolActionType.Executable when action.Target.Equals("mbam.exe", StringComparison.OrdinalIgnoreCase)
                    => _toolSetupService.Launch("malwarebytes"),
                StepToolActionType.Executable => _toolLauncher.LaunchExecutable(action.Target),
                _ => false
            };

            var successText = string.IsNullOrWhiteSpace(action.SafetyHint)
                ? $"{action.Label} wurde geöffnet."
                : $"{action.Label} wurde geöffnet. Hinweis: {action.SafetyHint}";

            SetToolFeedback(
                success,
                success ? successText : $"{action.Label} konnte nicht geöffnet werden.");
        }
        finally
        {
            IsToolActionBusy = false;
        }
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
                SafetyBackupConfirmed = s.SafetyBackupConfirmed,
                SafetyImpactConfirmed = s.SafetyImpactConfirmed,
                SafetyRecoveryConfirmed = s.SafetyRecoveryConfirmed,
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

    public void SetEmergencyMode(bool enabled)
    {
        IsEmergencyModeActive = enabled;

        if (enabled)
        {
            var emergencyIndices = GetEmergencyIndices();
            if (emergencyIndices.Count > 0)
            {
                var target = emergencyIndices
                    .FirstOrDefault(i => _wizardService.AllSteps[i].Status == StepStatus.Pending, emergencyIndices[0]);
                _wizardService.GoToStep(target);
            }
        }

        RefreshStep();
    }

    private void RegisterUndoState(IStep step)
    {
        _undoByStepId[step.Id] = new StepStateSnapshot(
            step.Status,
            step.UserNote,
            step.CompletedAt,
            step.SafetyBackupConfirmed,
            step.SafetyImpactConfirmed,
            step.SafetyRecoveryConfirmed);
    }

    private void SetToolFeedback(bool success, string message)
    {
        ToolFeedbackMessage = message;
        if (success)
        {
            ToolFeedbackBackground = "#E8F5E9";
            ToolFeedbackBorder = "#4CAF50";
            ToolFeedbackForeground = "#1B5E20";
        }
        else
        {
            ToolFeedbackBackground = "#FFEBEE";
            ToolFeedbackBorder = "#F44336";
            ToolFeedbackForeground = "#B71C1C";
        }
    }

    private void SetToolSetupState(ToolSetupState state, string text)
    {
        ToolSetupState = state;
        ToolSetupStateText = text;
        ToolSetupStateColor = state switch
        {
            ToolSetupState.Installed => "#2E7D32",
            ToolSetupState.NotInstalled => "#EF6C00",
            ToolSetupState.Installing => "#1565C0",
            ToolSetupState.Error => "#C62828",
            _ => "#9E9E9E"
        };
    }

    private static (string ToolId, string FallbackUrl) ParseInstallArguments(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
            return ("", "");

        var parts = arguments.Split('|', StringSplitOptions.TrimEntries);
        var toolId = parts.Length > 0 ? parts[0] : "";
        var fallbackUrl = parts.Length > 1 ? parts[1] : "";
        return (toolId, fallbackUrl);
    }

    private List<int> GetEmergencyIndices()
    {
        return _wizardService.AllSteps
            .Select((step, index) => (step, index))
            .Where(x => EmergencyStepIds.Contains(x.step.Id))
            .Select(x => x.index)
            .ToList();
    }

    private bool TryGetNextEmergencyIndex(int fromIndex, out int nextIndex)
    {
        nextIndex = -1;
        var emergencyIndices = GetEmergencyIndices();
        foreach (var index in emergencyIndices)
        {
            if (index > fromIndex)
            {
                nextIndex = index;
                return true;
            }
        }

        return false;
    }

    private bool TryGetPreviousEmergencyIndex(int fromIndex, out int previousIndex)
    {
        previousIndex = -1;
        var emergencyIndices = GetEmergencyIndices();
        for (var i = emergencyIndices.Count - 1; i >= 0; i--)
        {
            if (emergencyIndices[i] < fromIndex)
            {
                previousIndex = emergencyIndices[i];
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// ViewModel für einen einzelnen Wizard-Schritt.
/// </summary>
public partial class StepViewModel : ViewModelBase
{
    private readonly IStep _step;
    private readonly IReadOnlyList<StepToolAction> _orderedToolActions;
    private readonly IReadOnlyList<StepToolAction> _secondaryToolActions;
    private bool _isUpdatingState;
    public event EventHandler<StepStateChangedEventArgs>? StepChanged;

    public StepViewModel(IStep step)
    {
        _step = step;
        _orderedToolActions = step.ToolActions
            .Where(action => !string.IsNullOrWhiteSpace(action.Label) && !string.IsNullOrWhiteSpace(action.Target))
            .GroupBy(action => action.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        _secondaryToolActions = _orderedToolActions.Skip(1).ToList();

        _isUpdatingState = true;
        IsSafetyBackupConfirmed = step.SafetyBackupConfirmed;
        IsSafetyImpactConfirmed = step.SafetyImpactConfirmed;
        IsSafetyRecoveryConfirmed = step.SafetyRecoveryConfirmed;
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
    public string? ImagePath => _step.ImagePath;
    public string Icon => _step.Icon;
    public int ScoreValue => _step.ScoreValue;

    public string WhyImportant => _step.WhyImportant;
    public string WhatItDoes => _step.WhatItDoes;
    public string Risks => _step.Risks;
    public string WhatNotToDo => _step.WhatNotToDo;
    public string RecommendedApproach => _step.RecommendedApproach;
    public string CompactChecklist => _step.RecommendedApproach;
    public string SimpleExplanation => _step.SimpleExplanation;
    public string ExpertDetails => _step.ExpertDetails;
    public IReadOnlyList<StepAction> Actions => _step.Actions;
    public bool HasActions => HasToolActions || Actions.Count > 0;
    public IReadOnlyList<StepToolAction> ToolActions => _orderedToolActions;
    public StepToolAction? PrimaryToolAction => _orderedToolActions.FirstOrDefault();
    public IReadOnlyList<StepToolAction> SecondaryToolActions => _secondaryToolActions;
    public bool HasToolActions => PrimaryToolAction != null;
    public bool HasSecondaryToolActions => _secondaryToolActions.Count > 0;
    public bool RequiresSafetyAcknowledgement
        => _step.RiskLevel is Core.Enums.StepRiskLevel.High or Core.Enums.StepRiskLevel.Critical;
    public bool CanMarkCompleted => !RequiresSafetyAcknowledgement
        || (IsSafetyBackupConfirmed && IsSafetyImpactConfirmed && IsSafetyRecoveryConfirmed);

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
    private bool _isSafetyBackupConfirmed;

    [ObservableProperty]
    private bool _isSafetyImpactConfirmed;

    [ObservableProperty]
    private bool _isSafetyRecoveryConfirmed;

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

    partial void OnIsSafetyBackupConfirmedChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        var previousState = CaptureState();
        _step.SafetyBackupConfirmed = value;
        OnPropertyChanged(nameof(CanMarkCompleted));
        StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
    }

    partial void OnIsSafetyImpactConfirmedChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        var previousState = CaptureState();
        _step.SafetyImpactConfirmed = value;
        OnPropertyChanged(nameof(CanMarkCompleted));
        StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
    }

    partial void OnIsSafetyRecoveryConfirmedChanged(bool value)
    {
        if (_isUpdatingState)
            return;

        var previousState = CaptureState();
        _step.SafetyRecoveryConfirmed = value;
        OnPropertyChanged(nameof(CanMarkCompleted));
        StepChanged?.Invoke(this, new StepStateChangedEventArgs(_step.Id, previousState));
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
        _step.SafetyBackupConfirmed = state.SafetyBackupConfirmed;
        _step.SafetyImpactConfirmed = state.SafetyImpactConfirmed;
        _step.SafetyRecoveryConfirmed = state.SafetyRecoveryConfirmed;

        _isUpdatingState = true;
        try
        {
            UserNote = state.UserNote ?? string.Empty;
            IsCompleted = state.Status == StepStatus.Completed;
            IsSkipped = state.Status == StepStatus.Skipped;
            IsLater = state.Status == StepStatus.Later;
            IsSafetyBackupConfirmed = state.SafetyBackupConfirmed;
            IsSafetyImpactConfirmed = state.SafetyImpactConfirmed;
            IsSafetyRecoveryConfirmed = state.SafetyRecoveryConfirmed;
            OnPropertyChanged(nameof(CanMarkCompleted));
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    private StepStateSnapshot CaptureState()
        => new(
            _step.Status,
            _step.UserNote,
            _step.CompletedAt,
            _step.SafetyBackupConfirmed,
            _step.SafetyImpactConfirmed,
            _step.SafetyRecoveryConfirmed);
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

public readonly record struct StepStateSnapshot(
    StepStatus Status,
    string? UserNote,
    DateTime? CompletedAt,
    bool SafetyBackupConfirmed,
    bool SafetyImpactConfirmed,
    bool SafetyRecoveryConfirmed);
