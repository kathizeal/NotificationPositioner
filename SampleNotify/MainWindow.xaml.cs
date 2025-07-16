using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using SampleNotify.ViewModels;
using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using static SampleNotify.StandaloneNotificationPositioner;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SampleNotify
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();

            // Set the DataContext for binding through the Content property
            if (this.Content is FrameworkElement content)
            {
                content.DataContext = ViewModel;
            }

            // Initialize notification positioning if needed
            var positioner = new StandaloneNotificationPositioner();
            positioner.Position = NotificationPosition.TopRight;
            positioner.StartPositioning();

            Console.WriteLine("Notification Manager started!");
            Console.WriteLine("Instructions:");
            Console.WriteLine("1. Click 'Test Notification' to send sample notifications");
            Console.WriteLine("2. Use 'Refresh Apps' to scan for applications with notifications");
            Console.WriteLine("3. Toggle the switches to enable/disable notifications per app");
            Console.WriteLine("4. Use 'Enable All' or 'Disable All' for bulk operations");

            // Start the external notification simulator for testing
            ExternalNotificationSimulator.SimulateExternalApp();

            // Subscribe to the window closed event
            this.Closed += MainWindow_Closed;
        }

        private void TestNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSampleToast("Test Notification", $"This is a test notification sent at {DateTime.Now:HH:mm:ss}");
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            ViewModel?.Dispose();
        }

        private void ShowSampleToast(string title, string message)
        {
            // Create the toast content as XML string
            string toastXmlString =
       $@"<toast>
            <visual>
                <binding template='ToastGeneric'>
                    <text>{title}</text>
                    <text>{message}</text>
                </binding>
            </visual>
        </toast>";

            var notification = new AppNotification(toastXmlString);
            AppNotificationManager.Default.Show(notification);
        }
    }
}
