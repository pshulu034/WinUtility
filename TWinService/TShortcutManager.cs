using System;
using System.Diagnostics;
using System.IO;
using WinUtil;

namespace TWinService
{
    public class TShortcutManager
    {
        public static void Test()
        {
            var sm = new ShortcutManager();
            var exe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "";
            if (string.IsNullOrWhiteSpace(exe)) return;
            var name = "DemoShortcut";

            var desktopPath = sm.CreateDesktopShortcut(name, exe, "--demo");
            Console.WriteLine(desktopPath);

            var startMenuPath = sm.CreateStartMenuShortcut(name, exe, allUsers: false);
            Console.WriteLine(startMenuPath);

            sm.PinToTaskbar(desktopPath);

            sm.UnpinFromTaskbar(desktopPath);
            sm.DeleteDesktopShortcut(name);
            sm.DeleteStartMenuShortcut(name);
        }
    }
}
