using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
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
        public MainWindow()
        {
            this.InitializeComponent();


            var positioner = new StandaloneNotificationPositioner();
            positioner.Position = NotificationPosition.TopRight;
            positioner.StartPositioning();

            Console.WriteLine("Notification positioning applied!");
        
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

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            ShowSampleToast("Sample Toast", "This is a sample toast notification.");
        }
    }

}
