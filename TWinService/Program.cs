using ConsoleApp;
using TWinService;
using WinService;
using WinUtil;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        int port = 8080;
        TLocalUserGroupManager.Test();
    }
}