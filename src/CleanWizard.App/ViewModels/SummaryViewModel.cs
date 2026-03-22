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
    private readonly IPerformanceAnalyzer _performanceAnalyzer;

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
    private string _nextStepsTitle = "";

    [ObservableProperty]
    private string _nextStepsIntro = "";

    [ObservableProperty]
    private List<NextStepItem> _nextSteps = new();

    [ObservableProperty]
    private List<StepSummaryItem> _stepSummaries = new();

    [ObservableProperty]
    private string _exportMessage = "";

    [ObservableProperty]
    private bool _hasComparisonData;

    [ObservableProperty]
    private int _autostartBefore;

    [ObservableProperty]
    private int _autostartAfter;

    [ObservableProperty]
    private string _autostartDeltaText = "Kein Vergleich";

    [ObservableProperty]
    private string _autostartDeltaColor = "#9E9E9E";

    [ObservableProperty]
    private string _freeDiskBeforeText = "-";

    [ObservableProperty]
    private string _freeDiskAfterText = "-";

    [ObservableProperty]
    private string _freeDiskDeltaText = "Kein Vergleich";

    [ObservableProperty]
    private string _freeDiskDeltaColor = "#9E9E9E";

    [ObservableProperty]
    private string _usedRamBeforeText = "-";

    [ObservableProperty]
    private string _usedRamAfterText = "-";

    [ObservableProperty]
    private string _usedRamDeltaText = "Kein Vergleich";

    [ObservableProperty]
    private string _usedRamDeltaColor = "#9E9E9E";

    [ObservableProperty]
    private string _cpuBeforeText = "-";

    [ObservableProperty]
    private string _cpuAfterText = "-";

    [ObservableProperty]
    private string _cpuDeltaText = "Kein Vergleich";

    [ObservableProperty]
    private string _cpuDeltaColor = "#9E9E9E";

    [ObservableProperty]
    private double _cpuBeforePercent;

    [ObservableProperty]
    private double _cpuAfterPercent;

    [ObservableProperty]
    private double _autostartBeforePercent;

    [ObservableProperty]
    private double _autostartAfterPercent;

    [ObservableProperty]
    private double _freeDiskBeforePercent;

    [ObservableProperty]
    private double _freeDiskAfterPercent;

    [ObservableProperty]
    private double _usedRamBeforePercent;

    [ObservableProperty]
    private double _usedRamAfterPercent;

    public bool HasNextSteps => NextSteps.Count > 0;

    public SummaryViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        IExportService exportService,
        ILoggingService loggingService,
        IPerformanceAnalyzer performanceAnalyzer)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _exportService = exportService;
        _loggingService = loggingService;
        _performanceAnalyzer = performanceAnalyzer;
    }

    public async Task RefreshAsync(PerformanceSnapshot? beforeSnapshot)
    {
        TotalScore = _wizardService.CalculateScore();
        MaxScore = _wizardService.MaxScore;
        ScorePercent = MaxScore > 0 ? (double)TotalScore / MaxScore * 100 : 0;

        ScoreRating = ScorePercent switch
        {
            >= 80 => "Sehr gut",
            >= 60 => "Solide",
            >= 30 => "Teilweise erledigt",
            _ => "Offen"
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
        BuildNextSteps(steps);
        OnPropertyChanged(nameof(HasNextSteps));

        StepSummaries = steps.Select(s => new StepSummaryItem
        {
            Icon = s.Icon,
            Title = s.Title,
            Category = s.Category,
            Status = s.Status,
            StatusText = s.Status switch
            {
                StepStatus.Completed => "Erledigt",
                StepStatus.Skipped => "Übersprungen",
                StepStatus.Later => "Später",
                _ => "Offen"
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

        await BuildComparisonAsync(beforeSnapshot);
    }

    private void BuildNextSteps(IReadOnlyList<IStep> steps)
    {
        var actionable = steps
            .Where(s => s.Status is StepStatus.Later or StepStatus.Pending or StepStatus.Skipped)
            .OrderBy(s => s.Status == StepStatus.Later ? 0 : s.Status == StepStatus.Pending ? 1 : 2)
            .ThenByDescending(s => s.ScoreValue)
            .Take(3)
            .Select(s => new NextStepItem
            {
                Title = s.Title,
                Category = s.Category,
                StatusText = s.Status switch
                {
                    StepStatus.Later => "Für später vorgemerkt",
                    StepStatus.Skipped => "Übersprungen",
                    _ => "Noch offen"
                },
                Hint = s.Status switch
                {
                    StepStatus.Later => "Hier wolltest du bewusst später weitermachen.",
                    StepStatus.Skipped => "Nur nachholen, wenn dieser Schritt für deinen PC noch relevant ist.",
                    _ => "Das ist ein sinnvoller nächster Schritt, wenn du weitermachen willst."
                }
            })
            .ToList();

        if (actionable.Count == 0)
        {
            NextStepsTitle = "Alles Wesentliche ist erledigt";
            NextStepsIntro = "Du hast aktuell keine offenen oder zurückgestellten Schritte mehr im Assistenten.";
            NextSteps = new List<NextStepItem>();
            return;
        }

        NextStepsTitle = "Sinnvolle nächste Schritte";
        NextStepsIntro = "Wenn du weiter aufräumen willst, beginne mit diesen Punkten.";
        NextSteps = actionable;
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
            ExportMessage = $"Export erstellt: {filePath}";
            _loggingService.LogInfo($"Bericht exportiert: {filePath}");
        }
        catch (Exception ex)
        {
            ExportMessage = $"Export fehlgeschlagen: {ex.Message}";
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
            ExportMessage = $"Export erstellt: {filePath}";
        }
        catch (Exception ex)
        {
            ExportMessage = $"Export fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportLogAsync()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"CleanWizard_Log_{DateTime.Now:yyyyMMdd_HHmm}.txt");
            await _loggingService.ExportAsync(filePath);
            ExportMessage = $"Protokoll exportiert: {filePath}";
        }
        catch (Exception ex)
        {
            ExportMessage = $"Export fehlgeschlagen: {ex.Message}";
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
            CurrentStepId = _wizardService.CurrentStep?.Id,
            TotalScore = TotalScore,
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

    private async Task BuildComparisonAsync(PerformanceSnapshot? beforeSnapshot)
    {
        if (beforeSnapshot == null)
        {
            HasComparisonData = false;
            return;
        }

        var afterSnapshot = await _performanceAnalyzer.CaptureAsync();
        HasComparisonData = true;

        AutostartBefore = beforeSnapshot.AutostartCount;
        AutostartAfter = afterSnapshot.AutostartCount;
        var autostartDelta = afterSnapshot.AutostartCount - beforeSnapshot.AutostartCount;
        AutostartDeltaText = FormatDelta(autostartDelta, unit: "", invertGoodDirection: true);
        AutostartDeltaColor = GetDeltaColor(autostartDelta, invertGoodDirection: true);
        (AutostartBeforePercent, AutostartAfterPercent) = NormalizePair(beforeSnapshot.AutostartCount, afterSnapshot.AutostartCount);

        FreeDiskBeforeText = $"{ToGb(beforeSnapshot.FreeDiskSpaceBytes):0.0} GB";
        FreeDiskAfterText = $"{ToGb(afterSnapshot.FreeDiskSpaceBytes):0.0} GB";
        var freeDiskDeltaGb = ToGb(afterSnapshot.FreeDiskSpaceBytes - beforeSnapshot.FreeDiskSpaceBytes);
        FreeDiskDeltaText = FormatDelta(freeDiskDeltaGb, unit: " GB", invertGoodDirection: false);
        FreeDiskDeltaColor = GetDeltaColor(freeDiskDeltaGb, invertGoodDirection: false);
        (FreeDiskBeforePercent, FreeDiskAfterPercent) = NormalizePair(beforeSnapshot.FreeDiskSpaceBytes, afterSnapshot.FreeDiskSpaceBytes);

        var usedRamBeforeMb = ToMb(beforeSnapshot.UsedRamBytes);
        var usedRamAfterMb = ToMb(afterSnapshot.UsedRamBytes);
        UsedRamBeforeText = $"{usedRamBeforeMb:0} MB";
        UsedRamAfterText = $"{usedRamAfterMb:0} MB";
        var usedRamDeltaMb = usedRamAfterMb - usedRamBeforeMb;
        UsedRamDeltaText = FormatDelta(usedRamDeltaMb, unit: " MB", invertGoodDirection: true);
        UsedRamDeltaColor = GetDeltaColor(usedRamDeltaMb, invertGoodDirection: true);
        (UsedRamBeforePercent, UsedRamAfterPercent) = NormalizePair(beforeSnapshot.UsedRamBytes, afterSnapshot.UsedRamBytes);

        if (beforeSnapshot.CpuUsagePercent < 0 || afterSnapshot.CpuUsagePercent < 0)
        {
            CpuBeforeText = "Nicht verfügbar";
            CpuAfterText = "Nicht verfügbar";
            CpuDeltaText = "Kein Vergleich";
            CpuDeltaColor = "#9E9E9E";
            CpuBeforePercent = 0;
            CpuAfterPercent = 0;
            return;
        }

        CpuBeforeText = $"{beforeSnapshot.CpuUsagePercent:0.0}%";
        CpuAfterText = $"{afterSnapshot.CpuUsagePercent:0.0}%";
        var cpuDelta = afterSnapshot.CpuUsagePercent - beforeSnapshot.CpuUsagePercent;
        CpuDeltaText = FormatDelta(cpuDelta, unit: "%", invertGoodDirection: true);
        CpuDeltaColor = GetDeltaColor(cpuDelta, invertGoodDirection: true);
        CpuBeforePercent = ClampPercent(beforeSnapshot.CpuUsagePercent);
        CpuAfterPercent = ClampPercent(afterSnapshot.CpuUsagePercent);
    }

    private static double ToGb(long bytes) => bytes / 1024d / 1024d / 1024d;
    private static double ToMb(long bytes) => bytes / 1024d / 1024d;
    private static double ClampPercent(double value) => Math.Max(0, Math.Min(100, value));

    private static (double BeforePercent, double AfterPercent) NormalizePair(double before, double after)
    {
        var safeBefore = Math.Max(0, before);
        var safeAfter = Math.Max(0, after);
        var max = Math.Max(1d, Math.Max(safeBefore, safeAfter));
        return (safeBefore / max * 100d, safeAfter / max * 100d);
    }

    private static string FormatDelta(double value, string unit, bool invertGoodDirection)
    {
        if (Math.Abs(value) < 0.001)
            return "±0";

        var prefix = value > 0 ? "+" : "";
        return $"{prefix}{value:0.##}{unit}";
    }

    private static string GetDeltaColor(double value, bool invertGoodDirection)
    {
        if (Math.Abs(value) < 0.001)
            return "#9E9E9E";

        var good = invertGoodDirection ? value < 0 : value > 0;
        return good ? "#2E7D32" : "#C62828";
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

public class NextStepItem
{
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string Hint { get; set; } = "";
}
