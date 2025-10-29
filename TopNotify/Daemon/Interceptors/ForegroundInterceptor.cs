using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class ForegroundInterceptor : Interceptor
    {
        #region WinAPI Methods

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);


        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const byte VK_MENU = 0x12; // Alt key
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const uint GW_OWNER = 4;

        #endregion

        // Track the last notification window and its associated app
        private IntPtr lastNotificationWindow = IntPtr.Zero;
        private string lastNotificationApp = "";
        private bool wasNotificationVisible = false;
        private bool hasTriggeredFocusCheck = false;
        
        // Store app name from UserNotificationListener (proper way)
        private string currentNotificationApp = "";

        public override bool ShouldEnable()
        {
            var shouldEnable = Settings.ForceForegroundOnClick;
            Program.Logger.Information($"ForegroundInterceptor ShouldEnable: {shouldEnable}");
            return shouldEnable;
        }

        public override void Start()
        {
            Program.Logger.Information($"ForegroundInterceptor: Started! Using window-based detection. Enabled={Settings.ForceForegroundOnClick}");
            base.Start();
        }

        public override void OnNotification(UserNotification notification)
        {
            if (!Settings.ForceForegroundOnClick) { return; }

            try
            {
                // Store the app name from the notification (PROPER WAY - only works when packaged correctly)
                currentNotificationApp = notification.AppInfo.DisplayInfo.DisplayName;
                Program.Logger.Information($"ForegroundInterceptor: OnNotification - Got app name from API: {currentNotificationApp}");
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"ForegroundInterceptor: Error in OnNotification: {ex.Message}");
            }

            base.OnNotification(notification);
        }

        public override void Update()
        {
            if (!Settings.ForceForegroundOnClick) { return; }

            base.Update();

            // Get the notification window from NativeInterceptor
            var nativeInterceptor = InterceptorManager.Instance.Interceptors
                .OfType<NativeInterceptor>()
                .FirstOrDefault();

            if (nativeInterceptor == null || nativeInterceptor.hwnd == IntPtr.Zero)
            {
                return;
            }

            var notificationWindow = nativeInterceptor.hwnd;

            // Check if notification window exists and is visible
            bool isVisible = IsWindow(notificationWindow) && IsWindowVisible(notificationWindow);

            // Also check what window currently has focus
            var foregroundWindow = GetForegroundWindow();
            bool notificationHasFocus = (foregroundWindow == notificationWindow);

            // Detect when notification FIRST gets focus (user is interacting with it)
            if (wasNotificationVisible && notificationHasFocus && !hasTriggeredFocusCheck)
            {
                // Notification has focus - user is clicking it
                Program.Logger.Information($"ForegroundInterceptor: Notification window got focus - will check what opens");
                hasTriggeredFocusCheck = true;
                
                // Wait for the click to complete and see what app opens
                Task.Run(async () =>
                {
                    try
                    {
                        // Wait a bit for Windows to process the click
                        Program.Logger.Information($"ForegroundInterceptor: Waiting to detect which app should open...");
                        await Task.Delay(100);
                        
                        // Try to get app name using available methods:
                        // METHOD 1: UserNotificationListener API (works with proper MSIX packaging)
                        // METHOD 2: Heuristic (fallback for development - checks common apps)
                        
                        string targetApp = "";
                        
                        if (!string.IsNullOrEmpty(currentNotificationApp))
                        {
                            // Method 1: Got it from the notification API - most reliable!
                            targetApp = currentNotificationApp;
                            Program.Logger.Information($"ForegroundInterceptor: Using app name from API: {targetApp}");
                        }
                        else
                        {
                            // Method 2: Heuristic fallback for development/loose packaging
                            targetApp = FindMostLikelyAppFromNotification();
                            if (!string.IsNullOrEmpty(targetApp))
                            {
                                Program.Logger.Information($"ForegroundInterceptor: Using app name from heuristic: {targetApp}");
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(targetApp))
                        {
                            // Wait for notification to COMPLETELY finish closing and for Windows to process the click
                            // This is critical - if we try too early, Windows blocks the activation
                            Program.Logger.Information($"ForegroundInterceptor: Waiting for notification to fully close...");
                            await Task.Delay(800);
                            
                            // Now force the app to foreground
                            BringAppToForeground(targetApp);
                            
                            // Clear the stored app name
                            currentNotificationApp = "";
                            
                            // Reset the trigger flag so we can detect the next click
                            await Task.Delay(500);
                            hasTriggeredFocusCheck = false;
                            Program.Logger.Information($"ForegroundInterceptor: Ready for next notification");
                        }
                        else
                        {
                            Program.Logger.Warning($"ForegroundInterceptor: Could not determine which app the notification belongs to");
                            hasTriggeredFocusCheck = false; // Reset even on failure
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error($"ForegroundInterceptor: Error in async check: {ex.Message}");
                    }
                });
            }
            
            // Reset the trigger when notification disappears
            if (!isVisible && wasNotificationVisible)
            {
                hasTriggeredFocusCheck = false;
            }

            // Also detect when notification disappears entirely
            if (wasNotificationVisible && !isVisible && lastNotificationWindow == notificationWindow)
            {
                Program.Logger.Information($"ForegroundInterceptor: Notification window disappeared - trying to bring app to foreground");
                
                // Try to determine which app the notification was from
                string appName = GetAppNameFromForeground();
                
                if (!string.IsNullOrEmpty(appName) && appName != "explorer")
                {
                    Program.Logger.Information($"ForegroundInterceptor: Detected click via disappearance, bringing {appName} to foreground");
                    BringAppToForeground(appName);
                }
                else
                {
                    Program.Logger.Information($"ForegroundInterceptor: Could not determine app name from context");
                }
            }

            // Update tracking state
            if (isVisible && !wasNotificationVisible)
            {
                Program.Logger.Information($"ForegroundInterceptor: New notification window appeared");
            }
            
            wasNotificationVisible = isVisible;
            lastNotificationWindow = notificationWindow;
        }

        /// <summary>
        /// Tries to guess the app name by looking at what window got focus after the notification was clicked
        /// </summary>
        private string GetAppNameFromForeground()
        {
            var foregroundWindow = GetForegroundWindow();
            return GetAppNameFromWindow(foregroundWindow);
        }

        /// <summary>
        /// Gets the process name for a given window handle
        /// </summary>
        private string GetAppNameFromWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero) return "";

                GetWindowThreadProcessId(windowHandle, out uint processId);
                var process = Process.GetProcessById((int)processId);
                
                Program.Logger.Information($"ForegroundInterceptor: Window {windowHandle} belongs to process: {process.ProcessName}");
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"ForegroundInterceptor: Error getting process name: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Heuristic fallback: Checks for common notification apps running in background
        /// Used when UserNotificationListener API is unavailable (development/loose packaging)
        /// </summary>
        private string FindMostLikelyAppFromNotification()
        {
            try
            {
                // List of common apps that send notifications
                // This is a development fallback - in production with proper MSIX, the API provides the app name
                string[] commonApps = { 
                    "Slack", "Discord", "Teams", "msteams", "Spotify", 
                    "Telegram", "WhatsApp", "Signal", "Outlook", "OUTLOOK",
                    "chrome", "msedge", "firefox", "Thunderbird"
                };
                
                var foregroundWindow = GetForegroundWindow();
                var processes = Process.GetProcesses();
                
                foreach (var checkAppName in commonApps)
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.ProcessName.Equals(checkAppName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Check if this app has a window but it's not in foreground
                                if (process.MainWindowHandle != IntPtr.Zero && 
                                    process.MainWindowHandle != foregroundWindow)
                                {
                                    Program.Logger.Information($"ForegroundInterceptor: Found likely app: {process.ProcessName}");
                                    return process.ProcessName;
                                }
                            }
                        }
                        catch { continue; }
                    }
                }
                
                Program.Logger.Warning($"ForegroundInterceptor: Could not determine app - no common apps found in background");
                return "";
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"ForegroundInterceptor: Error finding likely app: {ex.Message}");
                return "";
            }
        }


        private void BringAppToForeground(string appName)
        {
            try
            {
                // First, try to find the process by app name
                var processes = Process.GetProcesses();
                IntPtr targetWindow = IntPtr.Zero;
                uint targetProcessId = 0;

                foreach (var process in processes)
                {
                    try
                    {
                        var processName = process.ProcessName;
                        
                        // First check if process name matches what we're looking for
                        bool nameMatches = processName.Contains(appName, StringComparison.OrdinalIgnoreCase) ||
                                          appName.Contains(processName, StringComparison.OrdinalIgnoreCase);
                        
                        if (!nameMatches)
                        {
                            // Not the app we're looking for, skip
                            continue;
                        }

                        Program.Logger.Information($"ForegroundInterceptor: Found matching process: {processName} (looking for {appName})");

                        // Check if it has a main window
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            var windowTitle = process.MainWindowTitle;
                            
                            // Verify this window is actually visible and has size
                            GetWindowRect(process.MainWindowHandle, out RECT rect);
                            int width = rect.Right - rect.Left;
                            int height = rect.Bottom - rect.Top;
                            bool hasSize = width > 100 && height > 100; // Reasonable window size
                            
                            Program.Logger.Information($"ForegroundInterceptor: MainWindowHandle {process.MainWindowHandle}: '{windowTitle}', Size: {width}x{height}, Pos: ({rect.Left},{rect.Top}), Visible: {IsWindowVisible(process.MainWindowHandle)}");
                            
                            if (hasSize && IsWindowVisible(process.MainWindowHandle))
                            {
                                targetWindow = process.MainWindowHandle;
                                targetProcessId = (uint)process.Id;
                                Program.Logger.Information($"ForegroundInterceptor: ✓ Using MainWindowHandle for {processName}");
                                break;
                            }
                            else
                            {
                                Program.Logger.Information($"ForegroundInterceptor: MainWindowHandle is too small or invisible, checking other windows...");
                            }
                        }
                        else
                        {
                            Program.Logger.Information($"ForegroundInterceptor: Process {processName} has no MainWindowHandle, checking all windows...");
                        }

                        // For apps like Slack, Discord, etc., they might have different process names
                        // Try to find any window belonging to the process
                        var windows = GetProcessWindows(process.Id);
                        if (windows.Any())
                        {
                            Program.Logger.Information($"ForegroundInterceptor: Process {processName} (matching {appName}) has {windows.Count} windows");
                            
                            // Look for the MAIN window with a title
                            IntPtr bestCandidate = IntPtr.Zero;
                            foreach (var window in windows)
                            {
                                var isVisible = IsWindowVisible(window);
                                var hasOwner = GetWindow(window, GW_OWNER) != IntPtr.Zero;
                                
                                // Get window title
                                int length = GetWindowTextLength(window);
                                string windowTitle = "";
                                if (length > 0)
                                {
                                    StringBuilder sb = new StringBuilder(length + 1);
                                    GetWindowText(window, sb, sb.Capacity);
                                    windowTitle = sb.ToString();
                                }
                                
                                Program.Logger.Information($"ForegroundInterceptor: Window {window} - Title: '{windowTitle}', Visible: {isVisible}, HasOwner: {hasOwner}");
                                
                                // Prefer windows with titles that look like main windows
                                if (isVisible && !hasOwner)
                                {
                                    if (!string.IsNullOrWhiteSpace(windowTitle) && 
                                        (windowTitle.Contains(appName, StringComparison.OrdinalIgnoreCase) ||
                                         windowTitle.Contains(processName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        // This looks like the main window!
                                        targetWindow = window;
                                        targetProcessId = (uint)process.Id;
                                        Program.Logger.Information($"ForegroundInterceptor: Selected MAIN window {window} ('{windowTitle}') for {appName}");
                                        break;
                                    }
                                    else if (bestCandidate == IntPtr.Zero)
                                    {
                                        // Keep as backup candidate
                                        bestCandidate = window;
                                    }
                                }
                            }
                            
                            // Use best candidate if we didn't find a clear main window
                            if (targetWindow == IntPtr.Zero && bestCandidate != IntPtr.Zero)
                            {
                                targetWindow = bestCandidate;
                                targetProcessId = (uint)process.Id;
                                Program.Logger.Information($"ForegroundInterceptor: Using backup candidate window {bestCandidate} for {appName}");
                            }
                            
                            if (targetWindow != IntPtr.Zero) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Process might have exited, continue
                        continue;
                    }
                }

                // If we found a window, bring it to foreground using advanced techniques
                if (targetWindow != IntPtr.Zero)
                {
                    Program.Logger.Information($"ForegroundInterceptor: Attempting to force {appName} to foreground with advanced techniques");
                    ForceForeground(targetWindow, targetProcessId);
                }
                else
                {
                    Program.Logger.Warning($"ForegroundInterceptor: Could not find window for {appName}");
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"ForegroundInterceptor BringAppToForeground error: {ex.Message}");
            }
        }

        /// <summary>
        /// Brings a window to the foreground
        /// </summary>
        private async void ForceForeground(IntPtr hWnd, uint processId)
        {
            try
            {
                Program.Logger.Information($"ForegroundInterceptor: Bringing window {hWnd} to foreground");

                // Restore if minimized
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    Thread.Sleep(50);
                }

                // Simulate Alt key to unlock foreground permissions (Windows security requirement)
                keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
                Thread.Sleep(10);
                keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Thread.Sleep(30);

                // Show the window
                ShowWindow(hWnd, SW_SHOW);

                // Set as foreground
                bool success = SetForegroundWindow(hWnd);
                Program.Logger.Information($"ForegroundInterceptor: SetForegroundWindow result: {success}");

                // Verify
                await Task.Delay(50);
                var checkForeground = GetForegroundWindow();
                bool isNowForeground = (checkForeground == hWnd);
                Program.Logger.Information($"ForegroundInterceptor: Window is now foreground: {isNowForeground}");
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"ForegroundInterceptor: Error in ForceForeground: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all window handles for a given process ID
        /// </summary>
        private List<IntPtr> GetProcessWindows(int processId)
        {
            var windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                if (windowProcessId == processId)
                {
                    windows.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }


        public override void Restart()
        {
            // Reset state when settings change
            wasNotificationVisible = false;
            lastNotificationWindow = IntPtr.Zero;
            lastNotificationApp = "";
            base.Restart();
        }
    }
}

