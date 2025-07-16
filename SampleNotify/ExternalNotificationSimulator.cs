using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;

namespace SampleNotify
{
    /// <summary>
    /// Simulates external applications sending notifications for testing purposes
    /// </summary>
    public class ExternalNotificationSimulator
    {
        public static void SimulateExternalApp()
        {
            Console.WriteLine("=== External Notification Simulator ===");
            Console.WriteLine("Simulating notifications from external applications...");
            
            // Simulate different types of external notifications
            Task.Run(async () =>
            {
                await Task.Delay(3000); // Wait 3 seconds for app to load
                SendNotification("Weather App", "Sunny weather today! 25°C");
                
                await Task.Delay(4000); // Wait 4 more seconds
                SendNotification("Email Client", "You have 3 new emails");
                
                await Task.Delay(3000); // Wait 3 more seconds
                SendNotification("Security Scanner", "System scan completed - No threats found");
                
                await Task.Delay(5000); // Wait 5 more seconds
                SendNotification("Calendar App", "Meeting in 10 minutes: Team Standup");
                
                await Task.Delay(4000); // Wait 4 more seconds
                SendNotification("Chat Application", "New message from John: How's the project going?");
                
                await Task.Delay(3000); // Wait 3 more seconds
                SendNotification("Update Service", "System updates available for download");
                
                Console.WriteLine("External notification simulation completed.");
                Console.WriteLine("You can now use the UI to enable/disable notifications for each application.");
            });
        }
        
        private static void SendNotification(string title, string message)
        {
            try
            {
                Console.WriteLine($"External App Sending: {title} - {message}");
                
                string toastXmlString = $@"<toast>
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Simulates notifications from a completely different process (more realistic)
        /// </summary>
        public static void SimulateFromDifferentProcess()
        {
            // This would require launching a separate executable
            // For now, we'll use PowerShell to send Windows notifications
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = @"-Command ""[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('SampleNotify').Show([Windows.UI.Notifications.ToastNotification]::new([Windows.Data.Xml.Dom.XmlDocument]::new())); Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.MessageBox]::Show('Test notification from external process', 'External App')""",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating external process: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts continuous notification simulation for testing
        /// </summary>
        public static void StartContinuousSimulation()
        {
            Task.Run(async () =>
            {
                var apps = new[]
                {
                    ("Weather App", new[] { "Sunny day ahead!", "Rain expected this afternoon", "Temperature dropping to 15°C" }),
                    ("Email Client", new[] { "New email from boss", "Newsletter arrived", "Meeting invitation received" }),
                    ("Chat App", new[] { "New message from Alice", "Group chat update", "Video call request" }),
                    ("News App", new[] { "Breaking news update", "Sports scores available", "Tech news digest" }),
                    ("Security App", new[] { "Scan completed", "Threat detected", "System update required" })
                };

                while (true)
                {
                    await Task.Delay(8000); // Wait 8 seconds between notifications
                    
                    var random = new Random();
                    var selectedApp = apps[random.Next(apps.Length)];
                    var selectedMessage = selectedApp.Item2[random.Next(selectedApp.Item2.Length)];
                    
                    SendNotification(selectedApp.Item1, selectedMessage);
                }
            });
        }
    }
}