using CleanWizard.Infrastructure.Services;
using CleanWizard.Core.Interfaces;
using CleanWizard.Modules.Autoruns;
using CleanWizard.Modules.Malwarebytes;
using CleanWizard.Modules.WindowsTools;

namespace CleanWizard.Core.Tests;

public class ToolActionAndPerformanceTests
{
    [Fact]
    public void AllConfiguredSteps_ExposeAtLeastOneToolAction()
    {
        var modules = new IWizardModule[]
        {
            new AutorunsModule(),
            new MalwarebytesModule(),
            new WindowsToolsModule()
        };

        var stepsWithoutActions = modules
            .SelectMany(m => m.Steps)
            .Where(step => step.ToolActions.Count == 0)
            .Select(step => step.Id)
            .ToList();

        Assert.Empty(stepsWithoutActions);
    }

    [Fact]
    public async Task PerformanceAnalyzer_CaptureAsync_ReturnsCpuInExpectedRangeOrFallback()
    {
        var sut = new PerformanceAnalyzer();

        var snapshot = await sut.CaptureAsync();

        Assert.True(snapshot.CpuUsagePercent == -1 || (snapshot.CpuUsagePercent >= 0 && snapshot.CpuUsagePercent <= 100));
    }
}
