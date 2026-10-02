using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Capstone_UI.Models
{
    internal class AdminAuth
    {
        public static bool VerifyAdminPassword()
        {
            string inputPassword = Interaction.InputBox("Enter the admin password: ", "Admin Verification", "", -1, -1);

            if (inputPassword == "RangerPassword123")
            {
                return true;
            }

            if (!string.IsNullOrEmpty(inputPassword)) // Check if the user entered something
            {
                MessageBox.Show("Incorrect password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        public static void RequestAppExit()
        {
            if(!VerifyAdminPassword())
            {
                return; // Exit the method if the password is incorrect
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
