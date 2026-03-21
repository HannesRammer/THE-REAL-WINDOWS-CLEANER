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
    public string LastWindowsUpdateText => SystemInfo?.LastWindowsUpdate?.ToString("dd.MM.yyyy HH:mm") ?? "Nicht erkannt";
    public string LastMalwareScanText => SystemInfo?.LastMalwareScan?.ToString("dd.MM.yyyy HH:mm") ?? "Nicht erkannt";

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
            OnPropertyChanged(nameof(LastWindowsUpdateText));
            OnPropertyChanged(nameof(LastMalwareScanText));
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
        if (SystemInfo.LastMalwareScan == null || SystemInfo.LastMalwareScan < DateTime.Now.AddDays(-30)) issues++;
        if (SystemInfo.LastWindowsUpdate == null || SystemInfo.LastWindowsUpdate < DateTime.Now.AddDays(-21)) issues++;
        if (PerformanceSnapshot?.CpuUsagePercent > 75) issues++;

        IsEmergencyMode = SystemInfo.AutostartCount > 30 || SystemInfo.RamInGb < 4;

        var recommendAutoruns = SystemInfo.AutostartCount > 20;
        var recommendMalware = SystemInfo.LastMalwareScan == null || SystemInfo.LastMalwareScan < DateTime.Now.AddDays(-30);
        var recommendWindowsTools =
            SystemInfo.FreeDiskSpaceBytes < 15L * 1024 * 1024 * 1024 ||
            SystemInfo.LastWindowsUpdate == null ||
            SystemInfo.LastWindowsUpdate < DateTime.Now.AddDays(-21);

        if (issues == 0)
        {
            SystemStatusColor = "#4CAF50";
            SystemStatusText = "Gut";
            RecommendationText = "Dein System sieht gut aus. Starte normal im Wizard für Feinschliff.";
        }
        else if (issues == 1)
        {
            SystemStatusColor = "#FF9800";
            SystemStatusText = "Optimierbar";
            RecommendationText = "Ein Bereich sollte priorisiert werden.";
        }
        else
        {
            SystemStatusColor = "#F44336";
            SystemStatusText = "Kritisch";
            RecommendationText = "Mehrere kritische Bereiche gefunden.";
        }

        if (IsEmergencyMode)
        {
            RecommendationText = "⚠️ Dein System ist stark belastet! Starte zuerst den Notfall-Schnellcheck.";
            return;
        }

        if (recommendAutoruns)
        {
            RecommendationText = "Empfehlung: Starte mit Autoruns, da viele Autostart-Einträge erkannt wurden.";
            return;
        }

        if (recommendMalware)
        {
            RecommendationText = "Empfehlung: Starte mit Malwarebytes, da kein aktueller Malware-Scan erkennbar ist.";
            return;
        }

        if (recommendWindowsTools)
        {
            RecommendationText = "Empfehlung: Starte mit Windows-Bordmitteln (Speicher/Updates).";
        }
    }

    [RelayCommand]
    private void StartWizard()
    {
        StartWizardRequested?.Invoke(this, EventArgs.Empty);
    }
}
