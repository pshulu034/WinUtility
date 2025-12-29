using System;
using System.Linq;
using WinUtil;

namespace TWinService
{
    public class TDiskManager
    {
        public static void Test()
        {
            var dm = new DiskManager();

            var vols = dm.GetVolumes();
            foreach (var v in vols)
            {
                Console.WriteLine($"{v.DriveLetter} {v.Label} {v.Format} {v.DriveType} Total={v.TotalBytes} Free={v.FreeBytes}");
            }

            var disks = dm.GetDisks();
            foreach (var d in disks)
            {
                Console.WriteLine($"Disk {d.Number} {d.FriendlyName} Size={d.SizeBytes} Partitions={d.PartitionCount}");
            }

            var parts = dm.GetPartitions();
            foreach (var p in parts.Take(10))
            {
                Console.WriteLine($"Disk {p.DiskNumber} Part {p.PartitionNumber} Letter={p.DriveLetter} Size={p.SizeBytes} Type={p.Type}");
            }

            var low = dm.ListLowSpace(10.0, 5L * 1024 * 1024 * 1024);
            foreach (var v in low)
            {
                Console.WriteLine($"LOW {v.DriveLetter} Free={v.FreeBytes} Total={v.TotalBytes}");
            }
        }
    }
}
