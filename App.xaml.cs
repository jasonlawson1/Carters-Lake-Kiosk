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

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(textBox);
                while (parent != null && !(parent is ScrollViewer))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is ScrollViewer scrollViewer)
                {
                    textBox.BringIntoView();
                }
            }
        }
    }
}