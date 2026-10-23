using Capstone_UI.Models;
using Capstone_UI.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Capstone_UI
{
    public partial class GameManagerWindow : Window
    {
        private bool _isKeyboardOpen = true; // Track the state of the touch keyboard

        public GameManagerWindow()
        {
            InitializeComponent();
            LoadGames(); // Load the games into the DataGrid when the window is initialized

            this.Loaded += GameManagerWindow_Loaded; // Subscribe to the Loaded event to open the touch keyboard
            this.Closed += GameManagerWindow_Closed; // Subscribe to the Closed event to close the touch keyboard

            TxtEditTitle.GotFocus += TextBox_GotFocus; // Subscribe to the GotFocus event for each TextBox to adjust window position
            TxtEditCategory.GotFocus += TextBox_GotFocus; // Subscribe to the GotFocus event for each TextBox to adjust window position
            TxtEditDescription.GotFocus += TextBox_GotFocus; // Subscribe to the GotFocus event for each TextBox to adjust window position
        }

        private void GameManagerWindow_Loaded(object sender, RoutedEventArgs e) // Event handler for the Loaded event of the window
        {
            try // Attempt to open the touch keyboard and position the window
            {
                this.Top = 0;
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;

                KeyboardHelper.OpenTouchKeyboard();
                _isKeyboardOpen = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not open touch keyboard: {ex.Message}");
            }
        }

        private void GameManagerWindow_Closed(object sender, EventArgs e) // Event handler for the Closed event of the window
        {
            try
            {
                KeyboardHelper.CloseTouchKeyboard();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not close touch keyboard: {ex.Message}");
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) //  Event handler for the GotFocus event of the TextBoxes
        {
            try
            {
                this.Top = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not adjust window position: {ex.Message}");
            }
        }

        private void ToggleKeyboard_Click(object sender, RoutedEventArgs e) // Event handler for the Toggle Keyboard button click
        {
            try
            {
                if (_isKeyboardOpen)
                {
                    KeyboardHelper.CloseTouchKeyboard();
                    _isKeyboardOpen = false;
                }
                else
                {
                    KeyboardHelper.OpenTouchKeyboard();
                    _isKeyboardOpen = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling keyboard: {ex.Message}");
            }
        }

        private void LoadGames() // Method to load games into the DataGrid
        {
            dgGames.ItemsSource = null; // Clear the existing items source
            dgGames.ItemsSource = GameSelection.GamesList; // Set the items source to the list of games
        }

        private void dgGames_SelectionChanged(object sender, SelectionChangedEventArgs e) // Event handler for the SelectionChanged event of the DataGrid
        {
            if (dgGames.SelectedItem is Game selectedGame) // Check if the selected item is a Game object
            {
                TxtEditTitle.Text = selectedGame.Title;
                TxtEditCategory.Text = selectedGame.Category;
                TxtEditDescription.Text = selectedGame.Description;
            }
        }

        private void BtnReplaceThumbnails_Click(object sender, RoutedEventArgs e) // Event handler for the Replace Thumbnails button click
        {
            try
            {
                if (dgGames.SelectedItem is Game selectedGame) // Check if a game is selected in the DataGrid
                {
                    string safeFolderName = string.Join("_", selectedGame.Title.Split(Path.GetInvalidFileNameChars())); // Create a safe folder name by replacing invalid characters with underscores
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", safeFolderName); // Define the target directory for the selected game
                    string imageTargetDirectory = Path.Combine(targetDirectory, "Images"); // Define the target directory for the images

                    List<string> newImageList = new List<string>(); // Create a new list to hold the new image paths

                    MessageBox.Show("Select up to 4 new replacement thumbnails. Click 'Cancel' at any time to stop.", "Replace Thumbnails", MessageBoxButton.OK, MessageBoxImage.Information); // Show a message box to inform the user about the thumbnail replacement process

                    for (int i = 1; i <= 4; i++)
                    {
                        OpenFileDialog imageDialog = new OpenFileDialog
                        {
                            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
                            Title = $"Select Replacement Thumbnail {i} of 4 (Optional - Click Cancel to stop)"
                        };

                        if (imageDialog.ShowDialog() == true)
                        {
                            try
                            {
                                Directory.CreateDirectory(imageTargetDirectory); // Ensure the target directory exists
                                string sourceImgPath = imageDialog.FileName; // Get the selected image file path
                                string imgFileName = Path.GetFileName(sourceImgPath); // Get the file name of the selected image
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName); // Define the destination path for the image

                                File.Copy(sourceImgPath, destImgPath, overwrite: true); // Copy the selected image to the target directory, overwriting if it already exists

                                string relativeImgPath = Path.Combine("Games", safeFolderName, "Images", imgFileName).Replace("\\", "/");
                                newImageList.Add("/" + relativeImgPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Could not copy thumbnail {i}: {ex.Message}", "Image Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (newImageList.Count > 0)
                    {
                        selectedGame.ImageList = newImageList;
                        selectedGame.ImagePath = newImageList[0];
                        GameSelection.SaveGamesList();

                        MessageBox.Show($"Thumbnails successfully updated ({newImageList.Count} new image(s) saved)!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a game from the list first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not replace thumbnails: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) // Event handler for the Close button click
        {
            try
            {
                if (dgGames.SelectedItem is Game selectedGame) // Check if a game is selected in the DataGrid
                {
                    string newTitle = TxtEditTitle.Text.Trim(); // Get the new title from the TextBox and trim any whitespace
                    if (!string.IsNullOrEmpty(newTitle)) // Check if the new title is not empty
                    {
                        selectedGame.Title = newTitle; // Update the title of the selected game
                    }

                    selectedGame.Category = TxtEditCategory.Text.Trim();
                    selectedGame.Description = TxtEditDescription.Text.Trim();
                }

                GameSelection.SaveGamesList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to auto-save on close: {ex.Message}");
            }

            this.Close();
        }
    }
}