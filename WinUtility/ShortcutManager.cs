using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WinUtility
{
    public class ShortcutManager
    {
        public string CreateShortcut(string shortcutPath, string targetPath, string? arguments = null, string? workingDirectory = null, string? iconPath = null, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(shortcutPath)) throw new ArgumentException(nameof(shortcutPath));
            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException(nameof(targetPath));
            Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);
            var link = (IShellLinkW)new CShellLink();
            link.SetPath(targetPath);
            if (!string.IsNullOrWhiteSpace(arguments)) link.SetArguments(arguments);
            if (!string.IsNullOrWhiteSpace(workingDirectory)) link.SetWorkingDirectory(workingDirectory);
            if (!string.IsNullOrWhiteSpace(iconPath)) link.SetIconLocation(iconPath, 0);
            if (!string.IsNullOrWhiteSpace(description)) link.SetDescription(description);
            var pf = (IPersistFile)link;
            pf.Save(shortcutPath, true);
            return shortcutPath;
        }

        public bool DeleteShortcut(string shortcutPath)
        {
            if (string.IsNullOrWhiteSpace(shortcutPath)) return false;
            if (!File.Exists(shortcutPath)) return false;
            try { File.Delete(shortcutPath); return true; } catch { return false; }
        }

        public string CreateDesktopShortcut(string name, string targetPath, string? arguments = null, string? iconPath = null)
        {
            var path = Path.Combine(PathEx.Desktop(), name + ".lnk");
            return CreateShortcut(path, targetPath, arguments, Path.GetDirectoryName(targetPath), iconPath, name);
        }

        public bool DeleteDesktopShortcut(string name)
        {
            var path = Path.Combine(PathEx.Desktop(), name + ".lnk");
            return DeleteShortcut(path);
        }

        public string CreateStartMenuShortcut(string name, string targetPath, bool allUsers = false, string? arguments = null, string? iconPath = null)
        {
            var baseDir = allUsers ? PathEx.StartMenuAllUsers() : PathEx.StartMenuCurrentUser();
            var programs = Path.Combine(baseDir, "Programs");
            var path = Path.Combine(programs, name + ".lnk");
            return CreateShortcut(path, targetPath, arguments, Path.GetDirectoryName(targetPath), iconPath, name);
        }

        public bool DeleteStartMenuShortcut(string name, bool allUsers = false)
        {
            var baseDir = allUsers ? PathEx.StartMenuAllUsers() : PathEx.StartMenuCurrentUser();
            var programs = Path.Combine(baseDir, "Programs");
            var path = Path.Combine(programs, name + ".lnk");
            return DeleteShortcut(path);
        }

        public bool PinToTaskbar(string shortcutPath)
        {
            if (!File.Exists(shortcutPath)) return false;
            var script = "$shell = New-Object -ComObject Shell.Application; " +
                         "$f = $shell.Namespace((Split-Path '" + shortcutPath.Replace("'", "''") + "')); " +
                         "$i = $f.ParseName((Split-Path '" + shortcutPath.Replace("'", "''") + "' -Leaf)); " +
                         "$verbs = $i.Verbs(); " +
                         "foreach($v in $verbs){ if($v.Name -match 'Pin to taskbar|固定到任务栏'){ $v.DoIt(); break } }";
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell(script, timeout: 15000);
            return res.IsSuccess;
        }

        public bool UnpinFromTaskbar(string shortcutPath)
        {
            if (!File.Exists(shortcutPath)) return false;
            var script = "$shell = New-Object -ComObject Shell.Application; " +
                         "$f = $shell.Namespace((Split-Path '" + shortcutPath.Replace("'", "''") + "')); " +
                         "$i = $f.ParseName((Split-Path '" + shortcutPath.Replace("'", "''") + "' -Leaf)); " +
                         "$verbs = $i.Verbs(); " +
                         "foreach($v in $verbs){ if($v.Name -match 'Unpin from taskbar|从任务栏取消固定'){ $v.DoIt(); break } }";
            var exec = new CommandExecutor();
            var res = exec.ExecutePowerShell(script, timeout: 15000);
            return res.IsSuccess;
        }
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class CShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLinkW
    {
        int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, out WIN32_FIND_DATAW pfd, uint fFlags);
        int GetIDList(out IntPtr ppidl);
        int SetIDList(IntPtr pidl);
        int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
        int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
        int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
        int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        int GetHotkey(out short pwHotkey);
        int SetHotkey(short wHotkey);
        int GetShowCmd(out int piShowCmd);
        int SetShowCmd(int iShowCmd);
        int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
        int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        int Resolve(IntPtr hwnd, uint fFlags);
        int SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    internal interface IPersistFile
    {
        int GetClassID(out Guid pClassID);
        int IsDirty();
        int Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        int Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
        int SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        int GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }
}
