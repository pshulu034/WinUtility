using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WinUtil
{
    public class SystemInfoManager
    {
        public class SystemInfo
        {
            public string CpuModel { get; set; } = string.Empty;
            public int LogicalProcessorCount { get; set; }
            public ulong TotalPhysicalMemoryBytes { get; set; }
            public string MotherboardManufacturer { get; set; } = string.Empty;
            public string MotherboardProduct { get; set; } = string.Empty;
            public string BiosVersion { get; set; } = string.Empty;
            public string OsProductName { get; set; } = string.Empty;
            public string OsBuild { get; set; } = string.Empty;
            public string OsVersionString { get; set; } = string.Empty;
            public string ComputerName { get; set; } = Environment.MachineName;
        }

        public SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo();
            info.CpuModel = GetCpuModel();
            info.LogicalProcessorCount = Environment.ProcessorCount;
            info.TotalPhysicalMemoryBytes = GetTotalPhysicalMemory();
            var mb = GetMotherboardInfo();
            info.MotherboardManufacturer = mb.manufacturer;
            info.MotherboardProduct = mb.product;
            info.BiosVersion = GetBiosVersion();
            var os = GetOsInfo();
            info.OsProductName = os.productName;
            info.OsBuild = os.build;
            info.OsVersionString = os.versionString;
            return info;
        }

        private string GetCpuModel()
        {
            try
            {
                var rm = new RegistryManager();
                var v = rm.GetString(RegistryManager.RootHive.LocalMachine, @"HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString", "");
                return v ?? "";
            }
            catch { return ""; }
        }

        private ulong GetTotalPhysicalMemory()
        {
            var status = new MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            if (GlobalMemoryStatusEx(ref status))
                return status.ullTotalPhys;
            return 0;
        }

        private (string manufacturer, string product) GetMotherboardInfo()
        {
            string manufacturer = "";
            string product = "";
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-CimInstance Win32_BaseBoard | Select-Object Manufacturer, Product | Format-List");
            var text = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                if (key.Equals("Manufacturer", StringComparison.OrdinalIgnoreCase)) manufacturer = val;
                else if (key.Equals("Product", StringComparison.OrdinalIgnoreCase)) product = val;
            }
            return (manufacturer, product);
        }

        private string GetBiosVersion()
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-CimInstance Win32_BIOS | Select-Object SMBIOSBIOSVersion | Format-List");
            var text = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                if (key.Equals("SMBIOSBIOSVersion", StringComparison.OrdinalIgnoreCase)) return val;
            }
            return "";
        }

        private (string productName, string build, string versionString) GetOsInfo()
        {
            string productName = "";
            string build = "";
            try
            {
                var rm = new RegistryManager();
                productName = rm.GetString(RegistryManager.RootHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "") ?? "";
                build = rm.GetString(RegistryManager.RootHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", "") ?? "";
                if (string.IsNullOrWhiteSpace(build))
                    build = rm.GetString(RegistryManager.RootHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "") ?? "";
            }
            catch { }
            string versionString = Environment.OSVersion.VersionString;
            return (productName, build, versionString);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
    }
}
