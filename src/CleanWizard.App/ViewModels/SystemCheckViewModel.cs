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
    private bool _hasLoadedOnce;

    public event EventHandler? StartWizardRequested;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private SystemInfoModel? _systemInfo;

    [ObservableProperty]
    private PerformanceSnapshot? _performanceSnapshot;

    [ObservableProperty]
    private string _systemStatusColor = "#4CAF50";

    [ObservableProperty]
    private string _systemStatusText = "Prüfung läuft";

    [ObservableProperty]
    private string _recommendationText = "";

    [ObservableProperty]
    private bool _isEmergencyMode = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public string WindowsVersionText => SystemInfo?.WindowsVersion ?? "Unbekannt";
    public string CpuText => $"{SystemInfo?.CpuName ?? "Unbekannt"} ({SystemInfo?.CpuCores ?? 0} Kerne)";
    public string RamText => $"{SystemInfo?.RamInGb ?? 0} GB";
    public string DriveTypeText => SystemInfo?.DriveType ?? "Unbekannt";
    public string FreeDiskText => $"{(long)Math.Max(0, (SystemInfo?.FreeDiskSpaceBytes ?? 0) / 1024d / 1024d / 1024d):0} GB frei";
    public string AutostartCountText => $"{SystemInfo?.AutostartCount ?? 0} Einträge";
    public string RunningProcessesText => $"{SystemInfo?.RunningProcessCount ?? 0} Prozesse";
    public string LastWindowsUpdateText => SystemInfo?.LastWindowsUpdate?.ToString("dd.MM.yyyy HH:mm") ?? "Nicht erkannt";
    public bool CanStartWizard => !IsLoading && !HasError && SystemInfo != null;
    public string LastMalwareScanText
    {
        get
        {
            if (SystemInfo?.LastMalwareScan == null)
                return "Nicht erkannt";
            var dateStr = SystemInfo.LastMalwareScan.Value.ToString("dd.MM.yyyy");
            var source = SystemInfo.LastMalwareScanSource;
            return string.IsNullOrEmpty(source) ? dateStr : $"{dateStr} ({source})";
        }
    }

    public SystemCheckViewModel(
        ISystemInfoService systemInfoService,
        IPerformanceAnalyzer performanceAnalyzer)
    {
        _systemInfoService = systemInfoService;
        _performanceAnalyzer = performanceAnalyzer;
    }

    public async Task EnsureLoadedAsync(bool forceRefresh = false)
    {
        if (IsLoading)
            return;

        if (!forceRefresh && _hasLoadedOnce && SystemInfo != null && !HasError)
            return;

        await LoadSystemInfoCoreAsync();
    }

    [RelayCommand]
    private async Task LoadSystemInfoAsync()
    {
        await LoadSystemInfoCoreAsync();
    }

    private async Task LoadSystemInfoCoreAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;
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
            OnPropertyChanged(nameof(CanStartWizard));
        }
        catch (Exception ex)
        {
            HasError = true;
            SystemStatusColor = "#F44336";
            SystemStatusText = "Fehler";
            RecommendationText = "Die Startprüfung konnte nicht vollständig geladen werden.";
            ErrorMessage = $"Die Prüfung ist fehlgeschlagen: {ex.Message}";
            OnPropertyChanged(nameof(CanStartWizard));
        }
        finally
        {
            _hasLoadedOnce = true;
            IsLoading = false;
            OnPropertyChanged(nameof(CanStartWizard));
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
            SystemStatusText = "Stabil";
            RecommendationText = "Der PC wirkt unauffällig. Du kannst den Assistenten normal starten.";
        }
        else if (issues == 1)
        {
            SystemStatusColor = "#FF9800";
            SystemStatusText = "Prüfen";
            RecommendationText = "Ein Bereich fällt auf und sollte zuerst geprüft werden.";
        }
        else
        {
            SystemStatusColor = "#F44336";
            SystemStatusText = "Auffällig";
            RecommendationText = "Mehrere Bereiche sollten nacheinander geprüft werden.";
        }

        if (IsEmergencyMode)
        {
            RecommendationText = "Der PC wirkt stark belastet. Starte zuerst mit den wichtigsten Schritten.";
            return;
        }

        if (recommendAutoruns)
        {
            RecommendationText = "Beginne mit Autoruns. Es wurden viele Startprogramme erkannt.";
            return;
        }

        if (recommendMalware)
        {
            RecommendationText = "Beginne mit Malwarebytes. Ein aktueller Malware-Scan wurde nicht erkannt.";
            return;
        }

        if (recommendWindowsTools)
        {
            RecommendationText = "Beginne mit den Windows-Werkzeugen. Speicher oder Updates brauchen vermutlich Aufmerksamkeit.";
        }
    }

    [RelayCommand]
    private void StartWizard()
    {
        if (!CanStartWizard)
            return;

        StartWizardRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task RetrySystemCheckAsync()
    {
        await EnsureLoadedAsync(forceRefresh: true);
    }
}
