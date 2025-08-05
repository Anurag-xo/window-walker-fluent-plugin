using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WindowWalker.Fluent.Plugin
{
    public static class WindowEnumerator
    {
        public static List<WindowInfo> GetOpenWindows()
        {
            var windows = new List<WindowInfo>();

            Win32Api.EnumWindows((hWnd, lParam) =>
            {
                if (Win32Api.IsWindowVisible(hWnd))
                {
                    int pid;
                    Win32Api.GetWindowThreadProcessId(hWnd, out pid);

                    try
                    {
                        var process = Process.GetProcessById(pid);
                        var title = GetWindowText(hWnd);
                        if (string.IsNullOrWhiteSpace(title) || process == null)
                            return true;

                        // Skip Fluent Search itself
                        if (process.ProcessName.Equals("FluentSearch", StringComparison.OrdinalIgnoreCase))
                            return true;

                        windows.Add(new WindowInfo
                        {
                            Title = title,
                            ProcessName = process.ProcessName,
                            Pid = pid,
                            Handle = hWnd
                        });
                    }
                    catch (Exception)
                    {
                        // Ignore inaccessible processes
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            var buffer = new char[256];
            var length = Win32Api.GetWindowText(hWnd, buffer, buffer.Length);
            return new string(buffer, 0, length);
        }
    }
}
