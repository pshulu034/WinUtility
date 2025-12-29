using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtil;

namespace WinUtilDemo
{
    public class TCommandExecutor
    {
        public async static void Test()
        {
            Console.WriteLine("=== Windows 命令行执行器测试 ===\n");

            var executor = new CommandExecutor();

            // 示例1: 执行简单命令
            Console.WriteLine("示例1: 执行 ipconfig 命令");
            var result1 = executor.Execute("ipconfig");
            Console.WriteLine($"退出代码: {result1.ExitCode}");
            Console.WriteLine($"输出长度: {result1.Output.Length} 字符");
            Console.WriteLine($"执行耗时: {result1.ElapsedMilliseconds}ms\n");

            // 示例2: 执行CMD命令
            Console.WriteLine("示例2: 执行 dir 命令");
            var result2 = executor.ExecuteCmd("dir /b", Environment.CurrentDirectory);
            Console.WriteLine($"前100个字符:\n{result2.Output.Substring(0, Math.Min(100, result2.Output.Length))}...\n");

            // 示例3: 执行PowerShell命令
            Console.WriteLine("示例3: 执行 PowerShell 命令");
            var result3 = executor.ExecutePowerShell("Get-Date");
            Console.WriteLine($"当前时间: {result3.Output}\n");

            // 示例4: 异步执行
            Console.WriteLine("示例4: 异步执行 ping 命令");
            var result4 = await executor.ExecuteAsync("ping", "127.0.0.1 -n 2", timeout: 5000);
            Console.WriteLine($"执行成功: {result4.IsSuccess}");
            Console.WriteLine($"输出:\n{result4.Output.Substring(0, Math.Min(200, result4.Output.Length))}...\n");

            // 示例5: 超时测试
            Console.WriteLine("示例5: 超时测试（3秒超时）");
            var result5 = executor.Execute("ping", "127.0.0.1 -n 100", timeout: 3000);
            if (result5.IsTimeout)
            {
                Console.WriteLine("✓ 命令已按预期超时\n");
            }
            else
            {
                Console.WriteLine("✗ 命令未超时\n");
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// CommandExecutor 使用示例
    /// </summary>
    public class CommandExecutorExample
    {
        /// <summary>
        /// 基本使用示例
        /// </summary>
        public static void BasicExample()
        {
            var executor = new CommandExecutor();

            // 执行简单的命令
            var result = executor.Execute("ipconfig", "/all");

            Console.WriteLine($"退出代码: {result.ExitCode}");
            Console.WriteLine($"执行成功: {result.IsSuccess}");
            Console.WriteLine($"耗时: {result.ElapsedMilliseconds}ms");
            Console.WriteLine($"输出:\n{result.Output}");

            if (!string.IsNullOrEmpty(result.Error))
            {
                Console.WriteLine($"错误:\n{result.Error}");
            }
        }

        /// <summary>
        /// 执行CMD命令示例
        /// </summary>
        public static void CmdExample()
        {
            var executor = new CommandExecutor();

            // 执行CMD命令
            var result = executor.ExecuteCmd("dir", Environment.CurrentDirectory, timeout: 5000);

            Console.WriteLine($"命令执行结果:");
            Console.WriteLine(result.Output);
        }

        /// <summary>
        /// 执行PowerShell命令示例
        /// </summary>
        public static void PowerShellExample()
        {
            var executor = new CommandExecutor();

            // 执行PowerShell命令
            var script = "Get-Process | Select-Object -First 5 Name, CPU";
            var result = executor.ExecutePowerShell(script);

            Console.WriteLine($"PowerShell执行结果:");
            Console.WriteLine(result.Output);
        }

        /// <summary>
        /// 异步执行示例
        /// </summary>
        public static async Task AsyncExample()
        {
            var executor = new CommandExecutor();

            // 异步执行命令
            var result = await executor.ExecuteAsync(
                "ping",
                "127.0.0.1 -n 4",
                timeout: 10000
            );

            Console.WriteLine($"异步执行完成:");
            Console.WriteLine($"退出代码: {result.ExitCode}");
            Console.WriteLine($"输出:\n{result.Output}");
        }

        /// <summary>
        /// 带环境变量的示例
        /// </summary>
        public static void EnvironmentVariableExample()
        {
            var executor = new CommandExecutor();

            // 设置环境变量
            var envVars = new Dictionary<string, string>
            {
                { "MY_VAR", "Hello World" },
                { "PATH", Environment.GetEnvironmentVariable("PATH") ?? "" }
            };

            // Windows下使用set命令查看环境变量
            var result = executor.ExecuteCmd($"echo %MY_VAR%", environmentVariables: envVars);

            Console.WriteLine($"环境变量示例:");
            Console.WriteLine(result.Output);
        }

        /// <summary>
        /// 超时处理示例
        /// </summary>
        public static void TimeoutExample()
        {
            var executor = new CommandExecutor();

            // 设置5秒超时
            var result = executor.Execute(
                "ping",
                "127.0.0.1 -n 100", // 这会执行很长时间
                timeout: 5000
            );

            if (result.IsTimeout)
            {
                Console.WriteLine("命令执行超时！");
            }
            else
            {
                Console.WriteLine($"命令执行完成，退出代码: {result.ExitCode}");
            }
        }

        /// <summary>
        /// 链式调用示例
        /// </summary>
        public static void ChainedExample()
        {
            var executor = new CommandExecutor
            {
                DefaultTimeout = 10000, // 默认10秒超时
                DefaultWorkingDirectory = Environment.CurrentDirectory
            };

            // 执行多个命令
            var commands = new[]
            {
                "echo Hello",
                "echo World",
                "dir"
            };

            foreach (var cmd in commands)
            {
                var result = executor.ExecuteCmd(cmd);
                Console.WriteLine($"执行: {cmd}");
                Console.WriteLine($"结果: {result.Output}\n");
            }
        }

        /// <summary>
        /// 运行所有示例
        /// </summary>
        public static async Task RunAllExamples()
        {
            Console.WriteLine("=== CommandExecutor 示例 ===\n");

            Console.WriteLine("1. 基本示例:");
            BasicExample();
            Console.WriteLine("\n" + new string('-', 50) + "\n");

            Console.WriteLine("2. CMD命令示例:");
            CmdExample();
            Console.WriteLine("\n" + new string('-', 50) + "\n");

            Console.WriteLine("3. PowerShell命令示例:");
            PowerShellExample();
            Console.WriteLine("\n" + new string('-', 50) + "\n");

            Console.WriteLine("4. 异步执行示例:");
            await AsyncExample();
            Console.WriteLine("\n" + new string('-', 50) + "\n");

            Console.WriteLine("5. 超时处理示例:");
            TimeoutExample();
            Console.WriteLine("\n" + new string('-', 50) + "\n");

            Console.WriteLine("6. 链式调用示例:");
            ChainedExample();
        }
    }
}
