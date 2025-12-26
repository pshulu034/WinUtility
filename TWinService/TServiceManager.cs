using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WinUtility;

namespace ConsoleApp
{
    public class TServiceManager
    {
        public async static void Test()
        {
            //1. public async static void Test()
            var sm = new ServiceManager();

            #region 获取状态 / 判断是否存在
            // 获取服务状态（不存在返回 null）
            var status = sm.GetStatus("Spooler");

            // 判断服务是否存在
            bool exists = sm.Exists("Spooler");

            #endregion

            #region 启动 / 停止 / 重启 / 等待状态
            // 启动服务（默认 30 秒超时）
            sm.Start("Spooler");

            // 停止服务
            sm.Stop("Spooler");

            // 重启服务
            sm.Restart("Spooler");

            // 异步重启
            await sm.RestartAsync("Spooler");

            // 等待服务到达指定状态
            sm.WaitForStatus("Spooler", ServiceControllerStatus.Running);
            #endregion
        }
    }
}
