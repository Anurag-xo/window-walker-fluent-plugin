using System;
using System.Collections.Generic;
using System.ComponentModel; // For Win32Exception
using System.Diagnostics;
using System.Runtime.InteropServices; // For Marshal
using Microsoft.Extensions.Logging; // Add Logging

namespace WindowWalker.Fluent.Plugin
{
    public static class WindowEnumerator
    {
        // Modify method signature to accept ILogger
        public static List<WindowInfo> GetOpenWindows(ILogger logger)
        {
            var windows = new List<WindowInfo>();
            Win32Api.EnumWindows((hWnd, lParam) =>
            {
                if (Win32Api.IsWindowVisible(hWnd))
                {
                    try
                    {
                        int pid;
                        Win32Api.GetWindowThreadProcessId(hWnd, out pid);

                        Process process;
                        try
                        {
                            process = Process.GetProcessById(pid);
                        }
                        catch (ArgumentException)
                        {
                            // Process with ID {pid} not found (might have closed)
                            logger.LogTrace("Process with ID {Pid} not found during enumeration.", pid);
                            return true; // Continue enumeration
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Process is not running (e.g., access denied, zombie)
                            logger.LogDebug(ex, "Could not access process with ID {Pid}.", pid);
                            return true; // Continue enumeration
                        }
                        catch (Exception ex) // Catch other potential exceptions from GetProcessById
                        {
                            logger.LogWarning(ex, "Unexpected error getting process with ID {Pid}.", pid);
                            return true; // Continue enumeration
                        }

                        var title = GetWindowText(hWnd, logger); // Pass logger
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
                    catch (Exception ex) // Catch unexpected errors in the enumeration callback
                    {
                        logger.LogError(ex, "Unexpected error processing window handle {Handle}.", hWnd);
                        // Decide: Log and continue, or potentially re-throw if critical.
                        // For a plugin, usually better to continue enumeration.
                    }
                }
                return true;
            }, IntPtr.Zero);
            logger.LogDebug("Enumerated {Count} open windows.", windows.Count);
            return windows;
        }

        // Modify GetWindowText to handle variable length titles
        private static string GetWindowText(IntPtr hWnd, ILogger logger)
        {
            try
            {
                // First, get the required length
                int length = Win32Api.GetWindowTextLength(hWnd);
                if (length == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0)
                    {
                        logger.LogTrace("GetWindowTextLength failed for handle {Handle} with error {Error}.", hWnd, error);
                    }
                    return string.Empty; // No title or error
                }

                // Allocate buffer (+1 for null terminator)
                char[] buffer = new char[length + 1];
                int resultLength = Win32Api.GetWindowText(hWnd, buffer, buffer.Length);

                // resultLength should ideally match length, but check
                if (resultLength > 0)
                {
                    return new string(buffer, 0, resultLength);
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    logger.LogTrace("GetWindowText failed for handle {Handle} with error {Error}.", hWnd, error);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while retrieving window text for handle {Handle}.", hWnd);
                return string.Empty; // Return empty on error
            }
        }
    }
}
