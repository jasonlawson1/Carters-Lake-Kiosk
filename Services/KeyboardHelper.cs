using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Capstone_UI
{
    /// <summary>
    /// Helper utility for managing the Windows On-Screen Keyboard (OSK) lifecycle, 
    /// programmatic repositioning via Win32 API calls, and touch-friendly input prompts.
    /// </summary>
    public static class KeyboardHelper
    {
        // ==========================================
        // Win32 Interop Imports & Constants
        // ==========================================

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;


        // ==========================================
        // Keyboard Lifecycle Methods
        // ==========================================

        /// <summary>
        /// Launches the Windows On-Screen Keyboard (osk.exe) robustly, bypassing 
        /// 32/64-bit filesystem redirection, and repositions it along the bottom edge.
        /// </summary>
        public static void OpenTouchKeyboard()
        {
            try
            {
                // Prevent duplicate instances if already open
                CloseTouchKeyboard();

                // Resolve native system path to bypass WOW64 redirection on 64-bit Windows
                string oskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "osk.exe");

                // If running as a 32-bit process on a 64-bit OS, point directly to Sysnative
                if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                {
                    string sysnativePath = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName, "Sysnative", "osk.exe");
                    if (File.Exists(sysnativePath))
                    {
                        oskPath = sysnativePath;
                    }
                }

                // Step 1: Start the On-Screen Keyboard process
                Process.Start(new ProcessStartInfo
                {
                    FileName = oskPath,
                    UseShellExecute = true
                });

                // Give the OSK process a brief moment to create its window handle
                System.Threading.Thread.Sleep(150);

                // Step 2: Calculate screen dimensions and target keyboard coordinates
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                int keyboardHeight = 350; // Standard OSK height in pixels
                int keyboardY = screenHeight - keyboardHeight; // Lock perfectly to the bottom

                // Step 3: Find the OSK window handle and dock it to the calculated position
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

        /// <summary>
        /// Terminates any running instances of the Windows On-Screen Keyboard process.
        /// </summary>
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


        // ==========================================
        // Helper Prompt Methods
        // ==========================================

        /// <summary>
        /// Opens the touch keyboard and displays a modal input dialog box safely anchored 
        /// at 10% from the top of the screen while the OSK stays at the bottom.
        /// </summary>
        public static string PromptWithKeyboard(string prompt, string title, string defaultResponse = "")
        {
            try
            {
                OpenTouchKeyboard();

                // Brief pause to ensure the dialog doesn't steal focus before the keyboard renders
                System.Threading.Thread.Sleep(100);

                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                int xPos = (int)((screenWidth / 2) - 200);
                int yPos = (int)(screenHeight * 0.1); // Anchored high up (10% from top)

                return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultResponse, xPos, yPos);
            }
            finally
            {
                CloseTouchKeyboard();
            }
        }
    }
}