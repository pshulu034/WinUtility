using System;
using WinUtil;

namespace TWinService
{
    public class TSystemInfoManager
    {
        public static void Test()
        {
            var sim = new SystemInfoManager();
            var info = sim.GetSystemInfo();
            Console.WriteLine($"ComputerName: {info.ComputerName}");
            Console.WriteLine($"CPU: {info.CpuModel}");
            Console.WriteLine($"LogicalProcessors: {info.LogicalProcessorCount}");
            Console.WriteLine($"TotalMemoryBytes: {info.TotalPhysicalMemoryBytes}");
            Console.WriteLine($"Motherboard: {info.MotherboardManufacturer} {info.MotherboardProduct}");
            Console.WriteLine($"BIOS: {info.BiosVersion}");
            Console.WriteLine($"OS: {info.OsProductName}");
            Console.WriteLine($"OSBuild: {info.OsBuild}");
            Console.WriteLine($"OSVersionString: {info.OsVersionString}");
        }
    }
}
