using System.Diagnostics;
using System.IO;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Infrastructure.Services;

public sealed class ToolSetupService : IToolSetupService
{
    private readonly IToolLauncherService _toolLauncher;
    private readonly ILoggingService _logger;

    public ToolSetupService(IToolLauncherService toolLauncher, ILoggingService logger)
    {
        _toolLauncher = toolLauncher;
        _logger = logger;
    }

    public ToolAvailabilityResult CheckAvailability(string toolId)
    {
        if (string.IsNullOrWhiteSpace(toolId))
            return new ToolAvailabilityResult { IsInstalled = false, Message = "Tool-ID fehlt." };

        var normalized = toolId.Trim().ToLowerInvariant();
        var installed = normalized switch
        {
            "autoruns" => IsAutorunsInstalled(),
            "malwarebytes" => IsMalwarebytesInstalled(),
            _ => false
        };

        return new ToolAvailabilityResult
        {
            IsInstalled = installed,
            Message = installed ? "Installiert und startbereit" : "Nicht installiert"
        };
    }

    public async Task<ToolInstallResult> InstallAsync(
        string toolId,
        string packageId,
        string fallbackUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolId) || string.IsNullOrWhiteSpace(packageId))
        {
            return new ToolInstallResult
            {
                Success = false,
                Message = "Installationsdaten unvollständig."
            };
        }

        var wingetExists = await HasWingetAsync(cancellationToken);
        if (!wingetExists)
        {
            var fallbackOpened = !string.IsNullOrWhiteSpace(fallbackUrl) && _toolLauncher.OpenUrl(fallbackUrl);
            return new ToolInstallResult
            {
                Success = false,
                UsedFallback = fallbackOpened,
                Message = fallbackOpened
                    ? "winget ist nicht verfügbar. Die offizielle Download-Seite wurde geöffnet."
                    : "winget ist nicht verfügbar und die Download-Seite konnte nicht geöffnet werden."
            };
        }

        try
        {
            var args =
                $"install --id \"{packageId}\" -e --source winget --accept-package-agreements --accept-source-agreements";
            var startInfo = new ProcessStartInfo("winget", args)
            {
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ToolInstallResult
                {
                    Success = false,
                    Message = "Die Installation konnte nicht gestartet werden."
                };
            }

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode == 0)
            {
                _logger.LogInfo($"Tool via winget installiert: {toolId} ({packageId})");
                return new ToolInstallResult
                {
                    Success = true,
                    Message = "Installation abgeschlossen. Prüfe danach den Status und öffne das Tool."
                };
            }

            _logger.LogWarning($"winget install fehlgeschlagen ({packageId}) mit ExitCode {process.ExitCode}");

            var fallbackOpened = !string.IsNullOrWhiteSpace(fallbackUrl) && _toolLauncher.OpenUrl(fallbackUrl);
            return new ToolInstallResult
            {
                Success = false,
                UsedFallback = fallbackOpened,
                Message = fallbackOpened
                    ? "Die Installation ist fehlgeschlagen. Die offizielle Download-Seite wurde geöffnet."
                    : "Die Installation ist fehlgeschlagen."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Tool-Installation fehlgeschlagen: {toolId} - {ex.Message}");
            var fallbackOpened = !string.IsNullOrWhiteSpace(fallbackUrl) && _toolLauncher.OpenUrl(fallbackUrl);
            return new ToolInstallResult
            {
                Success = false,
                UsedFallback = fallbackOpened,
                Message = fallbackOpened
                    ? "Die Installation wurde abgebrochen oder ist fehlgeschlagen. Die Download-Seite wurde geöffnet."
                    : "Die Installation wurde abgebrochen oder ist fehlgeschlagen."
            };
        }
    }

    public bool Launch(string toolId)
    {
        if (string.IsNullOrWhiteSpace(toolId))
            return false;

        var normalized = toolId.Trim().ToLowerInvariant();
        return normalized switch
        {
            "autoruns" => _toolLauncher.LaunchExecutable("autoruns64.exe"),
            "malwarebytes" => _toolLauncher.LaunchExecutable("mbam.exe"),
            _ => false
        };
    }

    private static async Task<bool> HasWingetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var info = new ProcessStartInfo("winget", "--version")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(info);
            if (process == null)
                return false;

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAutorunsInstalled()
    {
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var knownPaths = new[]
        {
            Path.Combine(downloads, "Autoruns", "Autoruns64.exe"),
            Path.Combine(downloads, "Autoruns64.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SysinternalsSuite", "Autoruns64.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SysinternalsSuite", "Autoruns64.exe")
        };

        return knownPaths.Any(File.Exists);
    }

    private static bool IsMalwarebytesInstalled()
    {
        var knownPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Malwarebytes", "Anti-Malware", "mbam.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Malwarebytes", "Anti-Malware", "mbam.exe")
        };

        return knownPaths.Any(File.Exists);
    }
}
