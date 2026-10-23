using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Capstone_UI
{

    public static class KeyboardHelper
    {


        [DllImport("user32.dll", SetLastError = true)] // Import the SetWindowPos function from user32.dll to set the position of a window
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); // Import the FindWindow function from user32.dll to find a window by its class name and window name

        [DllImport("user32.dll", SetLastError = true)] // Import the FindWindow function from user32.dll to find a window by its class name and window name
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const uint SWP_NOSIZE = 0x0001; // Constant to indicate that the window size should not be changed
        private const uint SWP_NOZORDER = 0x0004; // Constant to indicate that the window's Z order should not be changed
        private const uint SWP_SHOWWINDOW = 0x0040; // Constant to indicate that the window should be shown


        public static void OpenTouchKeyboard()
        {
            try
            {
                CloseTouchKeyboard();

                string oskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "osk.exe"); // Default path for the on-screen keyboard executable

                if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) // Check if the OS is 64-bit and the process is 32-bit
                {
                    string sysnativePath = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName, "Sysnative", "osk.exe"); // Path to the Sysnative folder for 32-bit processes on 64-bit Windows
                    if (File.Exists(sysnativePath)) // Check if the Sysnative path exists
                    {
                        oskPath = sysnativePath; // Use the Sysnative path if it exists
                    }
                }

                Process.Start(new ProcessStartInfo // Start the on-screen keyboard process
                {
                    FileName = oskPath, // Set the file name to the path of the on-screen keyboard executable
                    UseShellExecute = true
                });

                System.Threading.Thread.Sleep(150); // Wait for the keyboard to launch

                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                int keyboardHeight = 350; 
                int keyboardY = screenHeight - keyboardHeight; 

                IntPtr oskHandle = FindWindow("OSKMainClass", null);
                if (oskHandle != IntPtr.Zero)
                {
                    SetWindowPos(oskHandle, IntPtr.Zero, 0, keyboardY, screenWidth, keyboardHeight, SWP_SHOWWINDOW); // Set the position and size of the on-screen keyboard window
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error launching keyboard: {ex.Message}");
            }
        }


        public static void CloseTouchKeyboard() // Method to close the on-screen keyboard
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("osk")) // Get all processes with the name "osk" (on-screen keyboard)
                {
                    process.Kill(); // Kill the process to close the on-screen keyboard
                    process.WaitForExit(300); // Wait for the process to exit, with a timeout of 300 milliseconds
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing keyboard: {ex.Message}");
            }
        }


 
        public static string PromptWithKeyboard(string prompt, string title, string defaultResponse = "") // Method to show an input box with the on-screen keyboard
        {
            try
            {
                OpenTouchKeyboard();

                System.Threading.Thread.Sleep(100);

                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                int xPos = (int)((screenWidth / 2) - 200);
                int yPos = (int)(screenHeight * 0.1);

                return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultResponse, xPos, yPos);
            }
            finally
            {
                CloseTouchKeyboard();
            }
        }
    }
}