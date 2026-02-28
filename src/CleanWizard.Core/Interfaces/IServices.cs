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
    void OpenUrl(string url);
    void OpenFolder(string path);
    void OpenSettings(string settingsUri);
    void LaunchExecutable(string path);
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
