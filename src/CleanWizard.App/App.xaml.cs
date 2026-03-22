using System.IO;
using System.Windows;
using System.Windows.Threading;
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
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CleanWizard",
        "crash.log");
    private bool _isHandlingFatalException;

    protected override void OnStartup(StartupEventArgs e)
    {
        RegisterGlobalExceptionHandlers();

        try
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            HandleFatalException("Startup", ex);
        }
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
        services.AddSingleton<IToolSetupService, ToolSetupService>();
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

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleFatalException("UI-Thread", e.Exception);
        e.Handled = true;
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception
            ?? new Exception(e.ExceptionObject?.ToString() ?? "Unbekannte Ausnahme");
        HandleFatalException("AppDomain", ex);
    }

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleFatalException("TaskScheduler", e.Exception);
        e.SetObserved();
    }

    private void HandleFatalException(string source, Exception ex)
    {
        if (_isHandlingFatalException)
            return;

        _isHandlingFatalException = true;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath)!);
            var message = $"""
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Fatal Error ({source})
{ex}
------------------------------------------------------------
""";
            File.AppendAllText(CrashLogPath, message);
        }
        catch
        {
        }

        try
        {
            MessageBox.Show(
                $"CleanWizard ist abgestürzt.\n\nDetails wurden gespeichert unter:\n{CrashLogPath}",
                "CleanWizard – Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
        }

        try
        {
            Shutdown(-1);
        }
        catch
        {
        }
    }
}
