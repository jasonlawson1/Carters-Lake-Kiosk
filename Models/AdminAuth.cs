using System;
using System.Windows;
using Microsoft.VisualBasic;

namespace Capstone_UI.Models
{
    /// <summary>
    /// Handles administrative authentication and secure application exit procedures.
    /// </summary>
    internal class AdminAuth
    {
        /// <summary>
        /// Prompts the user for the administrator password using an input dialog and verifies it.
        /// </summary>
        /// <returns>True if the entered password matches the admin credentials; otherwise, false.</returns>
        public static bool VerifyAdminPassword()
        {
            // Prompt the user to enter the admin password via a Visual Basic input dialog box
            string inputPassword = Interaction.InputBox("Enter the admin password: ", "Admin Verification", "", -1, -1);

            // Return true if the password matches the authorized string
            if (inputPassword == "RangerPassword123")
            {
                return true;
            }

            // If the user entered text (didn't just click Cancel on an empty string) and it was wrong, notify them
            if (!string.IsNullOrEmpty(inputPassword))
            {
                MessageBox.Show("Incorrect password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Initiates the shutdown sequence by first requiring admin authentication 
        /// and prompting for user confirmation.
        /// </summary>
        public static void RequestAppExit()
        {
            // Step 1: Enforce admin verification; cancel exit if authentication fails
            if (!VerifyAdminPassword())
            {
                return;
            }

            // Step 2: Display a confirmation dialog box asking if they truly want to quit
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            // Step 3: Shutdown the entire application if the user clicks 'Yes'
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}