using CleanWizard.Core.Interfaces;
using CleanWizard.Infrastructure.Services;

namespace CleanWizard.Core.Tests;

public class ToolSetupServiceTests
{
    [Fact]
    public void CheckAvailability_UnknownTool_ReturnsNotInstalled()
    {
        var sut = new ToolSetupService(new FakeToolLauncher(), new FakeLogger());

        var result = sut.CheckAvailability("unknown-tool");

        Assert.False(result.IsInstalled);
        Assert.Equal("Nicht installiert", result.Message);
    }

    [Fact]
    public async Task InstallAsync_MissingData_ReturnsFailure()
    {
        var sut = new ToolSetupService(new FakeToolLauncher(), new FakeLogger());

        var result = await sut.InstallAsync("", "", "");

        Assert.False(result.Success);
        Assert.Contains("unvollständig", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Launch_UnknownTool_ReturnsFalse()
    {
        var sut = new ToolSetupService(new FakeToolLauncher(), new FakeLogger());

        var launched = sut.Launch("unknown-tool");

        Assert.False(launched);
    }

    private sealed class FakeToolLauncher : IToolLauncherService
    {
        public bool OpenUrl(string url) => true;
        public bool OpenFolder(string path) => true;
        public bool OpenSettings(string settingsUri) => true;
        public bool LaunchExecutable(string path) => false;
    }

    private sealed class FakeLogger : ILoggingService
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogToolLaunched(string toolName) { }
        public Task ExportAsync(string filePath) => Task.CompletedTask;
        public IReadOnlyList<string> GetEntries() => Array.Empty<string>();
    }
}
