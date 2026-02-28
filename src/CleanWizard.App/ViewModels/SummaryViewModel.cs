using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.App.ViewModels;

/// <summary>
/// ViewModel für die Zusammenfassungsseite.
/// </summary>
public partial class SummaryViewModel : ViewModelBase
{
    private readonly IWizardService _wizardService;
    private readonly IProgressService _progressService;
    private readonly IExportService _exportService;
    private readonly ILoggingService _loggingService;

    public event EventHandler? RestartRequested;

    [ObservableProperty]
    private int _totalScore;

    [ObservableProperty]
    private int _maxScore;

    [ObservableProperty]
    private double _scorePercent;

    [ObservableProperty]
    private string _scoreRating = "";

    [ObservableProperty]
    private string _scoreColor = "#4CAF50";

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _skippedCount;

    [ObservableProperty]
    private int _laterCount;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private List<StepSummaryItem> _stepSummaries = new();

    [ObservableProperty]
    private string _exportMessage = "";

    public SummaryViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        IExportService exportService,
        ILoggingService loggingService)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _exportService = exportService;
        _loggingService = loggingService;
    }

    public void Refresh()
    {
        TotalScore = _wizardService.CalculateScore();
        MaxScore = _wizardService.MaxScore;
        ScorePercent = MaxScore > 0 ? (double)TotalScore / MaxScore * 100 : 0;

        ScoreRating = ScorePercent switch
        {
            >= 80 => "Optimal",
            >= 60 => "Gut",
            >= 30 => "In Ordnung",
            _ => "Verbesserungswürdig"
        };

        ScoreColor = ScorePercent switch
        {
            >= 80 => "#4CAF50",
            >= 60 => "#8BC34A",
            >= 30 => "#FF9800",
            _ => "#F44336"
        };

        var steps = _wizardService.AllSteps;
        CompletedCount = steps.Count(s => s.Status == StepStatus.Completed);
        SkippedCount = steps.Count(s => s.Status == StepStatus.Skipped);
        LaterCount = steps.Count(s => s.Status == StepStatus.Later);
        PendingCount = steps.Count(s => s.Status == StepStatus.Pending);

        StepSummaries = steps.Select(s => new StepSummaryItem
        {
            Icon = s.Icon,
            Title = s.Title,
            Category = s.Category,
            Status = s.Status,
            StatusText = s.Status switch
            {
                StepStatus.Completed => "✓ Erledigt",
                StepStatus.Skipped => "→ Übersprungen",
                StepStatus.Later => "⏳ Später",
                _ => "○ Ausstehend"
            },
            StatusColor = s.Status switch
            {
                StepStatus.Completed => "#4CAF50",
                StepStatus.Skipped => "#9E9E9E",
                StepStatus.Later => "#FF9800",
                _ => "#607D8B"
            },
            Note = s.UserNote,
            Score = s.Status == StepStatus.Completed ? s.ScoreValue : 0
        }).ToList();
    }

    [RelayCommand]
    private async Task ExportTxtAsync()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"CleanWizard_Bericht_{DateTime.Now:yyyyMMdd_HHmm}.txt");
            var progress = BuildProgress();
            await _exportService.ExportReportAsync(filePath, progress, false);
            ExportMessage = $"✓ Exportiert: {filePath}";
            _loggingService.LogInfo($"Bericht exportiert: {filePath}");
        }
        catch (Exception ex)
        {
            ExportMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportJsonAsync()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"CleanWizard_Bericht_{DateTime.Now:yyyyMMdd_HHmm}.json");
            var progress = BuildProgress();
            await _exportService.ExportReportAsync(filePath, progress, true);
            ExportMessage = $"✓ Exportiert: {filePath}";
        }
        catch (Exception ex)
        {
            ExportMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Restart()
    {
        RestartRequested?.Invoke(this, EventArgs.Empty);
    }

    private WizardProgress BuildProgress()
    {
        return new WizardProgress
        {
            CreatedAt = DateTime.Now,
            TotalScore = TotalScore,
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

public class StepSummaryItem
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public StepStatus Status { get; set; }
    public string StatusText { get; set; } = "";
    public string StatusColor { get; set; } = "";
    public string? Note { get; set; }
    public int Score { get; set; }
}
