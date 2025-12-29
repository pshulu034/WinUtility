using System;
using System.Threading;
using WinUtil;

namespace TWinService
{
    public class TPowerManager
    {
        public static void Test()
        {
            var pm = new PowerManager();

            var when = DateTime.Now.AddSeconds(60);
            var scheduled = pm.ScheduleShutdown(when, restart: false, force: false);
            Console.WriteLine($"Scheduled shutdown in 60s: {scheduled}");

            Thread.Sleep(5000);
            var canceled = pm.CancelScheduled();
            Console.WriteLine($"Cancel scheduled: {canceled}");

            var hibEnable = pm.EnableHibernate();
            Console.WriteLine($"EnableHibernate: {hibEnable}");

            // pm.LockScreen(); // 如需验证锁屏，取消注释此行
        }
    }
}
