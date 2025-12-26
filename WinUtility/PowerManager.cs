using System;

namespace WinUtility
{
    public class PowerManager
    {
        private readonly CommandExecutor _exec = new CommandExecutor();

        public bool Shutdown(int seconds = 0, bool force = false)
        {
            string args = "/s";
            if (seconds > 0) args += $" /t {seconds}";
            if (force) args += " /f";
            var res = _exec.Execute("shutdown", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool Restart(int seconds = 0, bool force = false)
        {
            string args = "/r";
            if (seconds > 0) args += $" /t {seconds}";
            if (force) args += " /f";
            var res = _exec.Execute("shutdown", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool Logoff()
        {
            var res = _exec.Execute("shutdown", "/l", timeout: 30000);
            return res.IsSuccess;
        }

        public bool Hibernate(bool ensureEnabled = true)
        {
            if (ensureEnabled) EnableHibernate();
            var res = _exec.Execute("shutdown", "/h", timeout: 30000);
            return res.IsSuccess;
        }

        public bool EnableHibernate()
        {
            var res = _exec.Execute("powercfg", "/hibernate on", timeout: 30000);
            return res.IsSuccess;
        }

        public bool DisableHibernate()
        {
            var res = _exec.Execute("powercfg", "/hibernate off", timeout: 30000);
            return res.IsSuccess;
        }

        public bool LockScreen()
        {
            var res = _exec.Execute("rundll32.exe", "user32.dll,LockWorkStation", timeout: 15000);
            return res.IsSuccess;
        }

        public bool ScheduleShutdown(DateTime when, bool restart = false, bool force = false)
        {
            int seconds = (int)Math.Max(0, (when - DateTime.Now).TotalSeconds);
            if (restart) return Restart(seconds, force);
            return Shutdown(seconds, force);
        }

        public bool ScheduleShutdownAfterSeconds(int seconds, bool restart = false, bool force = false)
        {
            seconds = Math.Max(0, seconds);
            if (restart) return Restart(seconds, force);
            return Shutdown(seconds, force);
        }

        public bool CancelScheduled()
        {
            var res = _exec.Execute("shutdown", "/a", timeout: 15000);
            return res.IsSuccess;
        }
    }
}
