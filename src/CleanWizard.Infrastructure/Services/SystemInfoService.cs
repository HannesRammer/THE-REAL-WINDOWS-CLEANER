using System.Diagnostics;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;
using Microsoft.Win32;

namespace CleanWizard.Infrastructure.Services;

public class SystemInfoService : ISystemInfoService
{
    public async Task<SystemInfoModel> CollectAsync()
    {
        return await Task.Run(() =>
        {
            var model = new SystemInfoModel();

            // Windows Version
            model.WindowsVersion = Environment.OSVersion.VersionString;
            model.BuildNumber = Environment.OSVersion.Version.Build.ToString();

            // CPU & RAM via Environment
            model.CpuCores = Environment.ProcessorCount;
            model.CpuName = GetCpuName();
            model.RamInGb = (int)(GetTotalRam() / (1024L * 1024 * 1024));

            // Disk
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
            if (drive != null)
            {
                model.FreeDiskSpaceBytes = drive.AvailableFreeSpace;
                model.DriveType = GetDriveType(drive.Name);
            }

            // Autostart count from registry
            model.AutostartCount = CountAutostart();

            // Running processes
            model.RunningProcessCount = Process.GetProcesses().Length;

            return model;
        });
    }

    private static string GetCpuName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("ProcessorNameString") as string ?? "Unbekannt";
        }
        catch { return "Unbekannt"; }
    }

    private static long GetTotalRam()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\RESOURCEMAP\System Resources\Physical Memory");
            // Fallback to GC
        }
        catch { }
        return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    }

    private static string GetDriveType(string driveName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\disk\Enum");
            if (key != null)
            {
                var count = (int)(key.GetValue("Count") ?? 0);
                for (int i = 0; i < count; i++)
                {
                    var val = key.GetValue(i.ToString()) as string ?? "";
                    if (val.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
                        val.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
                        val.Contains("Solid", StringComparison.OrdinalIgnoreCase))
                        return "SSD";
                }
            }
        }
        catch { }
        return "HDD";
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
}
