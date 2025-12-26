using System;
using System.IO;

namespace WinUtility
{
    public class ScheduledTaskManager
    {
        private readonly CommandExecutor _exec = new CommandExecutor();

        public bool CreateDaily(string name, string executablePath, string? arguments, string startTime, string? startDate = null, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true)
        {
            string tr = ComposeCommand(executablePath, arguments);
            string args = $"/Create /SC DAILY /TN \"{name}\" /TR \"{tr}\" /ST {startTime}";
            if (!string.IsNullOrWhiteSpace(startDate)) args += $" /SD {startDate}";
            if (highest) args += " /RL HIGHEST";
            if (!string.IsNullOrWhiteSpace(runUser)) args += $" /RU \"{runUser}\"";
            if (!string.IsNullOrWhiteSpace(runPassword)) args += $" /RP \"{runPassword}\"";
            if (force) args += " /F";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool CreateWeekly(string name, string executablePath, string? arguments, string startTime, string days, string? startDate = null, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true)
        {
            string tr = ComposeCommand(executablePath, arguments);
            string args = $"/Create /SC WEEKLY /D {days} /TN \"{name}\" /TR \"{tr}\" /ST {startTime}";
            if (!string.IsNullOrWhiteSpace(startDate)) args += $" /SD {startDate}";
            if (highest) args += " /RL HIGHEST";
            if (!string.IsNullOrWhiteSpace(runUser)) args += $" /RU \"{runUser}\"";
            if (!string.IsNullOrWhiteSpace(runPassword)) args += $" /RP \"{runPassword}\"";
            if (force) args += " /F";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool CreateOnce(string name, string executablePath, string? arguments, string startTime, string startDate, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true)
        {
            string tr = ComposeCommand(executablePath, arguments);
            string args = $"/Create /SC ONCE /TN \"{name}\" /TR \"{tr}\" /ST {startTime} /SD {startDate}";
            if (highest) args += " /RL HIGHEST";
            if (!string.IsNullOrWhiteSpace(runUser)) args += $" /RU \"{runUser}\"";
            if (!string.IsNullOrWhiteSpace(runPassword)) args += $" /RP \"{runPassword}\"";
            if (force) args += " /F";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool CreateAtLogon(string name, string executablePath, string? arguments, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true)
        {
            string tr = ComposeCommand(executablePath, arguments);
            string args = $"/Create /SC ONLOGON /TN \"{name}\" /TR \"{tr}\"";
            if (highest) args += " /RL HIGHEST";
            if (!string.IsNullOrWhiteSpace(runUser)) args += $" /RU \"{runUser}\"";
            if (!string.IsNullOrWhiteSpace(runPassword)) args += $" /RP \"{runPassword}\"";
            if (force) args += " /F";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool ModifyCommand(string name, string executablePath, string? arguments)
        {
            string tr = ComposeCommand(executablePath, arguments);
            string args = $"/Change /TN \"{name}\" /TR \"{tr}\"";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool ModifyStartTime(string name, string startTime)
        {
            string args = $"/Change /TN \"{name}\" /ST {startTime}";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return res.IsSuccess;
        }

        public bool Enable(string name)
        {
            var res = _exec.Execute("schtasks", $"/Change /TN \"{name}\" /ENABLE", timeout: 30000);
            return res.IsSuccess;
        }

        public bool Disable(string name)
        {
            var res = _exec.Execute("schtasks", $"/Change /TN \"{name}\" /DISABLE", timeout: 30000);
            return res.IsSuccess;
        }

        public bool Delete(string name)
        {
            var res = _exec.Execute("schtasks", $"/Delete /TN \"{name}\" /F", timeout: 30000);
            return res.IsSuccess;
        }

        public bool Run(string name)
        {
            var res = _exec.Execute("schtasks", $"/Run /TN \"{name}\"", timeout: 30000);
            return res.IsSuccess;
        }

        public bool End(string name)
        {
            var res = _exec.Execute("schtasks", $"/End /TN \"{name}\"", timeout: 30000);
            return res.IsSuccess;
        }

        public string Query(string? name = null, bool verbose = true)
        {
            string args = "/Query";
            if (!string.IsNullOrWhiteSpace(name)) args += $" /TN \"{name}\"";
            if (verbose) args += " /V /FO LIST";
            var res = _exec.Execute("schtasks", args, timeout: 30000);
            return string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
        }

        private static string ComposeCommand(string executablePath, string? arguments)
        {
            string exe = executablePath;
            if (!Path.IsPathRooted(exe)) exe = Path.GetFullPath(exe);
            string exeQuoted = "\"" + exe + "\"";
            if (string.IsNullOrWhiteSpace(arguments)) return exeQuoted;
            return exeQuoted + " " + arguments;
        }
    }
}
