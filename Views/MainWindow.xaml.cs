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
    /// <summary>
    /// Interaction logic for MainWindow.xaml. Acts as the main shell and container 
    /// for kiosk navigation, frame management, and administrative entry points.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ==========================================
        // Constructor & Lifecycle
        // ==========================================

        public MainWindow()
        {
            // Connects the XAML front-end layout to the code-behind. 
            // CRITICAL: Must stay at the top of the constructor and never be deleted!
            InitializeComponent();
        }


        // ==========================================
        // Navigation Event Handlers
        // ==========================================

        /// <summary>
        /// Handles the play button click, navigating the user to the GameSelection page library.
        /// </summary>
        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GameSelection());
        }

        /// <summary>
        /// Automatically updates the adaptive footer action button (Switching between Exit Kiosk and Back)
        /// and hides/shows the Staff Tools button depending on the current page.
        /// </summary>
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SolidColorBrush redBrush = (TryFindResource("ExitRed") as SolidColorBrush)
                ?? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6192E"));

            if (MainFrame.CanGoBack)
            {
                FooterActionButton.Content = "⬅️ Back";
                FooterActionButton.Background = redBrush;
            }
            else
            {
                FooterActionButton.Content = "🛑 Exit Kiosk";
                FooterActionButton.Background = redBrush;
            }

            // Hide the Staff Tools button when viewing Ranger Tools, show it everywhere else
            if (e.Content is RangerTools)
            {
                RangerButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RangerButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the adaptive footer action button click: goes back if on a subpage, 
        /// or triggers the exit sequence if on the main menu.
        /// </summary>
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

        /// <summary>
        /// Resets or returns the main view frame back to the primary menu view.
        /// </summary>
        private void MainMenu_Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainWindow());
        }


        // ==========================================
        // Administrative & Exit Actions
        // ==========================================

        /// <summary>
        /// Triggers the secure application exit sequence from the top menu bar button 
        /// with touch keyboard support and admin verification.
        /// </summary>
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

        /// <summary>
        /// Triggers the secure application exit sequence from the kiosk logo button 
        /// with touch keyboard support and admin verification across all screens.
        /// </summary>
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

        /// <summary>
        /// Prompts for administrative password verification before allowing entry 
        /// into the protected RangerTools management portal.
        /// </summary>
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