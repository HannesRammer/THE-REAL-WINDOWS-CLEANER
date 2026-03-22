using CleanWizard.Core.Models;

namespace CleanWizard.Core.Interfaces;

public interface IProgressService
{
    Task SaveAsync(WizardProgress progress);
    Task<WizardProgress?> LoadAsync();
}

public interface ILoggingService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogToolLaunched(string toolName);
    Task ExportAsync(string filePath);
    IReadOnlyList<string> GetEntries();
}

public interface ISystemInfoService
{
    Task<SystemInfoModel> CollectAsync();
}

public interface IToolLauncherService
{
    bool OpenUrl(string url);
    bool OpenFolder(string path);
    bool OpenSettings(string settingsUri);
    bool LaunchExecutable(string path);
}

public interface IToolSetupService
{
    ToolAvailabilityResult CheckAvailability(string toolId);
    Task<ToolInstallResult> InstallAsync(string toolId, string packageId, string fallbackUrl, CancellationToken cancellationToken = default);
    bool Launch(string toolId);
}

public interface IPerformanceAnalyzer
{
    Task<PerformanceSnapshot> CaptureAsync();
}

public interface IExportService
{
    Task ExportReportAsync(string filePath, WizardProgress progress, bool asJson = false);
}

public interface IThemeService
{
    void SetTheme(CleanWizard.Core.Enums.AppTheme theme);
    CleanWizard.Core.Enums.AppTheme CurrentTheme { get; }
}
