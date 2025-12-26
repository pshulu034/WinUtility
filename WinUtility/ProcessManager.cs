using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUtility
{
    /// <summary>
    /// Windows 进程管理器
    /// 封装常见的进程操作：查询、启动、结束、信息获取等
    /// </summary>
    public class ProcessManager
    {
        /// <summary>
        /// 进程信息模型（只暴露常用字段，避免直接依赖 System.Diagnostics.Process）
        /// </summary>
        public class ProcessInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? MainWindowTitle { get; set; }
            public string? FileName { get; set; }
            public DateTime? StartTime { get; set; }
            public long WorkingSetBytes { get; set; }
            /// <summary>
            /// CPU 使用率（百分比，0-100，多核时可能大于100，按逻辑CPU总和）
            /// 仅在通过 CPU 统计方法获取时才会被赋值，其它情况下为 0
            /// </summary>
            public double CpuPercent { get; set; }

            public override string ToString()
            {
                var memMb = WorkingSetBytes / 1024d / 1024d;
                return $"{Name} (Id={Id}, CPU={CpuPercent:F1}%, Memory={memMb:F1}MB, Title={MainWindowTitle})";
            }
        }

        /// <summary>
        /// 获取当前所有进程
        /// </summary>
        public IReadOnlyList<ProcessInfo> GetAllProcesses()
        {
            return Process.GetProcesses()
                .Select(ToProcessInfoSafe)
                .Where(p => p != null)
                .Cast<ProcessInfo>()
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToList();
        }

        /// <summary>
        /// 根据进程名获取进程（不含扩展名，如：\"notepad\"）
        /// </summary>
        public IReadOnlyList<ProcessInfo> GetProcessesByName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentException("进程名不能为空", nameof(processName));

            return Process.GetProcessesByName(processName)
                .Select(ToProcessInfoSafe)
                .Where(p => p != null)
                .Cast<ProcessInfo>()
                .OrderBy(p => p.Id)
                .ToList();
        }

        /// <summary>
        /// 根据进程 Id 获取进程
        /// </summary>
        public ProcessInfo? GetProcessById(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                return ToProcessInfoSafe(p);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 启动一个新进程
        /// </summary>
        /// <param name="fileName">可执行文件路径或程序名（如 notepad.exe）</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="createNoWindow">是否不显示窗口</param>
        /// <param name="useShellExecute">是否使用 Shell 启动（影响环境变量、重定向等）</param>
        public ProcessInfo StartProcess(
            string fileName,
            string? arguments = null,
            string? workingDirectory = null,
            bool createNoWindow = false,
            bool useShellExecute = true)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("文件名不能为空", nameof(fileName));

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = useShellExecute,
                CreateNoWindow = createNoWindow,
                WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory)
                    ? Environment.CurrentDirectory
                    : workingDirectory
            };

            var process = Process.Start(startInfo)
                          ?? throw new InvalidOperationException("进程启动失败");

            return ToProcessInfoSafe(process) ?? new ProcessInfo
            {
                Id = process.Id,
                Name = process.ProcessName
            };
        }

        /// <summary>
        /// 根据 Id 结束进程
        /// </summary>
        public bool KillProcessById(int pid, bool force = true)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (force)
                {
                    process.Kill(entireProcessTree: true);
                }
                else
                {
                    // 优雅关闭主窗口（仅对带窗口进程有效）
                    if (!process.CloseMainWindow())
                        process.Kill();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 根据进程名结束所有同名进程
        /// </summary>
        public int KillProcessesByName(string processName, bool force = true)
        {
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentException("进程名不能为空", nameof(processName));

            int count = 0;

            foreach (var p in Process.GetProcessesByName(processName))
            {
                try
                {
                    if (force)
                    {
                        p.Kill(entireProcessTree: true);
                    }
                    else
                    {
                        if (!p.CloseMainWindow())
                            p.Kill();
                    }

                    count++;
                }
                catch
                {
                    // 忽略单个进程的失败，继续处理其它进程
                }
            }

            return count;
        }

        /// <summary>
        /// 构造一个简单的进程信息字符串，便于在控制台/日志中查看
        /// </summary>
        public string FormatProcessList(IEnumerable<ProcessInfo> processes)
        {
            var sb = new StringBuilder();
            foreach (var p in processes)
            {
                sb.AppendLine(p.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 按内存占用从高到低获取前 N 个进程
        /// </summary>
        public IReadOnlyList<ProcessInfo> GetTopByMemory(int topN = 10)
        {
            if (topN <= 0)
                throw new ArgumentOutOfRangeException(nameof(topN));

            return GetAllProcesses()
                .OrderByDescending(p => p.WorkingSetBytes)
                .ThenBy(p => p.Id)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// 按 CPU 使用率从高到低获取前 N 个进程（简单采样法）
        /// sampleMilliseconds 为采样时间，越长越稳定
        /// </summary>
        public async Task<IReadOnlyList<ProcessInfo>> GetTopByCpuAsync(
            int topN = 10,
            int sampleMilliseconds = 1000,
            CancellationToken cancellationToken = default)
        {
            if (topN <= 0)
                throw new ArgumentOutOfRangeException(nameof(topN));
            if (sampleMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleMilliseconds));

            var processes = Process.GetProcesses();

            // 第一次采样 CPU 时间
            var cpuTimes1 = new Dictionary<int, TimeSpan>();
            var startTime = DateTime.UtcNow;
            foreach (var p in processes)
            {
                try
                {
                    cpuTimes1[p.Id] = p.TotalProcessorTime;
                }
                catch
                {
                    // 忽略访问失败的进程
                }
            }

            // 等待采样间隔
            try
            {
                await Task.Delay(sampleMilliseconds, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return Array.Empty<ProcessInfo>();
            }

            var endTime = DateTime.UtcNow;
            var elapsed = endTime - startTime;
            if (elapsed <= TimeSpan.Zero)
                elapsed = TimeSpan.FromMilliseconds(sampleMilliseconds);

            double totalSeconds = elapsed.TotalSeconds;
            int cpuCount = Environment.ProcessorCount;

            var result = new List<ProcessInfo>();

            foreach (var p in processes)
            {
                try
                {
                    if (!cpuTimes1.TryGetValue(p.Id, out var cpu1))
                        continue;

                    var cpu2 = p.TotalProcessorTime;
                    var cpuDelta = (cpu2 - cpu1).TotalSeconds;
                    if (cpuDelta < 0)
                        continue;

                    // CPU 使用率 = CPU时间增量 / 采样时间 / CPU核心数 * 100
                    double cpuPercent = cpuDelta / totalSeconds / cpuCount * 100.0;

                    var info = ToProcessInfoSafe(p);
                    if (info == null)
                        continue;

                    info.CpuPercent = cpuPercent;
                    result.Add(info);
                }
                catch
                {
                    // 忽略采样失败的进程
                }
            }

            // 清理 Process 对象
            foreach (var p in processes)
            {
                try { p.Dispose(); } catch { }
            }

            return result
                .OrderByDescending(p => p.CpuPercent)
                .ThenByDescending(p => p.WorkingSetBytes)
                .ThenBy(p => p.Id)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// 安全地从 Process 转换为 ProcessInfo（处理访问被拒绝等异常）
        /// </summary>
        private static ProcessInfo? ToProcessInfoSafe(Process process)
        {
            try
            {
                string? fileName = null;
                DateTime? startTime = null;

                try { fileName = process.MainModule?.FileName; } catch { }
                try { startTime = process.StartTime; } catch { }

                return new ProcessInfo
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    MainWindowTitle = process.MainWindowTitle,
                    FileName = fileName,
                    StartTime = startTime,
                    WorkingSetBytes = process.WorkingSet64
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
