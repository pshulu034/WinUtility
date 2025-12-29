using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace WinUtil
{
    public class StartupManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunOnceKey = @"Software\Microsoft\Windows\CurrentVersion\RunOnce";

        private readonly RegistryManager _registry;

        public StartupManager()
        {
            _registry = new RegistryManager();
        }

        public void EnableForCurrentUser(string appName, string executablePath, string? arguments = null, bool runOnce = false)
        {
            SetStartup(RegistryManager.RootHive.CurrentUser, appName, executablePath, arguments, runOnce);
        }

        public void DisableForCurrentUser(string appName, bool runOnce = false)
        {
            RemoveStartup(RegistryManager.RootHive.CurrentUser, appName, runOnce);
        }

        public bool IsEnabledForCurrentUser(string appName, bool runOnce = false)
        {
            return HasStartup(RegistryManager.RootHive.CurrentUser, appName, runOnce);
        }

        public void EnableForAllUsers(string appName, string executablePath, string? arguments = null, bool runOnce = false)
        {
            SetStartup(RegistryManager.RootHive.LocalMachine, appName, executablePath, arguments, runOnce);
        }

        public void DisableForAllUsers(string appName, bool runOnce = false)
        {
            RemoveStartup(RegistryManager.RootHive.LocalMachine, appName, runOnce);
        }

        public bool IsEnabledForAllUsers(string appName, bool runOnce = false)
        {
            return HasStartup(RegistryManager.RootHive.LocalMachine, appName, runOnce);
        }

        public IReadOnlyDictionary<string, string> ListCurrentUser(bool runOnce = false)
        {
            return ListStartup(RegistryManager.RootHive.CurrentUser, runOnce);
        }

        public IReadOnlyDictionary<string, string> ListAllUsers(bool runOnce = false)
        {
            return ListStartup(RegistryManager.RootHive.LocalMachine, runOnce);
        }

        private void SetStartup(RegistryManager.RootHive root, string appName, string executablePath, string? arguments, bool runOnce)
        {
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentException("appName 不能为空", nameof(appName));
            if (string.IsNullOrWhiteSpace(executablePath))
                throw new ArgumentException("executablePath 不能为空", nameof(executablePath));

            string exe = NormalizeExePath(executablePath);
            string value = ComposeCommand(exe, arguments);

            string subKey = runOnce ? RunOnceKey : RunKey;
            _registry.SetString(root, subKey, appName, value);
        }

        private void RemoveStartup(RegistryManager.RootHive root, string appName, bool runOnce)
        {
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentException("appName 不能为空", nameof(appName));

            string subKey = runOnce ? RunOnceKey : RunKey;
            _registry.DeleteValue(root, subKey, appName);
        }

        private bool HasStartup(RegistryManager.RootHive root, string appName, bool runOnce)
        {
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentException("appName 不能为空", nameof(appName));

            string subKey = runOnce ? RunOnceKey : RunKey;
            return _registry.ValueExists(root, subKey, appName);
        }

        private IReadOnlyDictionary<string, string> ListStartup(RegistryManager.RootHive root, bool runOnce)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string subKey = runOnce ? RunOnceKey : RunKey;

            var rootKey = GetRootKey(root);
            using var key = rootKey.OpenSubKey(subKey, writable: false);
            if (key == null)
                return dict;

            foreach (var name in key.GetValueNames())
            {
                var val = key.GetValue(name) as string;
                if (val != null)
                    dict[name] = val;
            }

            return dict;
        }

        private static RegistryKey GetRootKey(RegistryManager.RootHive root)
        {
            return root switch
            {
                RegistryManager.RootHive.CurrentUser => Registry.CurrentUser,
                RegistryManager.RootHive.LocalMachine => Registry.LocalMachine,
                RegistryManager.RootHive.ClassesRoot => Registry.ClassesRoot,
                RegistryManager.RootHive.Users => Registry.Users,
                _ => Registry.CurrentUser
            };
        }

        private static string NormalizeExePath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;
            return Path.GetFullPath(path);
        }

        private static string ComposeCommand(string executablePath, string? arguments)
        {
            string exeQuoted = "\"" + executablePath + "\"";
            if (string.IsNullOrWhiteSpace(arguments))
                return exeQuoted;
            return exeQuoted + " " + arguments;
        }
    }
}
