using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WinUtil
{
    public class WindowManager
    {
        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
            public int ProcessId { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Visible { get; set; }
            public bool TopMost { get; set; }
        }

        public IReadOnlyList<WindowInfo> EnumerateWindows(bool onlyVisible = true)
        {
            var result = new List<WindowInfo>();
            EnumWindows((hWnd, lParam) =>
            {
                bool visible = IsWindowVisible(hWnd);
                if (onlyVisible && !visible) return true;
                var info = GetWindowInfo(hWnd);
                if (info != null) result.Add(info);
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public WindowInfo? GetWindowInfo(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return null;
            var sb = new StringBuilder(512);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString();

            sb.Clear();
            GetClassName(hWnd, sb, 256);
            string cls = sb.ToString();

            GetWindowThreadProcessId(hWnd, out int pid);
            var rect = new RECT();
            GetWindowRect(hWnd, out rect);

            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            bool topMost = (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
            bool visible = IsWindowVisible(hWnd);

            return new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ClassName = cls,
                ProcessId = pid,
                X = rect.Left,
                Y = rect.Top,
                Width = rect.Right - rect.Left,
                Height = rect.Bottom - rect.Top,
                Visible = visible,
                TopMost = topMost
            };
        }

        public bool SetTopMost(IntPtr hWnd, bool topMost)
        {
            if (hWnd == IntPtr.Zero) return false;
            IntPtr insertAfter = topMost ? HWND_TOPMOST : HWND_NOTOPMOST;
            return SetWindowPos(hWnd, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public bool Minimize(IntPtr hWnd) => ShowWindow(hWnd, SW_MINIMIZE);
        public bool Maximize(IntPtr hWnd) => ShowWindow(hWnd, SW_MAXIMIZE);
        public bool Restore(IntPtr hWnd) => ShowWindow(hWnd, SW_RESTORE);
        public bool Activate(IntPtr hWnd) => SetForegroundWindow(hWnd);

        public bool Move(IntPtr hWnd, int x, int y)
        {
            if (hWnd == IntPtr.Zero) return false;
            var info = GetWindowInfo(hWnd);
            if (info == null) return false;
            return MoveWindow(hWnd, x, y, info.Width, info.Height, true);
        }

        public bool Resize(IntPtr hWnd, int width, int height)
        {
            if (hWnd == IntPtr.Zero) return false;
            var info = GetWindowInfo(hWnd);
            if (info == null) return false;
            return MoveWindow(hWnd, info.X, info.Y, width, height, true);
        }

        public bool MoveResize(IntPtr hWnd, int x, int y, int width, int height)
        {
            if (hWnd == IntPtr.Zero) return false;
            return MoveWindow(hWnd, x, y, width, height, true);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0001;
        private const uint SWP_NOSIZE = 0x0002;
        private const int SW_MINIMIZE = 6;
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
