using CleanWizard.Core.Enums;

namespace CleanWizard.Core.Models;

public static class AppVersion
{
    public const string Current = "1.1.0";
    public const string Display = "v" + Current + " | CleanWizard";
}

public record StepAction(
    string Label,
    string Icon,
    StepActionType ActionType,
    string Parameter,
    StepActionPriority Priority = StepActionPriority.Primary);

public class WizardProgress
{
    public string Version { get; set; } = AppVersion.Current;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastSavedAt { get; set; }
    public string? CurrentStepId { get; set; }
    public SystemInfoModel? SystemInfo { get; set; }
    public List<StepProgress> Steps { get; set; } = new();
    public int TotalScore { get; set; }
    public ExpertMode Mode { get; set; } = ExpertMode.Simple;
}

public class StepProgress
{
    public string StepId { get; set; } = string.Empty;
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string? Note { get; set; }
    public bool SafetyBackupConfirmed { get; set; }
    public bool SafetyImpactConfirmed { get; set; }
    public bool SafetyRecoveryConfirmed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Score { get; set; }
}

public class SystemInfoModel
{
    public string WindowsVersion { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public string CpuName { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public int RamInGb { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public long FreeDiskSpaceBytes { get; set; }
    public int AutostartCount { get; set; }
    public int RunningProcessCount { get; set; }
    public DateTime? LastWindowsUpdate { get; set; }
    public DateTime? LastMalwareScan { get; set; }
    public string LastMalwareScanSource { get; set; } = string.Empty;
}

public class PerformanceSnapshot
{
    public int AutostartCount { get; set; }
    public double CpuUsagePercent { get; set; }
    public long FreeDiskSpaceBytes { get; set; }
    public long UsedRamBytes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class WarningModel
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public WarningLevel Level { get; set; } = WarningLevel.Info;
}

public class StepToolAction
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepToolActionType ActionType { get; set; } = StepToolActionType.Url;
    public string Target { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? SafetyHint { get; set; }
}

public enum WarningLevel
{
    Info,
    Warning,
    Critical
}

public enum StepToolActionType
{
    Url,
    SettingsUri,
    FolderPath,
    Executable
}
