using System.Diagnostics;
using System.Runtime.InteropServices;
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
                CpuUsagePercent = CaptureCpuUsagePercent()
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

    private static double CaptureCpuUsagePercent()
    {
        try
        {
            if (!GetSystemTimes(out var idleStart, out var kernelStart, out var userStart))
                return -1;

            Thread.Sleep(300);

            if (!GetSystemTimes(out var idleEnd, out var kernelEnd, out var userEnd))
                return -1;

            var idle = ToUInt64(idleEnd) - ToUInt64(idleStart);
            var kernel = ToUInt64(kernelEnd) - ToUInt64(kernelStart);
            var user = ToUInt64(userEnd) - ToUInt64(userStart);

            var total = kernel + user;
            if (total == 0)
                return -1;

            var busy = total - idle;
            var value = busy * 100d / total;
            return Math.Clamp(value, 0, 100);
        }
        catch
        {
            return -1;
        }
    }

    private static ulong ToUInt64(FILETIME time)
        => ((ulong)time.dwHighDateTime << 32) | time.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(
        out FILETIME lpIdleTime,
        out FILETIME lpKernelTime,
        out FILETIME lpUserTime);

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }
}
