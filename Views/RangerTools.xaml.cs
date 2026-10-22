using Capstone_UI.Models;
using Capstone_UI.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Capstone_UI
{
    /// <summary>
    /// Interaction logic for RangerTools.xaml. Provides administrative management tools 
    /// for kiosk rangers to add new game builds, edit metadata, manage thumbnails, 
    /// and safely remove games from the system.
    /// </summary>
    public partial class RangerTools : Page
    {
        public RangerTools()
        {
            InitializeComponent();
        }

        private void rangerToolsBackButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }

        private void logoExitRangerTools(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyboardHelper.OpenTouchKeyboard();
                AdminAuth.RequestAppExit();
            }
            finally
            {
                KeyboardHelper.CloseTouchKeyboard();
            }
        }

        // ==========================================
        // Add Game Management Workflow
        // ==========================================
        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Explicitly open the digital keyboard immediately when "Add Game" is clicked
                // so it's already on screen for the file picker and subsequent prompts.
                KeyboardHelper.OpenTouchKeyboard();

                string localGamesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games");
                Directory.CreateDirectory(localGamesFolder);

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Game Executables (*.exe)|*.exe|All Files (*.*)|*.*",
                    Title = "Select Game Executable (USB or Local)",
                    InitialDirectory = localGamesFolder,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceExePath = openFileDialog.FileName;

                    // Ensure window focus is restored after file dialog closes
                    Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                    var mainWindow = Window.GetWindow(this);
                    if (mainWindow != null)
                    {
                        mainWindow.Activate();
                        mainWindow.Focus();
                    }

                    if (GameSelection.GamesList == null)
                    {
                        GameSelection.GamesList = new List<Game>();
                    }

                    FileInfo newFileInfo = new FileInfo(sourceExePath);

                    bool isDuplicateGame = GameSelection.GamesList.Any(g =>
                    {
                        if (string.IsNullOrEmpty(g.ExecutablePath)) return false;

                        string existingFullPath = Path.IsPathRooted(g.ExecutablePath)
                            ? g.ExecutablePath
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, g.ExecutablePath);

                        if (!File.Exists(existingFullPath)) return false;

                        FileInfo existingFileInfo = new FileInfo(existingFullPath);

                        return existingFileInfo.Name.Equals(newFileInfo.Name, StringComparison.OrdinalIgnoreCase) &&
                               existingFileInfo.Length == newFileInfo.Length;
                    });

                    if (isDuplicateGame)
                    {
                        MessageBox.Show("This exact game build (.exe) is already installed on the kiosk system!", "Duplicate Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string defaultTitle = Path.GetFileNameWithoutExtension(sourceExePath);

                    // Prompt staff for a custom Title (keyboard is already open and ready)
                    string customTitle = KeyboardHelper.PromptWithKeyboard(
                        "Enter the display title for this game:",
                        "Game Title",
                        defaultTitle);

                    if (string.IsNullOrWhiteSpace(customTitle)) return;
                    customTitle = customTitle.Trim();

                    if (GameSelection.GamesList.Any(g => g.Title.Equals(customTitle, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A game with this title already exists. Please choose a unique title.", "Duplicate Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Prompt staff for a custom Category
                    string customCategory = KeyboardHelper.PromptWithKeyboard(
                        "Enter the category (e.g., Water Safety, Puzzle, Added Games):",
                        "Game Category",
                        "Added Games");

                    if (string.IsNullOrWhiteSpace(customCategory)) customCategory = "Added Games";

                    // Prompt staff for a custom Description
                    string customDescription = KeyboardHelper.PromptWithKeyboard(
                        "Enter a short description for the game:",
                        "Game Description",
                        "Custom added game build for the kiosk.");

                    if (string.IsNullOrWhiteSpace(customDescription)) customDescription = "Custom added game build.";

                    string safeFolderName = string.Join("_", customTitle.Split(Path.GetInvalidFileNameChars()));

                    string baseAppDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    string sourceDirectory = Path.GetDirectoryName(sourceExePath);
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", safeFolderName);

                    string finalStoredPath;

                    if (!sourceExePath.StartsWith(baseAppDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(sourceDirectory) && Directory.Exists(sourceDirectory))
                        {
                            CopyDirectory(sourceDirectory, targetDirectory);
                        }

                        string exeFileName = Path.GetFileName(sourceExePath);
                        finalStoredPath = Path.Combine("Games", safeFolderName, exeFileName);
                    }
                    else
                    {
                        finalStoredPath = sourceExePath.Substring(baseAppDirectory.Length);
                    }

                    List<string> customImageList = new List<string>();
                    string imageTargetDirectory = Path.Combine(targetDirectory, "Images");

                    MessageBox.Show("You can now select up to 4 custom screenshots/thumbnails for this game. Click 'Cancel' at any time to finish selecting.", "Custom Thumbnails", MessageBoxButton.OK, MessageBoxImage.Information);

                    for (int i = 1; i <= 4; i++)
                    {
                        OpenFileDialog imageDialog = new OpenFileDialog
                        {
                            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
                            Title = $"Select Thumbnail {i} of 4 (Optional - Click Cancel to stop)"
                        };

                        if (imageDialog.ShowDialog() == true)
                        {
                            Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                            if (mainWindow != null)
                            {
                                mainWindow.Activate();
                                mainWindow.Focus();
                            }

                            try
                            {
                                Directory.CreateDirectory(imageTargetDirectory);
                                string sourceImgPath = imageDialog.FileName;
                                string imgFileName = Path.GetFileName(sourceImgPath);
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName);

                                File.Copy(sourceImgPath, destImgPath, overwrite: true);

                                string relativeImgPath = Path.Combine("Games", safeFolderName, "Images", imgFileName).Replace("\\", "/");
                                customImageList.Add("/" + relativeImgPath);
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

                    if (customImageList.Count == 0)
                    {
                        customImageList.Add("/Images/DefaultGameMenu.png");
                    }

                    Game newGame = new Game
                    {
                        Title = customTitle,
                        ExecutablePath = finalStoredPath,
                        Category = customCategory.Trim(),
                        Description = customDescription.Trim(),
                        ImagePath = customImageList[0],
                        ImageList = customImageList,
                        IsActive = true
                    };

                    GameSelection.GamesList.Add(newGame);
                    GameSelection.SaveGamesList();

                    MessageBox.Show($"'{newGame.Title}' was successfully added with {customImageList.Count} thumbnail(s) and saved permanently!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not add game: {ex.Message}", "Error Adding Game", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Ensure keyboard closes when the entire add-game workflow finishes
                KeyboardHelper.CloseTouchKeyboard();
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, overwrite: true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, targetSubDir);
            }
        }

        // ==========================================
        // Game List & Detail Management Tools
        // ==========================================
        private void ManageGameListButton_Click(object sender, RoutedEventArgs e)
        {
            PanelMenuButtons.Visibility = Visibility.Collapsed;
            PanelManageGames.Visibility = Visibility.Visible;
            GameListView.ItemsSource = null;
            GameListView.ItemsSource = GameSelection.GamesList;
        }

        private void CloseManagePanel_Click(object sender, RoutedEventArgs e)
        {
            PanelManageGames.Visibility = Visibility.Collapsed;
            PanelMenuButtons.Visibility = Visibility.Visible;
        }

        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                TxtEditTitle.Text = selectedGame.Title;
                TxtEditCategory.Text = selectedGame.Category;
                TxtEditDescription.Text = selectedGame.Description;
            }
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameListView.SelectedItem is Game selectedGame)
                {
                    // Update metadata from the edit text boxes
                    selectedGame.Title = TxtEditTitle.Text.Trim();
                    selectedGame.Category = TxtEditCategory.Text.Trim();
                    selectedGame.Description = TxtEditDescription.Text.Trim();

                    // Save to disk
                    GameSelection.SaveGamesList();
                    GameListView.Items.Refresh();

                    MessageBox.Show("Game metadata successfully saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Please select a game to save changes.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                // 1. Ensure the digital keyboard is closed when finishing the edit
                KeyboardHelper.CloseTouchKeyboard();
            }

            // 2. Automatically close the manage panel and return to the main Ranger Tools menu
            PanelManageGames.Visibility = Visibility.Collapsed;
            PanelMenuButtons.Visibility = Visibility.Visible;
        }

        private void BtnReplaceThumbnails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameListView.SelectedItem is Game selectedGame)
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

        // ==========================================
        // Remove Game Workflow
        // ==========================================
        private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameSelection.GamesList == null || GameSelection.GamesList.Count == 0)
                {
                    MessageBox.Show("No games available to remove.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string currentGames = string.Join("\n", GameSelection.GamesList.Select(g => g.Title));

                string inputTitle = KeyboardHelper.PromptWithKeyboard(
                    $"Current games on kiosk:\n{currentGames}\n\nEnter the exact title of the game you want to remove:",
                    "Remove Game",
                    "");

                if (string.IsNullOrWhiteSpace(inputTitle)) return;

                var match = GameSelection.GamesList.FirstOrDefault(g => g.Title.Equals(inputTitle.Trim(), StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    if (!string.IsNullOrEmpty(match.ExecutablePath))
                    {
                        try
                        {
                            string fullPath = Path.IsPathRooted(match.ExecutablePath)
                                ? match.ExecutablePath
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, match.ExecutablePath);

                            string gameDirectory = Path.GetDirectoryName(fullPath);

                            if (!string.IsNullOrEmpty(gameDirectory) &&
                                gameDirectory.Contains("Games", StringComparison.OrdinalIgnoreCase) &&
                                Directory.Exists(gameDirectory))
                            {
                                foreach (string file in Directory.GetFiles(gameDirectory, "*.*", SearchOption.AllDirectories))
                                {
                                    File.SetAttributes(file, FileAttributes.Normal);
                                }

                                Directory.Delete(gameDirectory, recursive: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Warning: Could not delete physical folder: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    GameSelection.GamesList.Remove(match);
                    GameSelection.SaveGamesList();

                    MessageBox.Show($"'{match.Title}' was successfully removed from the kiosk and its folder was deleted from disk!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No game found matching '{inputTitle}'. Please check the title and try again.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not remove game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChkIsActive_Click(object sender, RoutedEventArgs e)
        {
            // Ensure clicking the checkbox retrieves the specific Game instance from its DataContext directly
            if (sender is CheckBox checkBox && checkBox.DataContext is Game clickedGame)
            {
                // TwoWay binding has already updated clickedGame.IsActive to match the checkbox state.
                // Save the list immediately to disk:
                GameSelection.SaveGamesList();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Automatically trigger the digital keyboard whenever any edit text box is tapped
            KeyboardHelper.OpenTouchKeyboard();
        }

    }
}