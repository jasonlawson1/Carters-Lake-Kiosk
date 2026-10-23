using System.IO; // Needed for Path.Combine
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Capstone_UI.Services; // <-- Add this namespace

namespace Capstone_UI
{
    public partial class App : Application
    {
        // Global database instance
        public static DatabaseService Db { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database service pointing to a local file in the app directory
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kiosk.db");
            Db = new DatabaseService(dbPath);

            // Register global focus listeners
            EventManager.RegisterClassHandler(
                typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(OnTextBoxGotFocus)
            );
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e) // Event handler to bring the focused TextBox into view
        {
            if (e.OriginalSource is TextBox textBox) // Check if the original source of the event is a TextBox
            {
                DependencyObject parent = VisualTreeHelper.GetParent(textBox); // Traverse up the visual tree to find the nearest ScrollViewer
                while (parent != null && !(parent is ScrollViewer)) // Continue traversing until a ScrollViewer is found or the root is reached
                {
                    parent = VisualTreeHelper.GetParent(parent); // Move up the visual tree
                }

                if (parent is ScrollViewer scrollViewer) // If a ScrollViewer is found, bring the TextBox into view
                {
                    textBox.BringIntoView(); // This will scroll the ScrollViewer to make the TextBox visible
                }
            }
        }
    }
}