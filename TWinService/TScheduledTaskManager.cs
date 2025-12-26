using System;
using System.Diagnostics;
using WinUtility;

namespace TWinService
{
    public class TScheduledTaskManager
    {
        public static void Test()
        {
            var stm = new ScheduledTaskManager();
            string name = "Demo-Once-Notepad";
            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "";
            if (string.IsNullOrWhiteSpace(exe)) return;

            var now = DateTime.Now.AddMinutes(1);
            string st = now.ToString("HH:mm");
            string sd = now.ToString("yyyy/MM/dd");

            var created = stm.CreateOnce(name, "notepad.exe", null, st, sd, highest: true, force: true);
            Console.WriteLine($"CreateOnce: {created}");

            var info = stm.Query(name);
            Console.WriteLine(info);

            var run = stm.Run(name);
            Console.WriteLine($"Run: {run}");

            var end = stm.End(name);
            Console.WriteLine($"End: {end}");

            var del = stm.Delete(name);
            Console.WriteLine($"Delete: {del}");
        }
    }
}
