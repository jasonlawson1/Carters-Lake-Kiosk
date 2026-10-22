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
        private bool _isKeyboardOpen = true;

        public GameManagerWindow()
        {
            InitializeComponent();
            LoadGames();

            // Hook up window load and close events for touch keyboard handling
            this.Loaded += GameManagerWindow_Loaded;
            this.Closed += GameManagerWindow_Closed;

            // Keep window pinned to the very top when text boxes receive focus
            TxtEditTitle.GotFocus += TextBox_GotFocus;
            TxtEditCategory.GotFocus += TextBox_GotFocus;
            TxtEditDescription.GotFocus += TextBox_GotFocus;
        }

        private void GameManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Snap window to absolute top (Top = 0) and center it horizontally
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

        private void GameManagerWindow_Closed(object sender, EventArgs e)
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure the window stays locked at the very top of the screen
                this.Top = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not adjust window position: {ex.Message}");
            }
        }

        private void ToggleKeyboard_Click(object sender, RoutedEventArgs e)
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

        private void LoadGames()
        {
            dgGames.ItemsSource = null;
            dgGames.ItemsSource = GameSelection.GamesList;
        }

        private void dgGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGames.SelectedItem is Game selectedGame)
            {
                TxtEditTitle.Text = selectedGame.Title;
                TxtEditCategory.Text = selectedGame.Category;
                TxtEditDescription.Text = selectedGame.Description;
            }
        }

        private void BtnReplaceThumbnails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgGames.SelectedItem is Game selectedGame)
                {
                    string safeFolderName = string.Join("_", selectedGame.Title.Split(Path.GetInvalidFileNameChars()));
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", safeFolderName);
                    string imageTargetDirectory = Path.Combine(targetDirectory, "Images");

                    List<string> newImageList = new List<string>();

                    MessageBox.Show("Select up to 4 new replacement thumbnails. Click 'Cancel' at any time to stop.", "Replace Thumbnails", MessageBoxButton.OK, MessageBoxImage.Information);

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
                                Directory.CreateDirectory(imageTargetDirectory);
                                string sourceImgPath = imageDialog.FileName;
                                string imgFileName = Path.GetFileName(sourceImgPath);
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName);

                                File.Copy(sourceImgPath, destImgPath, overwrite: true);

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Automatically save any pending text changes for the currently selected game
                if (dgGames.SelectedItem is Game selectedGame)
                {
                    string newTitle = TxtEditTitle.Text.Trim();
                    if (!string.IsNullOrEmpty(newTitle))
                    {
                        selectedGame.Title = newTitle;
                    }

                    selectedGame.Category = TxtEditCategory.Text.Trim();
                    selectedGame.Description = TxtEditDescription.Text.Trim();
                }

                // Persist changes to disk JSON file
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