using Capstone_UI.Models;
using Capstone_UI.Services;
using Microsoft.VisualBasic;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Capstone_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); //Must remain on top. Parses the XAML UI layout and creates visual control.
        }


        // The "play" button on the main page. When selected moves to GameSelection
        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GameSelection());
        }

        // The UI adjustment for each page.
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Checks that redBrush is using ExitRed color in MainWindow. If color is null, it changes the color to another shade of red
            SolidColorBrush redBrush = (TryFindResource("ExitRed") as SolidColorBrush)
                ?? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6192E"));

            if (MainFrame.CanGoBack) // If user can go back to previous page, footer changes button to "Back" function with redBrush
            {
                FooterActionButton.Content = "⬅️ Back";
                FooterActionButton.Background = redBrush;
            }
            else // If user cannot go back, footer shows "Exit Kiosk" function with redBrush
            {
                FooterActionButton.Content = "🛑 Exit Kiosk";
                FooterActionButton.Background = redBrush;
            }

            if (e.Content is RangerTools) //If user is in RangerTools page, hide the RangerButton. Otherwise, show it.
            {
                RangerButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RangerButton.Visibility = Visibility.Visible;
            }
        }


        // Footer button function. If user can go back, it goes back to previous page. If not, it exits the application.
        private void FooterActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            else
            {
                menuExitButton(sender, e);
            }
        }

        // Main menu button function. Navigates back to the main menu.
        private void MainMenu_Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainWindow());
        }

        // Exit button function. Opens the keyboard, Requests password to exit, and closes the keyboard
        private void menuExitButton(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyboardHelper.OpenTouchKeyboard();
                AdminAuth.RequestAppExit();
            }
            finally
            {
                KeyboardHelper.CloseTouchKeyboard();
            }
        }

        // Exit button function. Opens the keyboard, Requests password to exit, and closes the keyboard
        private void logoExit(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyboardHelper.OpenTouchKeyboard();
                AdminAuth.RequestAppExit();
            }
            finally
            {
                KeyboardHelper.CloseTouchKeyboard();
            }
        }

        // When Ranger Tools is selected, opens keyboard, requests password, and closes keyboard. If password is correct, navigates to the ranger tools. If password is incorrect, closes window and returns to main menu.
        private void Ranger_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isAuthenticated = false;

            try
            {
                KeyboardHelper.OpenTouchKeyboard();
                isAuthenticated = AdminAuth.VerifyAdminPassword();
            }
            finally
            {
                KeyboardHelper.CloseTouchKeyboard();
            }

            if (!isAuthenticated)
            {
                return;
            }

            MainFrame.Navigate(new RangerTools());
        }
    }
}