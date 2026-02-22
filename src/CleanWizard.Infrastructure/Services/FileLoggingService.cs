using CleanWizard.Core.Interfaces;

namespace CleanWizard.Infrastructure.Services;

public class FileLoggingService : ILoggingService
{
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CleanWizard", "logs.txt");

    private readonly List<string> _entries = new();
    private readonly object _lock = new();

    private void Append(string level, string message)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_lock)
        {
            _entries.Add(entry);
        }
    }

    public void LogInfo(string message) => Append("INFO", message);
    public void LogWarning(string message) => Append("WARN", message);
    public void LogError(string message) => Append("ERROR", message);
    public void LogToolLaunched(string toolName) => Append("TOOL", $"Geöffnet: {toolName}");

    public IReadOnlyList<string> GetEntries()
    {
        lock (_lock)
        {
            return _entries.AsReadOnly();
        }
    }

    public async Task ExportAsync(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        IReadOnlyList<string> entries;
        lock (_lock)
        {
            entries = _entries.AsReadOnly();
        }
        await File.WriteAllLinesAsync(filePath, entries);
    }
}
