using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualBasic;

namespace Capstone_UI.Models
{
    internal class AdminAuth
    {
        // Win32 imports to locate and reposition standard MessageBox windows
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Helper method to pop up standard MessageBoxes higher
        private static MessageBoxResult ShowMessageBoxAtTop(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            // Background task that waits for the message box window to spawn, then moves it to Top = 120
            System.Threading.Tasks.Task.Run(async () =>
            {
                IntPtr hWnd = IntPtr.Zero;
                for (int i = 0; i < 20; i++)
                {
                    hWnd = FindWindow("#32770", title);
                    if (hWnd != IntPtr.Zero) break;
                    await System.Threading.Tasks.Task.Delay(10);
                }

                if (hWnd != IntPtr.Zero && GetWindowRect(hWnd, out RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                    int newX = (screenWidth - width) / 2;
                    int newY = 120; // Pushes it cleanly above the touch keyboard

                    MoveWindow(hWnd, newX, newY, width, height, true);
                }
            });

            return MessageBox.Show(message, title, button, icon);
        }

        public static bool VerifyAdminPassword() // Method to verify the admin password
        {
            // Calculate center X coordinate, and set Y coordinate to 120 instead of -1
            int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            int posX = (screenWidth - 400) / 2; // Approximate InputBox width offset
            int posY = 120;

            string inputPassword = Interaction.InputBox("Enter the admin password: ", "Admin Verification", "", posX, posY);

            if (inputPassword == "RangerPassword123")
            {
                return true;
            }

            if (!string.IsNullOrEmpty(inputPassword))
            {
                ShowMessageBoxAtTop("Incorrect password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        public static void RequestAppExit() // Method to request application exit with admin verification
        {
            if (!VerifyAdminPassword())
            {
                return;
            }

            MessageBoxResult result = ShowMessageBoxAtTop(
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