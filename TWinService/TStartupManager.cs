using System;
using System.Diagnostics;
using WinUtility;

namespace ConsoleApp
{
    public class TStartupManager
    {
        public static void Test()
        {
            var sm = new StartupManager();
            string appName = "TWinServiceDemo";
            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? string.Empty;
            if (string.IsNullOrWhiteSpace(exe))
                return;

            sm.EnableForCurrentUser(appName, exe, "--demo");
            bool enabled = sm.IsEnabledForCurrentUser(appName);
            Console.WriteLine($"Enabled: {enabled}");

            var items = sm.ListCurrentUser();
            foreach (var kv in items)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }

            sm.DisableForCurrentUser(appName);
            bool after = sm.IsEnabledForCurrentUser(appName);
            Console.WriteLine($"EnabledAfterDisable: {after}");
        }
    }
}
