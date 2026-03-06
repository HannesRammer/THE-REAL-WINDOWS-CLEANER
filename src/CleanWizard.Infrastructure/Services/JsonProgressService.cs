using System.Text.Json;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Infrastructure.Services;

public class JsonProgressService : IProgressService
{
    private const int MaxBackupFiles = 3;
    private static readonly string ProgressFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CleanWizard", "progress.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public async Task SaveAsync(WizardProgress progress)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ProgressFilePath)!);
        progress.LastSavedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(progress, JsonOptions);

        RotateBackups();

        var tempFilePath = $"{ProgressFilePath}.tmp";
        await File.WriteAllTextAsync(tempFilePath, json);

        if (File.Exists(ProgressFilePath))
        {
            File.Replace(tempFilePath, ProgressFilePath, null);
        }
        else
        {
            File.Move(tempFilePath, ProgressFilePath);
        }
    }

    public async Task<WizardProgress?> LoadAsync()
    {
        var primary = await TryLoadFromFileAsync(ProgressFilePath);
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

    private static void RotateBackups()
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

        if (File.Exists(ProgressFilePath))
        {
            File.Copy(ProgressFilePath, GetBackupPath(1));
        }
    }

    private static string GetBackupPath(int index)
        => Path.Combine(
            Path.GetDirectoryName(ProgressFilePath)!,
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
