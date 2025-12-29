using System;
using System.Collections.Generic;
using System.Linq;

namespace WinUtil
{
    public class UserManager
    {
        public class LocalUserInfo
        {
            public string Name { get; set; } = string.Empty;
            public bool Enabled { get; set; }
            public string? Description { get; set; }
        }

        public class LocalGroupInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public IReadOnlyList<string> Members { get; set; } = Array.Empty<string>();
        }

        public bool CreateUser(string name, string password, bool active = true, bool passwordNeverExpires = false, string? comment = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) return false;
            var exec = new CommandExecutor();
            string args = $"user \"{name}\" \"{password}\" /add";
            var res = exec.Execute("net", args, timeout: 30000);
            if (!res.IsSuccess) return false;
            var resActive = exec.Execute("net", $"user \"{name}\" /active:{(active ? "yes" : "no")}", timeout: 30000);
            var ok = resActive.IsSuccess;
            if (passwordNeverExpires)
            {
                var ps = "Set-LocalUser -Name \"" + name.Replace("\"", "\"\"") + "\" -PasswordNeverExpires $true";
                var r = exec.ExecutePowerShell(ps, timeout: 15000);
                ok = ok && r.IsSuccess;
            }
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var r2 = exec.Execute("net", $"user \"{name}\" /comment:\"{comment}\"", timeout: 30000);
                ok = ok && r2.IsSuccess;
            }
            return ok;
        }

        public bool DeleteUser(string name)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"user \"{name}\" /delete", timeout: 30000);
            return res.IsSuccess;
        }

        public bool SetPassword(string name, string newPassword)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"user \"{name}\" \"{newPassword}\"", timeout: 30000);
            return res.IsSuccess;
        }

        public bool EnableUser(string name)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"user \"{name}\" /active:yes", timeout: 30000);
            return res.IsSuccess;
        }

        public bool DisableUser(string name)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"user \"{name}\" /active:no", timeout: 30000);
            return res.IsSuccess;
        }

        public bool UserExists(string name)
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-LocalUser -Name \"" + name.Replace("\"", "\"\"") + "\" | Select-Object Name | Format-List", timeout: 15000);
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            return !string.IsNullOrWhiteSpace(txt) && txt.Contains("Name", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LocalUserInfo> GetUsers()
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-LocalUser | Select-Object Name, Enabled, Description | Format-List", timeout: 20000);
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            var items = ParseListObjects(txt);
            var list = new List<LocalUserInfo>();
            foreach (var d in items)
            {
                var u = new LocalUserInfo
                {
                    Name = TryString(d, "Name") ?? "",
                    Enabled = TryBool(d, "Enabled"),
                    Description = TryString(d, "Description")
                };
                if (!string.IsNullOrWhiteSpace(u.Name))
                    list.Add(u);
            }
            return list.OrderBy(u => u.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public bool CreateGroup(string groupName, string? comment = null)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"localgroup \"{groupName}\" /add", timeout: 30000);
            var ok = res.IsSuccess;
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var r2 = exec.Execute("net", $"localgroup \"{groupName}\" /comment:\"{comment}\"", timeout: 30000);
                ok = ok && r2.IsSuccess;
            }
            return ok;
        }

        public bool DeleteGroup(string groupName)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"localgroup \"{groupName}\" /delete", timeout: 30000);
            return res.IsSuccess;
        }

        public bool GroupExists(string groupName)
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-LocalGroup -Name \"" + groupName.Replace("\"", "\"\"") + "\" | Select-Object Name | Format-List", timeout: 15000);
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            return !string.IsNullOrWhiteSpace(txt) && txt.Contains("Name", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LocalGroupInfo> GetGroups(bool withMembers = true)
        {
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell("Get-LocalGroup | Select-Object Name, Description | Format-List", timeout: 20000);
            var txt = string.IsNullOrWhiteSpace(res.Output) ? res.Error : res.Output;
            var items = ParseListObjects(txt);
            var list = new List<LocalGroupInfo>();
            foreach (var d in items)
            {
                var name = TryString(d, "Name") ?? "";
                if (string.IsNullOrWhiteSpace(name)) continue;
                var g = new LocalGroupInfo
                {
                    Name = name,
                    Description = TryString(d, "Description")
                };
                if (withMembers)
                {
                    var mres = exec.ExecutePowerShell("Get-LocalGroupMember -Group \"" + name.Replace("\"", "\"\"") + "\" | Select-Object Name | Format-List", timeout: 20000);
                    var mtxt = string.IsNullOrWhiteSpace(mres.Output) ? mres.Error : mres.Output;
                    var members = ParseListObjects(mtxt).Select(dict => TryString(dict, "Name") ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    g.Members = members;
                }
                list.Add(g);
            }
            return list.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public bool AddUserToGroup(string groupName, string userName)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"localgroup \"{groupName}\" \"{userName}\" /add", timeout: 30000);
            return res.IsSuccess;
        }

        public bool RemoveUserFromGroup(string groupName, string userName)
        {
            var exec = new CommandExecutor();
            var res = exec.Execute("net", $"localgroup \"{groupName}\" \"{userName}\" /delete", timeout: 30000);
            return res.IsSuccess;
        }

        private static List<Dictionary<string, string>> ParseListObjects(string text)
        {
            var result = new List<Dictionary<string, string>>();
            var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (current.Count > 0)
                    {
                        result.Add(current);
                        current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    continue;
                }
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                current[key] = val;
            }
            if (current.Count > 0) result.Add(current);
            return result;
        }

        private static bool TryBool(Dictionary<string, string> d, string k)
        {
            if (d.TryGetValue(k, out var s))
            {
                if (bool.TryParse(s, out var v)) return v;
                if (string.Equals(s, "Yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(s, "No", StringComparison.OrdinalIgnoreCase)) return false;
            }
            return false;
        }

        private static string? TryString(Dictionary<string, string> d, string k)
        {
            if (d.TryGetValue(k, out var s)) return s;
            return null;
        }
    }
}
