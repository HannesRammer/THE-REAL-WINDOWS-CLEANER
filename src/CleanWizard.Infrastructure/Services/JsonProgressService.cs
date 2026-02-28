using System.Text.Json;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Infrastructure.Services;

public class JsonProgressService : IProgressService
{
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
        await File.WriteAllTextAsync(ProgressFilePath, json);
    }

    public async Task<WizardProgress?> LoadAsync()
    {
        if (!File.Exists(ProgressFilePath))
            return null;
        try
        {
            var json = await File.ReadAllTextAsync(ProgressFilePath);
            return JsonSerializer.Deserialize<WizardProgress>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
