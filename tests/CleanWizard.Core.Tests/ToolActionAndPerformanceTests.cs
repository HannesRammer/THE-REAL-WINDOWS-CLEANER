using CleanWizard.Infrastructure.Services;
using CleanWizard.Core.Enums;
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

    [Fact]
    public void Steps_WithMultipleActions_ExposePrimaryActionFirst()
    {
        var modules = new IWizardModule[]
        {
            new AutorunsModule(),
            new MalwarebytesModule(),
            new WindowsToolsModule()
        };

        var stepsWithMultipleActions = modules
            .SelectMany(m => m.Steps)
            .Where(step => step.Actions.Count > 1)
            .ToList();

        Assert.NotEmpty(stepsWithMultipleActions);
        Assert.All(stepsWithMultipleActions, step =>
        {
            Assert.Equal(StepActionPriority.Primary, step.Actions[0].Priority);
            Assert.All(step.Actions.Skip(1), action => Assert.Equal(StepActionPriority.Secondary, action.Priority));
        });
    }
}
