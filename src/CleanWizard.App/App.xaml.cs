using System.Windows;
using CleanWizard.App.ViewModels;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Services;
using CleanWizard.Infrastructure.Services;
using CleanWizard.Modules.Autoruns;
using CleanWizard.Modules.Malwarebytes;
using CleanWizard.Modules.WindowsTools;
using Microsoft.Extensions.DependencyInjection;

namespace CleanWizard.App;

/// <summary>
/// Interaction logic for App.xaml – DI-Konfiguration und Startlogik.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Module
        services.AddSingleton<IWizardModule, AutorunsModule>();
        services.AddSingleton<IWizardModule, MalwarebytesModule>();
        services.AddSingleton<IWizardModule, WindowsToolsModule>();

        // Core Services
        services.AddSingleton<IWizardService>(sp =>
            new WizardService(sp.GetServices<IWizardModule>()));

        // Infrastructure Services
        services.AddSingleton<ILoggingService, FileLoggingService>();
        services.AddSingleton<IProgressService, JsonProgressService>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddSingleton<IToolLauncherService, ToolLauncherService>();
        services.AddSingleton<IPerformanceAnalyzer, PerformanceAnalyzer>();
        services.AddSingleton<IExportService, ExportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SystemCheckViewModel>();
        services.AddSingleton<WizardViewModel>();
        services.AddSingleton<SummaryViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            var mainViewModel = _serviceProvider?.GetService<MainViewModel>();
            mainViewModel?.SaveProgressOnExitAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // OnExit darf nicht abstürzen, wenn Auto-Save fehlschlägt.
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

