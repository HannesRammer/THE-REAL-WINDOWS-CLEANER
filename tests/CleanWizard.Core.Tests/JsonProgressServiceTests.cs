using CleanWizard.Core.Models;
using CleanWizard.Infrastructure.Services;

namespace CleanWizard.Core.Tests;

public class JsonProgressServiceTests
{
    [Fact]
    public async Task LoadAsync_ReturnsNull_WhenNoProgressExists()
    {
        var path = CreateTempProgressPath();
        var sut = new JsonProgressService(path);

        var result = await sut.LoadAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_RotatesBackups_AndKeepsMaxThreeFiles()
    {
        var path = CreateTempProgressPath();
        var dir = Path.GetDirectoryName(path)!;
        var sut = new JsonProgressService(path);

        for (var i = 1; i <= 5; i++)
        {
            await sut.SaveAsync(new WizardProgress
            {
                TotalScore = i,
                Steps = new List<StepProgress>
                {
                    new() { StepId = $"step_{i}", Score = i }
                }
            });
        }

        Assert.True(File.Exists(path));
        Assert.True(File.Exists(Path.Combine(dir, "progress.backup.1.json")));
        Assert.True(File.Exists(Path.Combine(dir, "progress.backup.2.json")));
        Assert.True(File.Exists(Path.Combine(dir, "progress.backup.3.json")));
        Assert.False(File.Exists(Path.Combine(dir, "progress.backup.4.json")));
    }

    [Fact]
    public async Task LoadAsync_FallsBackToBackup_WhenPrimaryIsCorrupted()
    {
        var path = CreateTempProgressPath();
        var sut = new JsonProgressService(path);

        await sut.SaveAsync(new WizardProgress { TotalScore = 11 });
        await sut.SaveAsync(new WizardProgress { TotalScore = 22 });

        await File.WriteAllTextAsync(path, "{broken-json");

        var loaded = await sut.LoadAsync();

        Assert.NotNull(loaded);
        Assert.Equal(11, loaded!.TotalScore);
    }

    private static string CreateTempProgressPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "CleanWizardTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "progress.json");
    }
}
