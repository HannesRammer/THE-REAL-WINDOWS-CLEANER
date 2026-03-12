using System.Text.Json;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Infrastructure.Services;

public class JsonProgressService : IProgressService
{
    private const int MaxBackupFiles = 3;
    private static readonly string DefaultProgressFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CleanWizard", "progress.json");
    private readonly string _progressFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public JsonProgressService(string? progressFilePath = null)
    {
        _progressFilePath = progressFilePath ?? DefaultProgressFilePath;
    }

    public async Task SaveAsync(WizardProgress progress)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_progressFilePath)!);
        progress.LastSavedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(progress, JsonOptions);

        RotateBackups();

        var tempFilePath = $"{_progressFilePath}.tmp";
        await File.WriteAllTextAsync(tempFilePath, json);

        if (File.Exists(_progressFilePath))
        {
            File.Replace(tempFilePath, _progressFilePath, null);
        }
        else
        {
            File.Move(tempFilePath, _progressFilePath);
        }
    }

    public async Task<WizardProgress?> LoadAsync()
    {
        var primary = await TryLoadFromFileAsync(_progressFilePath);
        if (primary != null)
            return primary;

        for (var i = 1; i <= MaxBackupFiles; i++)
        {
            var backup = await TryLoadFromFileAsync(GetBackupPath(i));
            if (backup != null)
                return backup;
        }

        return null;
    }

    private void RotateBackups()
    {
        for (var i = MaxBackupFiles; i >= 1; i--)
        {
            var currentBackup = GetBackupPath(i);
            if (!File.Exists(currentBackup))
                continue;

            if (i == MaxBackupFiles)
            {
                File.Delete(currentBackup);
            }
            else
            {
                var nextBackup = GetBackupPath(i + 1);
                File.Move(currentBackup, nextBackup);
            }
        }

        if (File.Exists(_progressFilePath))
        {
            File.Copy(_progressFilePath, GetBackupPath(1));
        }
    }

    private string GetBackupPath(int index)
        => Path.Combine(
            Path.GetDirectoryName(_progressFilePath)!,
            $"progress.backup.{index}.json");

    private static async Task<WizardProgress?> TryLoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<WizardProgress>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
