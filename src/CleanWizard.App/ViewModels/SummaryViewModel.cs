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

    [ObservableProperty]
    private double _cpuBeforePercent;

    [ObservableProperty]
    private double _cpuAfterPercent;

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

        await BuildComparisonAsync(beforeSnapshot);
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
    private async Task ExportLogAsync()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"CleanWizard_Log_{DateTime.Now:yyyyMMdd_HHmm}.txt");
            await _loggingService.ExportAsync(filePath);
            ExportMessage = $"✓ Log exportiert: {filePath}";
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

        FreeDiskBeforeText = $"{ToGb(beforeSnapshot.FreeDiskSpaceBytes):0.0} GB";
        FreeDiskAfterText = $"{ToGb(afterSnapshot.FreeDiskSpaceBytes):0.0} GB";
        var freeDiskDeltaGb = ToGb(afterSnapshot.FreeDiskSpaceBytes - beforeSnapshot.FreeDiskSpaceBytes);
        FreeDiskDeltaText = FormatDelta(freeDiskDeltaGb, unit: " GB", invertGoodDirection: false);
        FreeDiskDeltaColor = GetDeltaColor(freeDiskDeltaGb, invertGoodDirection: false);

        var usedRamBeforeMb = ToMb(beforeSnapshot.UsedRamBytes);
        var usedRamAfterMb = ToMb(afterSnapshot.UsedRamBytes);
        UsedRamBeforeText = $"{usedRamBeforeMb:0} MB";
        UsedRamAfterText = $"{usedRamAfterMb:0} MB";
        var usedRamDeltaMb = usedRamAfterMb - usedRamBeforeMb;
        UsedRamDeltaText = FormatDelta(usedRamDeltaMb, unit: " MB", invertGoodDirection: true);
        UsedRamDeltaColor = GetDeltaColor(usedRamDeltaMb, invertGoodDirection: true);

        CpuBeforeText = $"{beforeSnapshot.CpuUsagePercent:0.#} %";
        CpuAfterText = $"{afterSnapshot.CpuUsagePercent:0.#} %";
        var cpuDelta = afterSnapshot.CpuUsagePercent - beforeSnapshot.CpuUsagePercent;
        CpuDeltaText = FormatDelta(cpuDelta, unit: " %", invertGoodDirection: true);
        CpuDeltaColor = GetDeltaColor(cpuDelta, invertGoodDirection: true);

        // Bar chart percentages (normalize so the larger value = 100%).
        // If both values are equal (or zero), bars remain empty – the gray "±0" delta text
        // already communicates "no meaningful change / no data".
        var maxAutostart = Math.Max(AutostartBefore, AutostartAfter);
        AutostartBeforePercent = maxAutostart > 0 ? (double)AutostartBefore / maxAutostart * 100 : 0;
        AutostartAfterPercent = maxAutostart > 0 ? (double)AutostartAfter / maxAutostart * 100 : 0;

        var maxFreeDisk = Math.Max(beforeSnapshot.FreeDiskSpaceBytes, afterSnapshot.FreeDiskSpaceBytes);
        FreeDiskBeforePercent = maxFreeDisk > 0 ? (double)beforeSnapshot.FreeDiskSpaceBytes / maxFreeDisk * 100 : 0;
        FreeDiskAfterPercent = maxFreeDisk > 0 ? (double)afterSnapshot.FreeDiskSpaceBytes / maxFreeDisk * 100 : 0;

        var maxUsedRam = Math.Max(beforeSnapshot.UsedRamBytes, afterSnapshot.UsedRamBytes);
        UsedRamBeforePercent = maxUsedRam > 0 ? (double)beforeSnapshot.UsedRamBytes / maxUsedRam * 100 : 0;
        UsedRamAfterPercent = maxUsedRam > 0 ? (double)afterSnapshot.UsedRamBytes / maxUsedRam * 100 : 0;

        var maxCpu = Math.Max(beforeSnapshot.CpuUsagePercent, afterSnapshot.CpuUsagePercent);
        CpuBeforePercent = maxCpu > 0 ? beforeSnapshot.CpuUsagePercent / maxCpu * 100 : 0;
        CpuAfterPercent = maxCpu > 0 ? afterSnapshot.CpuUsagePercent / maxCpu * 100 : 0;
    }

    private static double ToGb(long bytes) => bytes / 1024d / 1024d / 1024d;
    private static double ToMb(long bytes) => bytes / 1024d / 1024d;

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
