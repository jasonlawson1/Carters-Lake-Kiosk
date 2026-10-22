using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Capstone_UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Listen globally for any TextBox gaining focus across the entire app
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
                // Find if the text box is inside a ScrollViewer and scroll it into view
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
