using System.Diagnostics;
using CleanWizard.Core.Interfaces;

namespace CleanWizard.Infrastructure.Services;

public class ToolLauncherService : IToolLauncherService
{
    private readonly ILoggingService _logger;

    public ToolLauncherService(ILoggingService logger)
    {
        _logger = logger;
    }

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            _logger.LogToolLaunched(url);
        }
        catch (Exception ex)
        {
            _logger.LogError($"URL konnte nicht geöffnet werden: {url} - {ex.Message}");
        }
    }

    public void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
            _logger.LogToolLaunched($"Ordner: {path}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ordner konnte nicht geöffnet werden: {path} - {ex.Message}");
        }
    }

    public void OpenSettings(string settingsUri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(settingsUri) { UseShellExecute = true });
            _logger.LogToolLaunched($"Einstellungen: {settingsUri}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Einstellungen konnten nicht geöffnet werden: {settingsUri} - {ex.Message}");
        }
    }

    public void LaunchExecutable(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            _logger.LogToolLaunched(path);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Programm konnte nicht gestartet werden: {path} - {ex.Message}");
        }
    }
}
