using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinUtility.RegistryManager;

namespace WinUtility
{
    /// <summary>
    /// Windows 注册表读写封装
    /// 仅操作当前用户 / 本地计算机两个主键，封装常见的增删改查
    /// </summary>
    public class RegistryManager
    {
        /// <summary>
        /// 根节点类型（常用四种）
        /// </summary>
        public enum RootHive
        {
            /// <summary>HKEY_CURRENT_USER</summary>
            CurrentUser,

            /// <summary>HKEY_LOCAL_MACHINE</summary>
            LocalMachine,

            /// <summary>HKEY_CLASSES_ROOT</summary>
            ClassesRoot,

            /// <summary>HKEY_USERS</summary>
            Users,
        }

        /// <summary>
        /// 根据 RootHive 获取对应的 RegistryKey
        /// </summary>
        private static RegistryKey GetRootKey(RootHive root)
        {
            return root switch
            {
                RootHive.CurrentUser => Registry.CurrentUser,
                RootHive.LocalMachine => Registry.LocalMachine,
                RootHive.ClassesRoot => Registry.ClassesRoot,
                RootHive.Users => Registry.Users,
                _ => throw new ArgumentOutOfRangeException(nameof(root), root, null)
            };
        }

        #region 子键操作
        /// <summary>
        /// 创建（或打开）子键，返回可写 RegistryKey
        /// 例如 subKey: "Software\\MyApp"
        /// </summary>
        public RegistryKey CreateOrOpenSubKey(RootHive root, string subKey)
        {
            if (string.IsNullOrWhiteSpace(subKey))
                throw new ArgumentException("子键路径不能为空", nameof(subKey));

            var rootKey = GetRootKey(root);
            return rootKey.CreateSubKey(subKey, writable: true)
                   ?? throw new InvalidOperationException($"无法创建或打开子键: {subKey}");
        }

        /// <summary>
        /// 判断子键是否存在
        /// </summary>
        public bool SubKeyExists(RootHive root, string subKey)
        {
            if (string.IsNullOrWhiteSpace(subKey))
                throw new ArgumentException("子键路径不能为空", nameof(subKey));

            var rootKey = GetRootKey(root);
            using var key = rootKey.OpenSubKey(subKey, writable: false);
            return key != null;
        }

        /// <summary>
        /// 删除子键（可以选择是否级联删除子项）
        /// </summary>
        public bool DeleteSubKey(RootHive root, string subKey, bool recursive = true)
        {
            if (string.IsNullOrWhiteSpace(subKey))
                throw new ArgumentException("子键路径不能为空", nameof(subKey));

            var rootKey = GetRootKey(root);

            try
            {
                if (recursive)
                    rootKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
                else
                    rootKey.DeleteSubKey(subKey, throwOnMissingSubKey: false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 键值操作
        /// <summary>
        /// 删除某个键值
        /// </summary>
        public bool DeleteValue(RootHive root, string subKey, string valueName)
        {
            try
            {
                var rootKey = GetRootKey(root);
                using var key = rootKey.OpenSubKey(subKey, writable: true);
                if (key == null)
                    return false;

                key.DeleteValue(valueName, throwOnMissingValue: false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断某个键值是否存在
        /// </summary>
        public bool ValueExists(RootHive root, string subKey, string valueName)
        {
            var rootKey = GetRootKey(root);
            using var key = rootKey.OpenSubKey(subKey, writable: false);
            if (key == null)
                return false;

            return Array.Exists(key.GetValueNames(), n => string.Equals(n, valueName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region 键值读写
        /// <summary>
        /// 读取原始对象值（用于自定义处理）
        /// </summary>
        public object? GetValue(RootHive root, string subKey, string valueName)
        {
            var rootKey = GetRootKey(root);
            using var key = rootKey.OpenSubKey(subKey, writable: false);
            if (key == null)
                return null;

            return key.GetValue(valueName, null);
        }

        /// <summary>
        /// 写入字符串值
        /// </summary>
        public void SetString(RootHive root, string subKey, string valueName, string? value)
        {
            using var key = CreateOrOpenSubKey(root, subKey);
            key.SetValue(valueName, value ?? string.Empty, RegistryValueKind.String);
        }

        /// <summary>
        /// 读取字符串值（不存在时返回 defaultValue）
        /// </summary>
        public string? GetString(RootHive root, string subKey, string valueName, string? defaultValue = null)
        {
            var obj = GetValue(root, subKey, valueName);
            return obj is string s ? s : defaultValue;
        }

        /// <summary>
        /// 写入整型值（32位）
        /// </summary>
        public void SetInt(RootHive root, string subKey, string valueName, int value)
        {
            using var key = CreateOrOpenSubKey(root, subKey);
            key.SetValue(valueName, value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// 读取整型值（不存在或类型不匹配时返回 defaultValue）
        /// </summary>
        public int GetInt(RootHive root, string subKey, string valueName, int defaultValue = 0)
        {
            var obj = GetValue(root, subKey, valueName);

            return obj switch
            {
                int i => i,
                string s when int.TryParse(s, out var parsed) => parsed,
                _ => defaultValue
            };
        }

        /// <summary>
        /// 写入布尔值（内部以 0/1 存储为 DWord）
        /// </summary>
        public void SetBool(RootHive root, string subKey, string valueName, bool value)
        {
            SetInt(root, subKey, valueName, value ? 1 : 0);
        }

        /// <summary>
        /// 读取布尔值（不存在时返回 defaultValue）
        /// </summary>
        public bool GetBool(RootHive root, string subKey, string valueName, bool defaultValue = false)
        {
            var i = GetInt(root, subKey, valueName, defaultValue ? 1 : 0);
            return i != 0;
        }
        #endregion
    
    }

    public static class ProxyExtensions
    {
        private static string subKey_Proxy = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

        //开启/禁用代理
        public static void SwitchIEProxy(this RegistryManager reg, bool enabled)
        {
            reg.SetInt(RootHive.CurrentUser, subKey_Proxy, "ProxyEnable", enabled ? 1 : 0);
        }

        // 设置代理服务器地址和端口
        public static void SetProxyServer(this RegistryManager reg, string ip, ushort port)
        {
            string endPoint = ip + ":" + port.ToString();
            reg.SetString(RootHive.CurrentUser, subKey_Proxy, "ProxyServer", endPoint);
        }
    }
    
}
