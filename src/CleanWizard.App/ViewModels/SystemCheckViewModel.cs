using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;

namespace CleanWizard.App.ViewModels;

/// <summary>
/// ViewModel für die Startseite mit System-Check.
/// </summary>
public partial class SystemCheckViewModel : ViewModelBase
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly IPerformanceAnalyzer _performanceAnalyzer;

    public event EventHandler? StartWizardRequested;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private SystemInfoModel? _systemInfo;

    [ObservableProperty]
    private PerformanceSnapshot? _performanceSnapshot;

    [ObservableProperty]
    private string _systemStatusColor = "#4CAF50";

    [ObservableProperty]
    private string _systemStatusText = "Wird analysiert...";

    [ObservableProperty]
    private string _recommendationText = "";

    [ObservableProperty]
    private bool _isEmergencyMode = false;

    public string WindowsVersionText => SystemInfo?.WindowsVersion ?? "Unbekannt";
    public string CpuText => $"{SystemInfo?.CpuName ?? "Unbekannt"} ({SystemInfo?.CpuCores ?? 0} Kerne)";
    public string RamText => $"{SystemInfo?.RamInGb ?? 0} GB";
    public string DriveTypeText => SystemInfo?.DriveType ?? "Unbekannt";
    public string FreeDiskText => $"{SystemInfo?.FreeDiskSpaceBytes / 1024 / 1024 / 1024 ?? 0} GB frei";
    public string AutostartCountText => $"{SystemInfo?.AutostartCount ?? 0} Einträge";
    public string RunningProcessesText => $"{SystemInfo?.RunningProcessCount ?? 0} Prozesse";

    public SystemCheckViewModel(
        ISystemInfoService systemInfoService,
        IPerformanceAnalyzer performanceAnalyzer)
    {
        _systemInfoService = systemInfoService;
        _performanceAnalyzer = performanceAnalyzer;
    }

    [RelayCommand]
    private async Task LoadSystemInfoAsync()
    {
        IsLoading = true;
        try
        {
            SystemInfo = await _systemInfoService.CollectAsync();
            PerformanceSnapshot = await _performanceAnalyzer.CaptureAsync();
            EvaluateSystem();
            OnPropertyChanged(nameof(WindowsVersionText));
            OnPropertyChanged(nameof(CpuText));
            OnPropertyChanged(nameof(RamText));
            OnPropertyChanged(nameof(DriveTypeText));
            OnPropertyChanged(nameof(FreeDiskText));
            OnPropertyChanged(nameof(AutostartCountText));
            OnPropertyChanged(nameof(RunningProcessesText));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void EvaluateSystem()
    {
        if (SystemInfo == null) return;

        var issues = 0;

        if (SystemInfo.AutostartCount > 20) issues++;
        if (SystemInfo.RamInGb < 4) issues++;
        if (SystemInfo.FreeDiskSpaceBytes < 5L * 1024 * 1024 * 1024) issues++;

        IsEmergencyMode = SystemInfo.AutostartCount > 30 || SystemInfo.RamInGb < 4;

        if (issues == 0)
        {
            SystemStatusColor = "#4CAF50";
            SystemStatusText = "Gut";
            RecommendationText = "Dein System sieht gut aus. Führe trotzdem die Schritte durch um es optimal zu halten.";
        }
        else if (issues == 1)
        {
            SystemStatusColor = "#FF9800";
            SystemStatusText = "Optimierbar";
            RecommendationText = $"Es gibt {issues} Bereich der verbessert werden kann. Besonders Schritt 1 (Autoruns) empfohlen.";
        }
        else
        {
            SystemStatusColor = "#F44336";
            SystemStatusText = "Kritisch";
            RecommendationText = "Mehrere kritische Bereiche gefunden! Empfehle den Notfall-Modus zu starten.";
        }

        if (IsEmergencyMode)
        {
            RecommendationText = "⚠️ Dein System ist stark belastet! Starte zuerst den Notfall-Schnellcheck.";
        }
    }

    [RelayCommand]
    private void StartWizard()
    {
        StartWizardRequested?.Invoke(this, EventArgs.Empty);
    }
}
