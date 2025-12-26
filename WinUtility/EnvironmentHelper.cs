using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinService
{
    public static class EnvironmentHelper
    {
        // -----------------------------
        // Read
        // -----------------------------

        public static string? Get(string name, EnvScope scope = EnvScope.User)
        {
            return Environment.GetEnvironmentVariable(name, ToTarget(scope));
        }

        public static string GetOrDefault(string name, string defaultValue,
                                          EnvScope scope = EnvScope.User)
        {
            return Get(name, scope) ?? defaultValue;
        }

        // -----------------------------
        // Write
        // -----------------------------

        public static void Set(string name, string value,
                               EnvScope scope = EnvScope.User)
        {
            Environment.SetEnvironmentVariable(name, value, ToTarget(scope));
        }

        // -----------------------------
        // Delete
        // -----------------------------

        public static void Remove(string name,
                                  EnvScope scope = EnvScope.User)
        {
            Environment.SetEnvironmentVariable(name, null, ToTarget(scope));
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private static EnvironmentVariableTarget ToTarget(EnvScope scope)
        {
            return scope switch
            {
                EnvScope.Process => EnvironmentVariableTarget.Process,
                EnvScope.User => EnvironmentVariableTarget.User,
                EnvScope.Machine => EnvironmentVariableTarget.Machine,
                _ => EnvironmentVariableTarget.User
            };
        }
    }

    public enum EnvScope
    {
        Process,   // current process only
        User,      // current user
        Machine    // system-wide (admin required)
    }
}
