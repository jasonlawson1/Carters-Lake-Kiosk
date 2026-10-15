using System;
using System.Diagnostics;
using System.Windows;

namespace Capstone_UI
{
    public static class KeyboardHelper
    {
        public static void OpenTouchKeyboard()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "osk.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error launching keyboard: {ex.Message}");
            }
        }

        public static void CloseTouchKeyboard()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("osk"))
                {
                    process.Kill();
                    process.WaitForExit(500);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing keyboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Universal prompt method for all text inputs. Opens the touch keyboard, 
        /// centers the input box horizontally, and places it safely near the top.
        /// </summary>
        public static string PromptWithKeyboard(string prompt, string title, string defaultResponse = "")
        {
            try
            {
                OpenTouchKeyboard();

                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int xPos = (screenWidth / 2) - 200;
                int yPos = 120; // Keeps the input dialog box clear of the top edge and keyboard

                return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultResponse, xPos, yPos);
            }
            finally
            {
                CloseTouchKeyboard();
            }
        }
    }
}