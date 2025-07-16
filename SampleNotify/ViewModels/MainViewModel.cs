using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.UI.Xaml.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SampleNotify.Models;

namespace SampleNotify.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly Win32NotificationManager _notificationManager;

        public MainViewModel()
        {
            _notificationManager = new Win32NotificationManager();
            Applications = new ObservableCollection<ApplicationInfo>();
            
            // Subscribe to events
            _notificationManager.ApplicationDiscovered += OnApplicationDiscovered;
            _notificationManager.NotificationBlocked += OnNotificationBlocked;
            
            // Load initial applications
            RefreshApplications();
        }

        [ObservableProperty]
        private ObservableCollection<ApplicationInfo> applications = new();

        [ObservableProperty]
        private bool isMonitoring = false;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [RelayCommand]
        private void RefreshApplications()
        {
            try
            {
                StatusMessage = "Refreshing applications...";
                _notificationManager.RefreshApplicationList();
                
                Applications.Clear();
                var apps = _notificationManager.GetAllApplications();
                foreach (var app in apps)
                {
                    Applications.Add(app);
                }
                
                StatusMessage = $"Found {Applications.Count} applications with notifications";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ToggleNotifications(ApplicationInfo app)
        {
            try
            {
                app.NotificationsEnabled = !app.NotificationsEnabled;
                _notificationManager.SetNotificationEnabled(app.Name, app.NotificationsEnabled);
                
                StatusMessage = app.NotificationsEnabled 
                    ? $"Enabled notifications for {app.Name}"
                    : $"Disabled notifications for {app.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error toggling notifications: {ex.Message}";
            }
        }

        [RelayCommand]
        private void StartMonitoring()
        {
            IsMonitoring = true;
            StatusMessage = "Monitoring started - notifications will be controlled automatically";
        }

        [RelayCommand]
        private void StopMonitoring()
        {
            IsMonitoring = false;
            StatusMessage = "Monitoring stopped";
        }

        [RelayCommand]
        private void EnableAllNotifications()
        {
            try
            {
                foreach (var app in Applications)
                {
                    app.NotificationsEnabled = true;
                    _notificationManager.SetNotificationEnabled(app.Name, true);
                }
                StatusMessage = "Enabled notifications for all applications";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error enabling all notifications: {ex.Message}";
            }
        }

        [RelayCommand]
        private void DisableAllNotifications()
        {
            try
            {
                foreach (var app in Applications)
                {
                    app.NotificationsEnabled = false;
                    _notificationManager.SetNotificationEnabled(app.Name, false);
                }
                StatusMessage = "Disabled notifications for all applications";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error disabling all notifications: {ex.Message}";
            }
        }

        private void OnApplicationDiscovered(object? sender, ApplicationInfo app)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                if (!Applications.Any(a => a.Name == app.Name))
                {
                    Applications.Add(app);
                    StatusMessage = $"Discovered new application: {app.Name}";
                }
            });
        }

        private void OnNotificationBlocked(object? sender, (ApplicationInfo app, IntPtr window) args)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                StatusMessage = $"Blocked notification from {args.app.Name}";
            });
        }

        public void Dispose()
        {
            _notificationManager?.Dispose();
        }
    }
}