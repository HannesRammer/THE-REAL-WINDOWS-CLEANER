using System.Text;
using System.Text.Json;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Infrastructure.Services;

public class ExportService : IExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public async Task ExportReportAsync(string filePath, WizardProgress progress, bool asJson = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

        if (asJson)
        {
            var json = JsonSerializer.Serialize(progress, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("  CleanWizard – Systembereinigungsbericht");
            sb.AppendLine("========================================");
            sb.AppendLine($"Erstellt am: {progress.CreatedAt:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Modus: {(progress.Mode == CleanWizard.Core.Enums.ExpertMode.Expert ? "Experte" : "Einfach")}");
            sb.AppendLine($"Gesamtpunktzahl: {progress.TotalScore}");
            sb.AppendLine();

            if (progress.SystemInfo != null)
            {
                sb.AppendLine("--- Systeminfo ---");
                sb.AppendLine($"Windows: {progress.SystemInfo.WindowsVersion}");
                sb.AppendLine($"CPU: {progress.SystemInfo.CpuName}");
                sb.AppendLine($"RAM: {progress.SystemInfo.RamInGb} GB");
                sb.AppendLine($"Laufwerkstyp: {progress.SystemInfo.DriveType}");
                sb.AppendLine($"Freier Speicher: {progress.SystemInfo.FreeDiskSpaceBytes / 1024 / 1024 / 1024} GB");
                sb.AppendLine($"Autostart-Einträge: {progress.SystemInfo.AutostartCount}");
                sb.AppendLine();
            }

            sb.AppendLine("--- Schritte ---");
            foreach (var step in progress.Steps)
            {
                var statusText = step.Status switch
                {
                    Core.Enums.StepStatus.Completed => "✓ Erledigt",
                    Core.Enums.StepStatus.Skipped => "→ Übersprungen",
                    Core.Enums.StepStatus.Later => "⏳ Später",
                    _ => "○ Ausstehend"
                };
                sb.AppendLine($"{statusText} | {step.StepId} | Punkte: {step.Score}");
                if (!string.IsNullOrWhiteSpace(step.Note))
                    sb.AppendLine($"   Notiz: {step.Note}");
                if (step.CompletedAt.HasValue)
                    sb.AppendLine($"   Erledigt am: {step.CompletedAt:dd.MM.yyyy HH:mm}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
