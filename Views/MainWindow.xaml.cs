using Capstone_UI.Models;
using Capstone_UI.Services;
using Microsoft.VisualBasic;
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
            InitializeComponent(); //Connects the front end to the back end. It must stay at the top of the code. DO NOT DELETE!
        }

        private void menuExitButton(object sender, RoutedEventArgs e) //This is the code for the exit button in the menu bar. It will ask the user if they are sure they want to exit the application. If they click yes, the application will close. If they click no, the application will stay open.
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

        private void Play_Button_Click(object sender, RoutedEventArgs e) //This is the code for the play button. It will navigate to the GameSelection page when clicked.
        {
            MainFrame.Navigate(new GameSelection());
        }

        private void MainMenu_Button_Click(object sender, RoutedEventArgs e) // This is the code to return to the Main Menu view when clicked.
        {
            // Replace 'MainMenuView' with your actual Main Menu page class name if it differs
            MainFrame.Navigate(new MainWindow());
        }

        private void logoExit(object sender, RoutedEventArgs e) //This is the code for the logo button. It will ask the user if they are sure they want to exit the application. If they click yes, the application will close. If they click no, the application will stay open.
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

        private void Ranger_Button_Click(object sender, RoutedEventArgs e) //This is the code for the ranger button. It will navigate to the RangerTools page when clicked.
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