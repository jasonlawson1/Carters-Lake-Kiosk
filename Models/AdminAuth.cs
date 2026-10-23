using System;
using System.Windows;
using Microsoft.VisualBasic;

namespace Capstone_UI.Models
{ 
    internal class AdminAuth
    {

        public static bool VerifyAdminPassword() // Method to verify the admin password
        {
            string inputPassword = Interaction.InputBox("Enter the admin password: ", "Admin Verification", "", -1, -1);

            if (inputPassword == "RangerPassword123")
            {
                return true;
            }

            if (!string.IsNullOrEmpty(inputPassword))
            {
                MessageBox.Show("Incorrect password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }


        public static void RequestAppExit() // Method to request application exit with admin verification
        {
            if (!VerifyAdminPassword())
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}