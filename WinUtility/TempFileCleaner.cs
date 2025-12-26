using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinService
{
    public static class TempFileCleaner
    {
        // 清理临时文件
        public static void CleanTempFiles()
        {
            try
            {
                // 获取系统临时目录
                string tempPath = Path.GetTempPath();
                CleanDirectory(tempPath);

                // 获取浏览器缓存的临时文件路径
                string[] browserCachePaths = new string[]
                {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\Default\\Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data\\Default\\Cache")
                };

                // 清理浏览器缓存
                foreach (var cachePath in browserCachePaths)
                {
                    if (Directory.Exists(cachePath))
                    {
                        CleanDirectory(cachePath);
                    }
                }

                // 这里你可以添加更多应用程序或系统路径

                Console.WriteLine("临时文件清理完成!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理过程中发生错误: {ex.Message}");
            }
        }

        // 清理目录中的所有文件和子目录
        public static void CleanDirectory(string path)
        {
            try
            {
                // 确保目录存在
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"无法删除文件 {file}: {ex.Message}");
                        }
                    }

                    var directories = Directory.GetDirectories(path);
                    foreach (var directory in directories)
                    {
                        try
                        {
                            Directory.Delete(directory, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"无法删除目录 {directory}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理目录时发生错误: {ex.Message}");
            }
        }

        // 清理指定的文件类型（如 *.log, *.bak等）
        public static void CleanSpecificFiles(string directoryPath, string filePattern)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, filePattern);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"无法删除文件 {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理文件时发生错误: {ex.Message}");
            }
        }
    }
}
