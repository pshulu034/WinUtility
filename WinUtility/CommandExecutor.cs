using System.Diagnostics;
using System.Text;

namespace WinUtility
{
    /// <summary>
    /// Windows命令行执行器
    /// 封装了命令行执行功能，支持同步和异步执行
    /// </summary>
    public class CommandExecutor
    {
        /// <summary>
        /// 命令执行结果
        /// </summary>
        public class CommandResult
        {
            /// <summary>
            /// 退出代码，0表示成功
            /// </summary>
            public int ExitCode { get; set; }

            /// <summary>
            /// 标准输出内容
            /// </summary>
            public string Output { get; set; } = string.Empty;

            /// <summary>
            /// 错误输出内容
            /// </summary>
            public string Error { get; set; } = string.Empty;

            /// <summary>
            /// 执行是否成功（ExitCode == 0）
            /// </summary>
            public bool IsSuccess => ExitCode == 0;

            /// <summary>
            /// 是否超时
            /// </summary>
            public bool IsTimeout { get; set; }

            /// <summary>
            /// 执行耗时（毫秒）
            /// </summary>
            public long ElapsedMilliseconds { get; set; }
        }

        /// <summary>
        /// 默认超时时间（毫秒），-1表示无超时
        /// </summary>
        public int DefaultTimeout { get; set; } = -1;

        /// <summary>
        /// 默认工作目录
        /// </summary>
        public string? DefaultWorkingDirectory { get; set; }

        /// <summary>
        /// 默认环境变量
        /// </summary>
        public Dictionary<string, string> DefaultEnvironmentVariables { get; set; } = new();

        /// <summary>
        /// 同步执行命令
        /// </summary>
        /// <param name="command">要执行的命令</param>
        /// <param name="arguments">命令参数</param>
        /// <param name="workingDirectory">工作目录，null使用默认值</param>
        /// <param name="timeout">超时时间（毫秒），-1表示无超时，null使用默认值</param>
        /// <param name="environmentVariables">环境变量，null使用默认值</param>
        /// <returns>命令执行结果</returns>
        public CommandResult Execute(
            string command,
            string? arguments = null,
            string? workingDirectory = null,
            int? timeout = null,
            Dictionary<string, string>? environmentVariables = null)
        {
            var startInfo = CreateProcessStartInfo(command, arguments, workingDirectory, environmentVariables);
            return ExecuteInternal(startInfo, timeout ?? DefaultTimeout);
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <param name="command">要执行的命令</param>
        /// <param name="arguments">命令参数</param>
        /// <param name="workingDirectory">工作目录，null使用默认值</param>
        /// <param name="timeout">超时时间（毫秒），-1表示无超时，null使用默认值</param>
        /// <param name="environmentVariables">环境变量，null使用默认值</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>命令执行结果</returns>
        public async Task<CommandResult> ExecuteAsync(
            string command,
            string? arguments = null,
            string? workingDirectory = null,
            int? timeout = null,
            Dictionary<string, string>? environmentVariables = null,
            CancellationToken cancellationToken = default)
        {
            var startInfo = CreateProcessStartInfo(command, arguments, workingDirectory, environmentVariables);
            return await ExecuteInternalAsync(startInfo, timeout ?? DefaultTimeout, cancellationToken);
        }

        /// <summary>
        /// 执行PowerShell命令
        /// </summary>
        /// <param name="script">PowerShell脚本</param>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>命令执行结果</returns>
        public CommandResult ExecutePowerShell(
            string script,
            string? workingDirectory = null,
            int? timeout = null)
        {
            return Execute(
                "powershell.exe",
                $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                workingDirectory,
                timeout
            );
        }

        /// <summary>
        /// 执行CMD命令
        /// </summary>
        /// <param name="command">CMD命令</param>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="environmentVariables">环境变量</param>
        /// <returns>命令执行结果</returns>
        public CommandResult ExecuteCmd(
            string command,
            string? workingDirectory = null,
            int? timeout = null,
            Dictionary<string, string>? environmentVariables = null)
        {
            return Execute(
                "cmd.exe",
                $"/c {command}",
                workingDirectory,
                timeout,
                environmentVariables
            );
        }

        /// <summary>
        /// 创建进程启动信息
        /// </summary>
        private ProcessStartInfo CreateProcessStartInfo(
            string command,
            string? arguments,
            string? workingDirectory,
            Dictionary<string, string>? environmentVariables)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? DefaultWorkingDirectory ?? Environment.CurrentDirectory
            };

            // 设置环境变量
            var envVars = environmentVariables ?? DefaultEnvironmentVariables;
            foreach (var kvp in envVars)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }

            return startInfo;
        }

        /// <summary>
        /// 内部执行方法（同步）
        /// </summary>
        private CommandResult ExecuteInternal(ProcessStartInfo startInfo, int timeout)
        {
            var result = new CommandResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var process = new Process { StartInfo = startInfo };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool completed;
                if (timeout > 0)
                {
                    completed = process.WaitForExit(timeout);
                    if (!completed)
                    {
                        process.Kill();
                        result.IsTimeout = true;
                        result.ExitCode = -1;
                    }
                    else
                    {
                        process.WaitForExit(); // 确保输出流处理完成
                        result.ExitCode = process.ExitCode;
                    }
                }
                else
                {
                    process.WaitForExit(); // 无超时，等待完成
                    result.ExitCode = process.ExitCode;
                    completed = true;
                }

                result.Output = outputBuilder.ToString().TrimEnd();
                result.Error = errorBuilder.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.ExitCode = -1;
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// 内部执行方法（异步）
        /// </summary>
        private async Task<CommandResult> ExecuteInternalAsync(
            ProcessStartInfo startInfo,
            int timeout,
            CancellationToken cancellationToken)
        {
            var result = new CommandResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var process = new Process { StartInfo = startInfo };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var outputTask = new TaskCompletionSource<bool>();
                var errorTask = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        outputTask.TrySetResult(true);
                    else
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        errorTask.TrySetResult(true);
                    else
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Task<bool> processTask;
                if (timeout > 0)
                {
                    processTask = Task.Run(() => process.WaitForExit(timeout), cancellationToken);
                }
                else
                {
                    processTask = Task.Run(() => { process.WaitForExit(); return true; }, cancellationToken);
                }

                var completedTask = await Task.WhenAny(processTask, Task.Delay(timeout > 0 ? timeout : int.MaxValue, cancellationToken));

                if (completedTask != processTask || cancellationToken.IsCancellationRequested)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        result.IsTimeout = true;
                        result.ExitCode = -1;
                    }
                }
                else
                {
                    await Task.WhenAll(outputTask.Task, errorTask.Task);
                    result.ExitCode = process.ExitCode;
                }

                result.Output = outputBuilder.ToString().TrimEnd();
                result.Error = errorBuilder.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.ExitCode = -1;
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }
    }
    

}
