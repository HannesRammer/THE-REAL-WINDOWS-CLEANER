using CleanWizard.App.ViewModels;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.Core.Tests;

public class SystemCheckViewModelTests
{
    [Fact]
    public async Task LoadSystemInfo_SetsErrorState_WhenServiceFails()
    {
        var sut = new SystemCheckViewModel(
            new ThrowingSystemInfoService(),
            new FakePerformanceAnalyzer());

        await sut.LoadSystemInfoCommand.ExecuteAsync(null);

        Assert.True(sut.HasError);
        Assert.False(sut.CanStartWizard);
        Assert.Contains("fehlgeschlagen", sut.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadSystemInfo_AllowsWizardStart_WhenSuccessful()
    {
        var sut = new SystemCheckViewModel(
            new FakeSystemInfoService(),
            new FakePerformanceAnalyzer());

        await sut.LoadSystemInfoCommand.ExecuteAsync(null);

        Assert.False(sut.HasError);
        Assert.NotNull(sut.SystemInfo);
        Assert.True(sut.CanStartWizard);
    }

    private sealed class ThrowingSystemInfoService : ISystemInfoService
    {
        public Task<SystemInfoModel> CollectAsync()
            => throw new InvalidOperationException("test error");
    }

    private sealed class FakeSystemInfoService : ISystemInfoService
    {
        public Task<SystemInfoModel> CollectAsync()
        {
            return Task.FromResult(new SystemInfoModel
            {
                WindowsVersion = "Windows 11",
                CpuName = "CPU",
                CpuCores = 8,
                RamInGb = 16,
                DriveType = "SSD",
                FreeDiskSpaceBytes = 100L * 1024 * 1024 * 1024,
                AutostartCount = 5,
                RunningProcessCount = 120,
                LastWindowsUpdate = DateTime.Now.AddDays(-3),
                LastMalwareScan = DateTime.Now.AddDays(-5)
            });
        }
    }

    private sealed class FakePerformanceAnalyzer : IPerformanceAnalyzer
    {
        public Task<PerformanceSnapshot> CaptureAsync()
            => Task.FromResult(new PerformanceSnapshot
            {
                AutostartCount = 5,
                CpuUsagePercent = 10,
                FreeDiskSpaceBytes = 100L * 1024 * 1024 * 1024,
                UsedRamBytes = 4L * 1024 * 1024 * 1024
            });
    }
}
