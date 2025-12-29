using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtil;

namespace WinUtilDemo
{
    public class TRegistryManager
    {
        public static void Test()
        {
            // 1. 在当前用户下写入/读取配置
            var rm = new RegistryManager();
            var root = RegistryManager.RootHive.CurrentUser;
            string subKey = @"Software\MyApp\Settings";

            // 写入
            rm.SetString(root, subKey, "UserName", "Alice");
            rm.SetInt(root, subKey, "Age", 30);
            rm.SetBool(root, subKey, "IsEnabled", true);

            // 读取
            string? name = rm.GetString(root, subKey, "UserName", "Guest");
            int age = rm.GetInt(root, subKey, "Age", 0);
            bool enabled = rm.GetBool(root, subKey, "IsEnabled", false);

            Console.WriteLine($"Name={name}, Age={age}, Enabled={enabled}");

            //2. 判断键 / 值是否存在，删除键值
             subKey = @"Software\MyApp\Settings";

            // 判断子键是否存在
            bool keyExists = rm.SubKeyExists(root, subKey);

            // 判断某个值是否存在
            bool hasUserName = rm.ValueExists(root, subKey, "UserName");

            // 删除某个键值
            bool deletedValue = rm.DeleteValue(root, subKey, "UserName");

            // 删除整个子键（包含所有子项和值）
            bool deletedKey = rm.DeleteSubKey(root, @"Software\MyApp", recursive: true);

            //3. 访问其他根键（例如 HKEY_LOCAL_MACHINE 和 HKEY_CLASSES_ROOT）
            var lmRoot = RegistryManager.RootHive.LocalMachine;
            string winVerKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

            string? productName = rm.GetString(lmRoot, winVerKey, "ProductName", "Unknown");
            Console.WriteLine($"Windows: {productName}");

            // 访问 HKEY_CLASSES_ROOT 示例（检查 .txt 关联是否存在）
            var crRoot = RegistryManager.RootHive.ClassesRoot;
            bool txtExists = rm.SubKeyExists(crRoot, @".txt");
            Console.WriteLine($".txt 关联是否存在: {txtExists}");

        }    
        
        public static void TestProxy()
        {
            RegistryManager reg = new RegistryManager();
            reg.SwitchIEProxy(true);
            reg.SetProxyServer("192.168.2.128", 3280);
        }
    }
}
