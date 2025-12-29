//dotnet add package System.ServiceProcess.ServiceController

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using static WinUtil.RegistryManager;

namespace WinUtil
{
    /// <summary>
    /// Windows 服务管理封装：
    /// - 通用：查询状态、启动、停止、重启、等待状态
    /// - 典型服务：打印服务、Windows Update、时间同步等快捷方法
    /// </summary>
    public class ServiceManager
    {
        /// <summary>
        /// 获取服务当前状态（不存在或异常返回 null）
        /// </summary>
        public ServiceControllerStatus? GetStatus(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                return sc.Status;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 服务是否存在
        /// </summary>
        public bool Exists(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                // 访问 Status 即可验证
                var _ = sc.Status;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start(string serviceName, TimeSpan? timeout = null)
        {
            using var sc = new ServiceController(serviceName);

            if (sc.Status == ServiceControllerStatus.Running)
                return;

            if (sc.Status == ServiceControllerStatus.Paused)
            {
                sc.Continue();
            }
            else
            {
                sc.Start();
            }

            sc.WaitForStatus(ServiceControllerStatus.Running, timeout ?? TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop(string serviceName, TimeSpan? timeout = null)
        {
            using var sc = new ServiceController(serviceName);

            if (sc.Status == ServiceControllerStatus.Stopped)
                return;

            if (sc.CanStop)
            {
                sc.Stop();
            }

            sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout ?? TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// 重启服务（先停后启）
        /// </summary>
        public void Restart(string serviceName, TimeSpan? timeoutPerStep = null)
        {
            var t = timeoutPerStep ?? TimeSpan.FromSeconds(30);
            Stop(serviceName, t);
            Start(serviceName, t);
        }

        /// <summary>
        /// 异步重启服务
        /// </summary>
        public Task RestartAsync(string serviceName, TimeSpan? timeoutPerStep = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Restart(serviceName, timeoutPerStep);
            }, cancellationToken);
        }

        /// <summary>
        /// 等待服务达到指定状态（不存在时抛异常）
        /// </summary>
        public void WaitForStatus(string serviceName, ServiceControllerStatus desiredStatus, TimeSpan? timeout = null)
        {
            using var sc = new ServiceController(serviceName);
            sc.WaitForStatus(desiredStatus, timeout ?? TimeSpan.FromSeconds(30));
        }

    }

    public static class ServiceExtensions
    {
        #region 典型服务
        // ===== 典型常用 Windows 服务的快捷方法 =====

        /// <summary>
        /// 打印后台处理程序（Print Spooler）服务名
        /// </summary>
        public const string ServicePrintSpooler = "Spooler";

        /// <summary>
        /// Windows 时间服务
        /// </summary>
        public const string ServiceWindowsTime = "W32Time";

        /// <summary>
        /// Windows Update 服务
        /// </summary>
        public const string ServiceWindowsUpdate = "wuauserv";

        /// <summary>
        /// DNS Client 服务
        /// </summary>
        public const string ServiceDnsClient = "Dnscache";

        /// <summary>
        /// Windows 防火墙（Windows Defender Firewall）
        /// </summary>
        public const string ServiceWindowsFirewall = "MpsSvc";

        /// <summary>
        /// 打印服务：重启 Print Spooler
        /// </summary>
        public static void RestartPrintSpooler(this ServiceManager sc)
        {
            sc.Restart(ServicePrintSpooler);
        }

        /// <summary>
        /// Windows Update：启动
        /// </summary>
        public static void StartWindowsUpdate(this ServiceManager sc)
        {
            sc.Start(ServiceWindowsUpdate);
        }

        /// <summary>
        /// Windows Update：停止
        /// </summary>
        public static void StopWindowsUpdate(this ServiceManager sc)
        {
            sc.Stop(ServiceWindowsUpdate);
        }

        /// <summary>
        /// Windows 时间服务：强制重同步（依赖命令行）
        /// </summary>
        public static void ResyncWindowsTime(this ServiceManager sc)
        {
            // 这里只是提供一个简单示例，真正的时间同步通常用 w32tm 工具
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "w32tm.exe",
                    Arguments = "/resync",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(30000);
        }

        /// <summary>
        /// DNS 客户端：重启以清空 DNS 缓存
        /// </summary>
        public static void RestartDnsClient(this ServiceManager sc)
        {
            sc.Restart(ServiceDnsClient);
        }

        /// <summary>
        /// Windows 防火墙：启动
        /// </summary>
        public static void StartFirewall(this ServiceManager sc)
        {
            sc.Start(ServiceWindowsFirewall);
        }

        /// <summary>
        /// Windows 防火墙：停止（不推荐，仅调试环境使用）
        /// </summary>
        public static void StopFirewall(this ServiceManager sc)
        {
            sc.Stop(ServiceWindowsFirewall);
        }
        #endregion
    }
}
