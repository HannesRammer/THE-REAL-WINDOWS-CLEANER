using System.Diagnostics;
using System.Runtime.InteropServices;
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
            if (model.RamInGb < 1)
                model.RamInGb = 1;

            // Disk
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
            if (drive != null)
            {
                model.FreeDiskSpaceBytes = Math.Max(0, drive.AvailableFreeSpace);
                model.DriveType = GetDriveType(drive.Name);
            }

            // Autostart count from registry
            model.AutostartCount = CountAutostart();

            // Running processes
            model.RunningProcessCount = Process.GetProcesses().Length;
            model.LastWindowsUpdate = GetLastWindowsUpdate();
            model.LastMalwareScan = GetLastMalwareScan();

            // Last malware scan (Defender EventLog → Defender Registry → Malwarebytes)
            var (scanDate, scanSource) = new MalwareScanDetector().Detect();
            model.LastMalwareScan = scanDate;
            model.LastMalwareScanSource = scanSource;

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
            var status = new MemoryStatusEx();
            if (GlobalMemoryStatusEx(status))
                return (long)status.TotalPhysical;
        }
        catch
        {
        }

        var fallback = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        return fallback > 0 ? fallback : 4L * 1024 * 1024 * 1024;
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
        return "Unbekannt";
    }

    private static int CountAutostart()
    {
        int count = 0;
        try
        {
            using var hkcu = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run");
            count += hkcu?.ValueCount ?? 0;
            using var hkcuRunOnce = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
            count += hkcuRunOnce?.ValueCount ?? 0;

            using var hklm = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run");
            count += hklm?.ValueCount ?? 0;
            using var hklmRunOnce = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
            count += hklmRunOnce?.ValueCount ?? 0;

            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
                count += Directory.GetFiles(startupFolder).Length;
        }
        catch { }
        return count;
    }

    private static DateTime? GetLastWindowsUpdate()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Install");
            var raw = key?.GetValue("LastSuccessTime") as string;
            if (DateTime.TryParse(raw, out var parsed))
                return parsed;
        }
        catch { }

        return null;
    }

    private static DateTime? GetLastMalwareScan()
    {
        try
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var baseDir = Path.Combine(programData, "Malwarebytes");
            if (!Directory.Exists(baseDir))
                return null;

            var latest = Directory
                .GetFiles(baseDir, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                    f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".log", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();

            return latest?.LastWriteTime;
        }
        catch
        {
            return null;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public uint Length = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailPhysical;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);
}
