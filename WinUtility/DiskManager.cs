using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinUtility
{
    public class DiskManager
    {
        public class VolumeInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? DriveLetter { get; set; }
            public long TotalBytes { get; set; }
            public long FreeBytes { get; set; }
            public string? Format { get; set; }
            public string? Label { get; set; }
            public DriveType DriveType { get; set; }
        }

        public class PartitionInfo
        {
            public int DiskNumber { get; set; }
            public int PartitionNumber { get; set; }
            public string? DriveLetter { get; set; }
            public long SizeBytes { get; set; }
            public string? Type { get; set; }
        }

        public class DiskInfo
        {
            public int Number { get; set; }
            public string? FriendlyName { get; set; }
            public long SizeBytes { get; set; }
            public int PartitionCount { get; set; }
        }

        public IReadOnlyList<VolumeInfo> GetVolumes()
        {
            var list = new List<VolumeInfo>();
            foreach (var di in DriveInfo.GetDrives())
            {
                try
                {
                    var v = new VolumeInfo
                    {
                        Name = di.Name,
                        DriveLetter = di.Name.TrimEnd('\\'),
                        TotalBytes = di.IsReady ? di.TotalSize : 0,
                        FreeBytes = di.IsReady ? di.AvailableFreeSpace : 0,
                        Format = di.IsReady ? di.DriveFormat : null,
                        Label = di.IsReady ? di.VolumeLabel : null,
                        DriveType = di.DriveType
                    };
                    list.Add(v);
                }
                catch
                {
                }
            }
            return list.OrderBy(v => v.DriveLetter).ToList();
        }

        public IReadOnlyList<PartitionInfo> GetPartitions()
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-Partition | Select-Object DiskNumber, PartitionNumber, DriveLetter, Size, Type | Format-List");
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            var items = ParseListObjects(txt);
            var list = new List<PartitionInfo>();
            foreach (var dict in items)
            {
                var p = new PartitionInfo
                {
                    DiskNumber = TryInt(dict, "DiskNumber"),
                    PartitionNumber = TryInt(dict, "PartitionNumber"),
                    DriveLetter = TryString(dict, "DriveLetter"),
                    SizeBytes = TryLong(dict, "Size"),
                    Type = TryString(dict, "Type")
                };
                list.Add(p);
            }
            return list.OrderBy(p => p.DiskNumber).ThenBy(p => p.PartitionNumber).ToList();
        }

        public IReadOnlyList<DiskInfo> GetDisks()
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-Disk | Select-Object Number, FriendlyName, Size, PartitionCount | Format-List");
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            var items = ParseListObjects(txt);
            var list = new List<DiskInfo>();
            foreach (var dict in items)
            {
                var d = new DiskInfo
                {
                    Number = TryInt(dict, "Number"),
                    FriendlyName = TryString(dict, "FriendlyName"),
                    SizeBytes = TryLong(dict, "Size"),
                    PartitionCount = TryInt(dict, "PartitionCount")
                };
                list.Add(d);
            }
            return list.OrderBy(d => d.Number).ToList();
        }

        public IReadOnlyList<VolumeInfo> ListLowSpace(double thresholdPercent = 10.0, long minFreeBytes = 5L * 1024 * 1024 * 1024)
        {
            var vols = GetVolumes();
            var low = new List<VolumeInfo>();
            foreach (var v in vols)
            {
                if (v.TotalBytes <= 0) continue;
                double percent = v.FreeBytes * 100.0 / v.TotalBytes;
                if (percent < thresholdPercent || v.FreeBytes < minFreeBytes)
                    low.Add(v);
            }
            return low;
        }

        private static List<Dictionary<string, string>> ParseListObjects(string text)
        {
            var result = new List<Dictionary<string, string>>();
            var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (current.Count > 0)
                    {
                        result.Add(current);
                        current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    continue;
                }
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                current[key] = val;
            }
            if (current.Count > 0) result.Add(current);
            return result;
        }

        private static int TryInt(Dictionary<string, string> d, string k)
        {
            if (d.TryGetValue(k, out var s) && int.TryParse(s, out var v)) return v;
            return 0;
        }
        private static long TryLong(Dictionary<string, string> d, string k)
        {
            if (d.TryGetValue(k, out var s) && long.TryParse(s, out var v)) return v;
            return 0;
        }
        private static string? TryString(Dictionary<string, string> d, string k)
        {
            if (d.TryGetValue(k, out var s)) return s;
            return null;
        }
    }
}
