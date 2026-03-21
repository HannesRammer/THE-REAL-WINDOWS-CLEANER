using System.Diagnostics;
using System.IO;
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
        var expanded = Environment.ExpandEnvironmentVariables(path);
        Exception? lastException = null;

        foreach (var candidate in ResolveExecutableCandidates(expanded))
        {
            try
            {
                Process.Start(new ProcessStartInfo(candidate) { UseShellExecute = true });
                _logger.LogToolLaunched(candidate);
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        _logger.LogError($"Programm konnte nicht gestartet werden: {path} - {lastException?.Message ?? "Unbekannter Fehler"}");
        return false;
    }

    private static IEnumerable<string> ResolveExecutableCandidates(string target)
    {
        var candidates = new List<string> { target };

        if (target.Equals("mbam.exe", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Malwarebytes", "Anti-Malware", "mbam.exe"));
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Malwarebytes", "Anti-Malware", "mbam.exe"));
        }

        if (target.Equals("autoruns64.exe", StringComparison.OrdinalIgnoreCase))
        {
            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            candidates.Add(Path.Combine(downloads, "Autoruns", "Autoruns64.exe"));
            candidates.Add(Path.Combine(downloads, "Autoruns64.exe"));
            candidates.Add("autoruns.exe");
        }

        return candidates
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
