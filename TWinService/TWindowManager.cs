using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WinUtility;

namespace TWinService
{
    public class TWindowManager
    {
        public static void Test()
        {
            var wm = new WindowManager();
            var all = wm.EnumerateWindows();
            foreach (var w in all.Take(5))
            {
                Console.WriteLine($"{w.Handle} | {w.Title} | {w.ClassName} | PID={w.ProcessId} | ({w.X},{w.Y},{w.Width},{w.Height}) | Visible={w.Visible} TopMost={w.TopMost}");
            }

            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                UseShellExecute = true
            });
            Thread.Sleep(800);

            var win = wm.EnumerateWindows().FirstOrDefault(w => w.ProcessId == (proc?.Id ?? -1));
            if (win == null) return;

            wm.Maximize(win.Handle);
            Thread.Sleep(400);
            wm.SetTopMost(win.Handle, true);
            Thread.Sleep(400);
            wm.MoveResize(win.Handle, 100, 100, 800, 600);
            Thread.Sleep(400);
            wm.Minimize(win.Handle);
            Thread.Sleep(400);
            wm.Restore(win.Handle);
            Thread.Sleep(400);
            wm.SetTopMost(win.Handle, false);
            Thread.Sleep(400);

            try
            {
                proc?.CloseMainWindow();
            }
            catch { }
        }
    }
}
