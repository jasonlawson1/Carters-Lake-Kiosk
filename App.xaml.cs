using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Capstone_UI
{
    /// <summary>
    /// Interaction logic for App.xaml. Provides application-wide event handling, 
    /// global touch focus listeners, and automated scroll-into-view behavior for text inputs.
    /// </summary>
    public partial class App : Application
    {
        // ==========================================
        // Application Startup & Global Event Hooks
        // ==========================================

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register a global class handler to listen for any TextBox gaining focus 
            // across all windows and pages throughout the entire application.
            EventManager.RegisterClassHandler(
                typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(OnTextBoxGotFocus)
            );
        }


        // ==========================================
        // Global Focus & Scrolling Handlers
        // ==========================================

        /// <summary>
        /// Automatically scrolls focused text boxes into view when they receive input focus, 
        /// ensuring they are never obscured by on-screen keyboards or container boundaries.
        /// </summary>
        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                // Traverse up the visual tree to find an ancestor ScrollViewer container
                DependencyObject parent = VisualTreeHelper.GetParent(textBox);
                while (parent != null && !(parent is ScrollViewer))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                // If a parent ScrollViewer is found, bring the active text box cleanly into view
                if (parent is ScrollViewer scrollViewer)
                {
                    textBox.BringIntoView();
                }
            }
        }
    }
}
