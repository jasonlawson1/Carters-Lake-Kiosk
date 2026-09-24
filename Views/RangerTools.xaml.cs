using System;
using System.Collections.Generic;
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
    public partial class RangerTools : Page
    {
        public RangerTools()
        {
            InitializeComponent(); //Connect frontend to back end. Leave on top of code. DO NOT DELETE!
        }

        private void rangerToolsBackButton_Click_1(object sender, RoutedEventArgs e) //Back button to return to the main menu
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }

        private void logoExitRangerTools(object sender, RoutedEventArgs e) //Exit button to close the application via the castle logo.
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
