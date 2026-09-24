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
using Capstone_UI.Models;

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
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Play_Button_Click(object sender, RoutedEventArgs e) //This is the code for the play button. It will navigate to the GameSelection page when clicked.
        {
            MainFrame.Navigate(new GameSelection());
        }

        private void logoExit(object sender, RoutedEventArgs e) //This is the code for the logo button. It will ask the user if they are sure they want to exit the application. If they click yes, the application will close. If they click no, the application will stay open.
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Ranger_Button_Click(object sender, RoutedEventArgs e) //This is the code for the ranger button. It will navigate to the RangerTools page when clicked.
        {
            MainFrame.Navigate(new RangerTools());
        }
    }
}