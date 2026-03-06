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

    public bool OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            _logger.LogToolLaunched(url);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"URL konnte nicht geöffnet werden: {url} - {ex.Message}");
            return false;
        }
    }

    public bool OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
            _logger.LogToolLaunched($"Ordner: {path}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ordner konnte nicht geöffnet werden: {path} - {ex.Message}");
            return false;
        }
    }

    public bool OpenSettings(string settingsUri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(settingsUri) { UseShellExecute = true });
            _logger.LogToolLaunched($"Einstellungen: {settingsUri}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Einstellungen konnten nicht geöffnet werden: {settingsUri} - {ex.Message}");
            return false;
        }
    }

    public bool LaunchExecutable(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            _logger.LogToolLaunched(path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Programm konnte nicht gestartet werden: {path} - {ex.Message}");
            return false;
        }
    }
}
