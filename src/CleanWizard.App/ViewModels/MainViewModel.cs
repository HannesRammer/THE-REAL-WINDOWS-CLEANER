using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CleanWizard.Core.Enums;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.App.ViewModels;

/// <summary>
/// Basis-ViewModel für alle ViewModels der App.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}

/// <summary>
/// Haupt-ViewModel – steuert die Navigation zwischen den Hauptbereichen.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IWizardService _wizardService;
    private readonly IProgressService _progressService;
    private readonly ILoggingService _loggingService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.Dark;

    [ObservableProperty]
    private ExpertMode _expertMode = ExpertMode.Simple;

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private bool _isSimpleMode = true;

    [ObservableProperty]
    private bool _isExpertMode = false;

    public SystemCheckViewModel SystemCheckViewModel { get; }
    public WizardViewModel WizardViewModel { get; }
    public SummaryViewModel SummaryViewModel { get; }

    public MainViewModel(
        IWizardService wizardService,
        IProgressService progressService,
        ILoggingService loggingService,
        SystemCheckViewModel systemCheckViewModel,
        WizardViewModel wizardViewModel,
        SummaryViewModel summaryViewModel)
    {
        _wizardService = wizardService;
        _progressService = progressService;
        _loggingService = loggingService;
        SystemCheckViewModel = systemCheckViewModel;
        WizardViewModel = wizardViewModel;
        SummaryViewModel = summaryViewModel;

        // Start with system check
        CurrentView = SystemCheckViewModel;

        // Wire up navigation
        SystemCheckViewModel.StartWizardRequested += (_, _) => NavigateToWizard();
        WizardViewModel.WizardCompleted += (_, _) => NavigateToSummary();
        SummaryViewModel.RestartRequested += (_, _) => NavigateToSystemCheck();
    }

    [RelayCommand]
    private void NavigateSystemCheck() => NavigateToSystemCheck();

    [RelayCommand]
    private void NavigateWizard() => NavigateToWizard();

    [RelayCommand]
    private void NavigateSummary() => NavigateToSummary();

    private void NavigateToWizard()
    {
        CurrentView = WizardViewModel;
        _loggingService.LogInfo("Wizard gestartet");
    }

    private void NavigateToSummary()
    {
        SummaryViewModel.Refresh();
        CurrentView = SummaryViewModel;
        _loggingService.LogInfo("Zusammenfassung angezeigt");
    }

    private void NavigateToSystemCheck()
    {
        CurrentView = SystemCheckViewModel;
    }

    partial void OnIsSimpleModeChanged(bool value)
    {
        if (value)
        {
            IsExpertMode = false;
            ExpertMode = ExpertMode.Simple;
            _wizardService.CurrentMode = ExpertMode.Simple;
            WizardViewModel.RefreshStep();
        }
    }

    partial void OnIsExpertModeChanged(bool value)
    {
        if (value)
        {
            IsSimpleMode = false;
            ExpertMode = ExpertMode.Expert;
            _wizardService.CurrentMode = ExpertMode.Expert;
            WizardViewModel.RefreshStep();
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        CurrentTheme = value ? AppTheme.Dark : AppTheme.Light;
        ApplyTheme(CurrentTheme);
    }

    private static void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        var dict = app.Resources.MergedDictionaries;
        var colorDict = dict.FirstOrDefault(d => d.Source?.OriginalString.Contains("Colors") == true);
        if (colorDict != null) dict.Remove(colorDict);

        var source = theme == AppTheme.Dark
            ? new Uri("Styles/Colors.Dark.xaml", UriKind.Relative)
            : new Uri("Styles/Colors.xaml", UriKind.Relative);

        dict.Insert(0, new System.Windows.ResourceDictionary { Source = source });
    }
}
