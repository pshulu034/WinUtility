using System;
using System.Collections.Generic;
using System.IO;

namespace WinUtil
{
    public static class PathHelper
    {
        #region 系统目录
        //C:\WINDOWS\system32
        public static string System32()
        {
            var p = Environment.SystemDirectory;
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\WINDOWS
        public static string Windows()
        {
            var p = Environment.GetEnvironmentVariable("SystemRoot");
            if (!string.IsNullOrEmpty(p)) return p;
            var sys = Environment.SystemDirectory;
            var dir = Directory.GetParent(sys);
            return dir?.FullName ?? sys;
        }

        public static string ProgramFiles()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        public static string ProgramFilesX86()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            return string.IsNullOrEmpty(p) ? ProgramFiles() : p;
        }

        //C:\ProgramData
        public static string ProgramData()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return string.IsNullOrEmpty(p) ? "" : p;
        }
        #endregion

        #region 用户与应用数据
        //C:\Users\用户名\AppData\Roaming
        public static string AppDataRoaming()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\用户名\AppData\Local
        public static string AppDataLocal()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\passenger
        public static string UserProfile()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return string.IsNullOrEmpty(p) ? "" : p;
        }
        #endregion

        #region 桌面与文档类
        //C:\Users\passenger\Desktop
        public static string Desktop()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\passenger\Documents
        public static string Documents()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\passenger\Downloads
        public static string Downloads()
        {
            var p = Path.Combine(UserProfile(), "Downloads");
            return p;
        }

        //C:\Users\passenger\Pictures
        public static string Pictures()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        // C:\Users\passenger\Music
        public static string Music()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\passenger\Videos
        public static string Videos()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            return string.IsNullOrEmpty(p) ? "" : p;
        }
        #endregion

        #region 开机启动与菜单
        //C:\Users\passenger\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
        public static string StartupCurrentUser()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\passenger\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
        public static string StartMenuCurrentUser()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup
        public static string StartupAllUsers()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\ProgramData\Microsoft\Windows\Start Menu
        public static string StartMenuAllUsers()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            return string.IsNullOrEmpty(p) ? "" : p;
        }
        #endregion

        #region 公共用户目录
        // C:\Users\Public\Desktop
        public static string PublicDesktop()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        //C:\Users\Public\Documents
        public static string PublicDocuments()
        {
            var p = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            return string.IsNullOrEmpty(p) ? "" : p;
        }
        #endregion

        //C:\Users\passenger\AppData\Local\Temp\
        public static string Temp()
        {
            return Path.GetTempPath();
        }

        //C:\Users\passenger\OneDrive
        public static string OneDrive()
        {
            var p = Environment.GetEnvironmentVariable("OneDrive");
            return string.IsNullOrEmpty(p) ? "" : p;
        }

        public static string CurrentAppDirectory()
        {
            var p = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(p)) return "";
            return Path.GetFullPath(p);
        }

        public static IReadOnlyDictionary<string, string> CommonMap()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ProgramFiles"] = ProgramFiles(),
                ["ProgramFilesX86"] = ProgramFilesX86(),
                ["ProgramData"] = ProgramData(),
                ["AppDataRoaming"] = AppDataRoaming(),
                ["AppDataLocal"] = AppDataLocal(),
                ["UserProfile"] = UserProfile(),
                ["Desktop"] = Desktop(),
                ["Documents"] = Documents(),
                ["Downloads"] = Downloads(),
                ["Pictures"] = Pictures(),
                ["Music"] = Music(),
                ["Videos"] = Videos(),
                ["Temp"] = Temp(),
                ["System32"] = System32(),
                ["Windows"] = Windows(),
                ["AppBaseDir"] = CurrentAppDirectory(),
                ["StartupCU"] = StartupCurrentUser(),
                ["StartupAll"] = StartupAllUsers(),
                ["StartMenuCU"] = StartMenuCurrentUser(),
                ["StartMenuAll"] = StartMenuAllUsers(),
                ["PublicDesktop"] = PublicDesktop(),
                ["PublicDocuments"] = PublicDocuments(),
                ["OneDrive"] = OneDrive()
            };
            return dict;
        }
    }
}
