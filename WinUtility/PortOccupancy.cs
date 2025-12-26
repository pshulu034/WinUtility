namespace WinService;
using System;
using System.Diagnostics;

public class PortOccupancy
{
    // 使用 netstat 查询端口占用
    public static string FindProcessByPortWithNetstat(int port)
    {
        try
        {
            // 获取 netstat 输出
            var processInfo = RunNetstatCommand(port);
            if (string.IsNullOrEmpty(processInfo))
            {
                return $"端口 {port} 没有被占用.";
            }
            return processInfo;
        }
        catch (Exception ex)
        {
            return $"查询失败: {ex.Message}";
        }
    }

    // 执行 netstat 命令并过滤出相关端口信息
    private static string RunNetstatCommand(int port)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = $"-ano | findstr :{port}",  // 使用 findstr 筛选特定端口的连接
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // 如果有输出则返回
        if (!string.IsNullOrEmpty(output))
        {
            return $"端口 {port} 被以下进程占用：\n{output}";
        }

        return null;
    }

    // 根据进程ID获取进程名称
    private static string GetProcessNameById(int processId)
    {
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch (Exception ex)
        {
            return $"无法获取进程名称: {ex.Message}";
        }
    }
}
