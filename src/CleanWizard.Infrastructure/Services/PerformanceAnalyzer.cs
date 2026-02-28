using System.Diagnostics;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;
using Microsoft.Win32;

namespace CleanWizard.Infrastructure.Services;

public class PerformanceAnalyzer : IPerformanceAnalyzer
{
    public async Task<PerformanceSnapshot> CaptureAsync()
    {
        return await Task.Run(() =>
        {
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.Now,
                AutostartCount = CountAutostart(),
                FreeDiskSpaceBytes = GetFreeDiskSpace(),
                UsedRamBytes = GetUsedRam(),
                CpuUsagePercent = 0
            };
            return snapshot;
        });
    }

    private static int CountAutostart()
    {
        int count = 0;
        try
        {
            using var hkcu = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run");
            count += hkcu?.ValueCount ?? 0;
            using var hklm = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run");
            count += hklm?.ValueCount ?? 0;
        }
        catch { }
        return count;
    }

    private static long GetFreeDiskSpace()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
        return drive?.AvailableFreeSpace ?? 0;
    }

    private static long GetUsedRam()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64;
    }
}
