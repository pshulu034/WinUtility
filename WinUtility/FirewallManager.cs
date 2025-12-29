using System;
using System.Collections.Generic;
using WinUtil;

namespace WinUtil
{
    public class FirewallManager
    {
        public enum Direction { In, Out }
        public enum RuleAction { Allow, Block }
        public enum Protocol { Any, TCP, UDP }

        public bool EnableAllProfiles()
        {
            return RunNetsh("advfirewall set allprofiles state on");
        }

        public bool DisableAllProfiles()
        {
            return RunNetsh("advfirewall set allprofiles state off");
        }

        public bool EnableProfile(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile)) return false;
            return RunNetsh($"advfirewall set {profile} state on");
        }

        public bool DisableProfile(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile)) return false;
            return RunNetsh($"advfirewall set {profile} state off");
        }

        public string ShowStatus()
        {
            return RunNetshOutput("advfirewall show allprofiles");
        }

        public string ShowRules(string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RunNetshOutput("advfirewall firewall show rule name=all");
            return RunNetshOutput($"advfirewall firewall show rule name=\"{name}\"");
        }

        public bool AddPortRule(string name, int port, Protocol protocol, Direction direction, RuleAction action, string? profile = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (port <= 0 || port > 65535) return false;
            var proto = protocol == Protocol.Any ? "ANY" : protocol.ToString();
            var dir = direction == Direction.In ? "in" : "out";
            var act = action == RuleAction.Allow ? "allow" : "block";
            var profilePart = string.IsNullOrWhiteSpace(profile) ? "" : $" profile={profile}";
            return RunNetsh($"advfirewall firewall add rule name=\"{name}\" dir={dir} action={act} protocol={proto} localport={port}{profilePart}");
        }

        public bool AddProgramRule(string name, string programPath, Direction direction, RuleAction action, string? profile = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (string.IsNullOrWhiteSpace(programPath)) return false;
            var dir = direction == Direction.In ? "in" : "out";
            var act = action == RuleAction.Allow ? "allow" : "block";
            var profilePart = string.IsNullOrWhiteSpace(profile) ? "" : $" profile={profile}";
            return RunNetsh($"advfirewall firewall add rule name=\"{name}\" dir={dir} action={act} program=\"{programPath}\" enable=yes{profilePart}");
        }

        public bool DeleteRule(string name, Direction? direction = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var dirPart = direction == null ? "" : $" dir={(direction == Direction.In ? "in" : "out")}";
            return RunNetsh($"advfirewall firewall delete rule name=\"{name}\"{dirPart}");
        }

        public bool OpenPort(string name, int port, Protocol protocol = Protocol.TCP, string? profile = null)
        {
            return AddPortRule(name, port, protocol, Direction.In, RuleAction.Allow, profile);
        }

        public bool ClosePort(string name, int port, Direction? direction = null)
        {
            return DeleteRule(name, direction);
        }

        private static bool RunNetsh(string args)
        {
            var executor = new CommandExecutor();
            var result = executor.Execute("netsh", args, timeout: 30000);
            return result.IsSuccess;
        }

        private static string RunNetshOutput(string args)
        {
            var executor = new CommandExecutor();
            var result = executor.Execute("netsh", args, timeout: 30000);
            return string.IsNullOrWhiteSpace(result.Output) ? result.Error : result.Output;
        }
    }
}
