using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SampleNotify.Models
{
    public class ApplicationInfo : INotifyPropertyChanged
    {
        private bool _notificationsEnabled = true;
        private int _notificationCount = 0;

        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public uint ProcessId { get; set; }
        public List<IntPtr> NotificationWindows { get; set; } = new List<IntPtr>();
        
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (_notificationsEnabled != value)
                {
                    _notificationsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotificationsEnabled)));
                }
            }
        }

        public int NotificationCount
        {
            get => _notificationCount;
            set
            {
                if (_notificationCount != value)
                {
                    _notificationCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotificationCount)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}