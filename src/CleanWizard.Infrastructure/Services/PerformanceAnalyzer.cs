using System.Diagnostics;
using System.Runtime.InteropServices;
using CleanWizard.Core.Interfaces;
using CleanWizard.Core.Models;
using Microsoft.Win32;

namespace CleanWizard.Infrastructure.Services;

public class PerformanceAnalyzer : IPerformanceAnalyzer
{
    private const int CpuSamplingDelayMs = 500; // Two samples 500 ms apart give reliable CPU load

    public async Task<PerformanceSnapshot> CaptureAsync()
    {
        var snapshot = await Task.Run(() => new PerformanceSnapshot
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
        snapshot.CpuUsagePercent = await MeasureCpuUsageAsync();
        return snapshot;
    }

    private static async Task<double> MeasureCpuUsageAsync()
    {
        try
        {
            if (!GetSystemTimes(out var idle1, out var kernel1, out var user1))
                return 0;
            await Task.Delay(CpuSamplingDelayMs);
            if (!GetSystemTimes(out var idle2, out var kernel2, out var user2))
                return 0;

            var idleDiff = FileTimeToLong(idle2) - FileTimeToLong(idle1);
            var kernelDiff = FileTimeToLong(kernel2) - FileTimeToLong(kernel1);
            var userDiff = FileTimeToLong(user2) - FileTimeToLong(user1);
            var total = kernelDiff + userDiff;

            if (total <= 0) return 0;
            return Math.Round((1.0 - (double)idleDiff / total) * 100.0, 1);
        }
        catch (DllNotFoundException) { return 0; }
        catch (EntryPointNotFoundException) { return 0; }
        catch (InvalidOperationException) { return 0; }
    }

    private static long FileTimeToLong(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        => ((long)ft.dwHighDateTime << 32) | (uint)ft.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(
        out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

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
