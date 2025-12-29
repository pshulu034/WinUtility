using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtil;

namespace ConsoleApp
{
    public class TProcessManager
    {
        public async static void Test()
        {
            var pm = new ProcessManager();

            // 1. 列出所有进程
            var all = pm.GetAllProcesses();
            Console.WriteLine(pm.FormatProcessList(all));

            // 2. 按名称查找进程（比如记事本）
            var notepads = pm.GetProcessesByName("notepad");
            Console.WriteLine($"Notepad 进程数: {notepads.Count}");

            // 3. 启动一个记事本
            var pInfo = pm.StartProcess("notepad.exe");

            // 4. 通过 Id 结束进程
            pm.KillProcessById(pInfo.Id);

            // 5. 一次性结束所有 notepad
            int killed = pm.KillProcessesByName("notepad");
            Console.WriteLine($"已结束 {killed} 个 notepad 进程");


            Console.WriteLine("示例6: 显示进程 CPU / 内存占用 Top 10\n");

            // 按内存排序
            Console.WriteLine("按内存占用排序（前10个进程）：");
            var topMem = pm.GetTopByMemory(10);
            int index = 1;
            foreach (var p in topMem)
            {
                var memMb = p.WorkingSetBytes / 1024d / 1024d;
                Console.WriteLine($"{index,2}. Id={p.Id,-6} Name={p.Name,-25} Memory={memMb,8:F1} MB  Title={p.MainWindowTitle}");
                index++;
            }

            Console.WriteLine();

            // 按 CPU 排序（采样 1 秒）
            Console.WriteLine("按 CPU 使用率排序（前10个进程，采样 1 秒）：");
            var topCpu = await pm.GetTopByCpuAsync(10, 1000);
            index = 1;
            foreach (var p in topCpu)
            {
                var memMb = p.WorkingSetBytes / 1024d / 1024d;
                Console.WriteLine($"{index,2}. Id={p.Id,-6} Name={p.Name,-25} CPU={p.CpuPercent,6:F1}%  Memory={memMb,8:F1} MB  Title={p.MainWindowTitle}");
                index++;
            }
        }

    }
}
