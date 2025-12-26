namespace TWinService;
using System;
using System.IO;
using WinService;

public class TTempFileCleaner
{
    public static void Test()
    {
        try
        {
            // 1. 清理系统临时文件
            Console.WriteLine("清理系统临时文件...");
            TempFileCleaner.CleanTempFiles();
            Console.WriteLine("系统临时文件清理完成!");

            // 2. 清理浏览器缓存
            Console.WriteLine("清理浏览器缓存...");
            string[] browserCachePaths = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\Default\\Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data\\Default\\Cache")
            };

            foreach (var cachePath in browserCachePaths)
            {
                if (Directory.Exists(cachePath))
                {
                    TempFileCleaner.CleanDirectory(cachePath);
                    Console.WriteLine($"清理浏览器缓存：{cachePath}");
                }
            }

            // 3. 清理应用程序日志文件（示例路径）
            Console.WriteLine("清理应用程序日志文件...");
            TempFileCleaner.CleanSpecificFiles(@"C:\MyApp\Logs", "*.log");
            Console.WriteLine("应用程序日志文件清理完成!");

            // 4. 清理 Windows 更新缓存（可选）
            string updateCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution\\Download");
            if (Directory.Exists(updateCachePath))
            {
                TempFileCleaner.CleanDirectory(updateCachePath);
                Console.WriteLine("Windows 更新缓存清理完成!");
            }

            Console.WriteLine("所有清理操作已完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误：{ex.Message}");
        }
    }
}

