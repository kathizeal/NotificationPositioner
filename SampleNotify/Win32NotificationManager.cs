using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using SampleNotify.Models;

namespace SampleNotify
{
    public class Win32NotificationManager
    {
        #region Win32 API Declarations

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const uint SWP_HIDEWINDOW = 0x0080;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        #endregion

        #region Private Fields

        private readonly Dictionary<string, ApplicationInfo> _applications = new();
        private readonly HashSet<IntPtr> _blockedWindows = new();
        private readonly System.Timers.Timer _monitoringTimer;

        #endregion

        #region Events

        public event EventHandler<ApplicationInfo>? ApplicationDiscovered;
        public event EventHandler<(ApplicationInfo app, IntPtr window)>? NotificationBlocked;

        #endregion

        #region Constructor

        public Win32NotificationManager()
        {
            _monitoringTimer = new System.Timers.Timer(500);
            _monitoringTimer.Elapsed += (sender, e) => MonitorNotifications(null);
            _monitoringTimer.Start();
        }

        #endregion

        #region Public Methods

        public ObservableCollection<ApplicationInfo> GetAllApplications()
        {
            RefreshApplicationList();
            return new ObservableCollection<ApplicationInfo>(_applications.Values);
        }

        public void SetNotificationEnabled(string appName, bool enabled)
        {
            if (_applications.TryGetValue(appName, out var app))
            {
                app.NotificationsEnabled = enabled;
                
                // Apply the setting to current notification windows
                foreach (var window in app.NotificationWindows.ToArray())
                {
                    if (enabled)
                    {
                        ShowNotificationWindow(window);
                        _blockedWindows.Remove(window);
                    }
                    else
                    {
                        HideNotificationWindow(window);
                        _blockedWindows.Add(window);
                    }
                }
            }
        }

        public void RefreshApplicationList()
        {
            var discoveredApps = new HashSet<string>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (IsNotificationWindow(hWnd))
                {
                    var appInfo = GetApplicationInfoFromWindow(hWnd);
                    if (appInfo != null && !string.IsNullOrEmpty(appInfo.Name))
                    {
                        discoveredApps.Add(appInfo.Name);
                        
                        if (!_applications.ContainsKey(appInfo.Name))
                        {
                            _applications[appInfo.Name] = appInfo;
                            ApplicationDiscovered?.Invoke(this, appInfo);
                        }
                        
                        var existingApp = _applications[appInfo.Name];
                        if (!existingApp.NotificationWindows.Contains(hWnd))
                        {
                            existingApp.NotificationWindows.Add(hWnd);
                            existingApp.NotificationCount++;
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            // Remove applications that are no longer running
            var appsToRemove = _applications.Keys.Where(name => !discoveredApps.Contains(name)).ToList();
            foreach (var appName in appsToRemove)
            {
                _applications.Remove(appName);
            }
        }

        #endregion

        #region Private Methods

        private void MonitorNotifications(object? state)
        {
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsNotificationWindow(hWnd))
                    {
                        var appInfo = GetApplicationInfoFromWindow(hWnd);
                        if (appInfo != null && !string.IsNullOrEmpty(appInfo.Name))
                        {
                            // Check if this app's notifications should be blocked
                            if (_applications.TryGetValue(appInfo.Name, out var app) && !app.NotificationsEnabled)
                            {
                                if (!_blockedWindows.Contains(hWnd))
                                {
                                    HideNotificationWindow(hWnd);
                                    _blockedWindows.Add(hWnd);
                                    NotificationBlocked?.Invoke(this, (app, hWnd));
                                }
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                // Log error but continue monitoring
                System.Diagnostics.Debug.WriteLine($"Error in notification monitoring: {ex.Message}");
            }
        }

        private bool IsNotificationWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd))
                return false;

            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            var classNameStr = className.ToString();

            // Common notification window classes
            var notificationClasses = new[]
            {
                "Windows.UI.Core.CoreWindow",
                "ToastWin32Window",
                "ToastHost",
                "NotificationHost",
                "Windows.ApplicationModel.Core.CoreApplicationView",
                "Shell_TrayWnd",
                "NotifyIconWnd32"
            };

            if (notificationClasses.Any(cls => classNameStr.Contains(cls, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Check window title for notification keywords
            var title = GetWindowTitle(hWnd);
            var notificationKeywords = new[] { "notification", "toast", "alert", "popup" };
            
            return notificationKeywords.Any(keyword => 
                title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private ApplicationInfo? GetApplicationInfoFromWindow(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                var process = Process.GetProcessById((int)processId);
                
                var appInfo = new ApplicationInfo
                {
                    Name = process.ProcessName,
                    ProcessId = processId,
                    ExecutablePath = GetProcessExecutablePath(processId)
                };

                return appInfo;
            }
            catch
            {
                return null;
            }
        }

        private string GetProcessExecutablePath(uint processId)
        {
            try
            {
                var processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
                if (processHandle != IntPtr.Zero)
                {
                    var path = new StringBuilder(1024);
                    if (GetModuleFileNameEx(processHandle, IntPtr.Zero, path, (uint)path.Capacity) > 0)
                    {
                        CloseHandle(processHandle);
                        return path.ToString();
                    }
                    CloseHandle(processHandle);
                }
            }
            catch { }
            
            return string.Empty;
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            var builder = new StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        private void HideNotificationWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_HIDE);
        }

        private void ShowNotificationWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOW);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _monitoringTimer?.Dispose();
        }

        #endregion
    }
}