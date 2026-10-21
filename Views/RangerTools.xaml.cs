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

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                    if (GameSelection.GamesList == null)
                    {
                        GameSelection.GamesList = new List<Game>();
                    }

                    // 1. ROBUST FILE-BASED DUPLICATE CHECK
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

                    // 2. Prompt staff for a custom Title
                    string customTitle = KeyboardHelper.PromptWithKeyboard(
                        "Enter the display title for this game:",
                        "Game Title",
                        defaultTitle);

                    if (string.IsNullOrWhiteSpace(customTitle)) return;
                    customTitle = customTitle.Trim();

                    // Prevent duplicate titles
                    if (GameSelection.GamesList.Any(g => g.Title.Equals(customTitle, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A game with this title already exists. Please choose a unique title.", "Duplicate Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 3. Prompt staff for a custom Category
                    string customCategory = KeyboardHelper.PromptWithKeyboard(
                        "Enter the category (e.g., Water Safety, Puzzle, Added Games):",
                        "Game Category",
                        "Added Games");

                    if (string.IsNullOrWhiteSpace(customCategory)) customCategory = "Added Games";

                    // 4. Prompt staff for a custom Description
                    string customDescription = KeyboardHelper.PromptWithKeyboard(
                        "Enter a short description for the game:",
                        "Game Description",
                        "Custom added game build for the kiosk.");

                    if (string.IsNullOrWhiteSpace(customDescription)) customDescription = "Custom added game build.";

                    // 5. COPY GAME FILES TO LOCAL KIOSK DIRECTORY
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

                    // 6. PROMPT FOR UP TO 4 CUSTOM THUMBNAILS
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
                            try
                            {
                                Directory.CreateDirectory(imageTargetDirectory);
                                string sourceImgPath = imageDialog.FileName;
                                string imgFileName = Path.GetFileName(sourceImgPath);
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName);

                                File.Copy(sourceImgPath, destImgPath, overwrite: true);

                                // Build relative path formatted cleanly for WPF image loading
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
                            // User clicked cancel, stop asking for more thumbnails
                            break;
                        }
                    }

                    // Fallback to default thumbnail if none were provided
                    if (customImageList.Count == 0)
                    {
                        customImageList.Add("/Images/DefaultGameMenu.png");
                    }

                    // 7. Create and save game object
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

        private void ManageGameListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GameManagerWindow managerWindow = new GameManagerWindow();
                managerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open game manager: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Loads selected game info into the text boxes
        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                TxtEditTitle.Text = selectedGame.Title;
                TxtEditCategory.Text = selectedGame.Category;
                TxtEditDescription.Text = selectedGame.Description;
            }
        }

        // Saves text edits to the selected game and updates the JSON file
        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameListView.SelectedItem is Game selectedGame)
                {
                    string newTitle = TxtEditTitle.Text.Trim();
                    if (string.IsNullOrEmpty(newTitle))
                    {
                        MessageBox.Show("Game title cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    selectedGame.Title = newTitle;
                    selectedGame.Category = TxtEditCategory.Text.Trim();
                    selectedGame.Description = TxtEditDescription.Text.Trim();

                    GameSelection.SaveGamesList();

                    // Refresh list view to show updated title
                    GameListView.ItemsSource = null;
                    GameListView.ItemsSource = GameSelection.GamesList;

                    MessageBox.Show("Game details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Please select a game from the list first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Replaces the 4 thumbnails on the fly for the selected game
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
                    // 1. DELETE PHYSICAL FILES FROM DISK
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
                                // Clear Read-Only attributes so Windows permits deletion
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

                    // 2. DIRECTLY REMOVE FROM LIST AND SAVE PERMANENTLY
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
    }
}