using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Capstone_UI
{
    public static class KeyboardHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static void OpenTouchKeyboard()
        {
            try
            {
                // Reliably open standard Windows On-Screen Keyboard
                Process.Start(new ProcessStartInfo
                {
                    FileName = "osk.exe",
                    UseShellExecute = true
                });

                // Move OSK to the bottom of the primary screen
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                int keyboardHeight = 350; // Standard OSK height
                int keyboardY = screenHeight - keyboardHeight; // Lock to bottom

                IntPtr oskHandle = FindWindow("OSKMainClass", null);
                if (oskHandle != IntPtr.Zero)
                {
                    SetWindowPos(oskHandle, IntPtr.Zero, 0, keyboardY, screenWidth, keyboardHeight, SWP_SHOWWINDOW);
                }
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
                    process.WaitForExit(300);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing keyboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Prompts the user with a text box safely anchored at 30% from the top 
        /// while the OSK stays at the bottom.
        /// </summary>
        public static string PromptWithKeyboard(string prompt, string title, string defaultResponse = "")
        {
            try
            {
                OpenTouchKeyboard();

                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // Center horizontally, place at 30% down from the top (upper-middle)
                int xPos = (int)((screenWidth / 2) - 200);
                int yPos = (int)(screenHeight * 0.3);

                return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultResponse, xPos, yPos);
            }
            finally
            {
                CloseTouchKeyboard();
            }
        }
    }
}