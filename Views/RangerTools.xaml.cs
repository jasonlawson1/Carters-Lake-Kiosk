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
            InitializeComponent(); //Initialize the components of the RangerTools page. Keep on top
        }

        // Event handler for the back button click event. Navigates back to the previous page.
        private void rangerToolsBackButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }

        // Logic for exit via the logo button. Opens the touch keyboard and requests application exit.
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

        // Event handler for the "Add Game" button click event. Opens a file dialog to select a game executable and adds it to the kiosk system.
        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyboardHelper.OpenTouchKeyboard(); //Opens Keyboard

                string localGamesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games"); // Define the local games folder path
                Directory.CreateDirectory(localGamesFolder); // Ensure the local games folder exists

                OpenFileDialog openFileDialog = new OpenFileDialog // Create and configure the OpenFileDialog to select a game executable
                {
                    Filter = "Game Executables (*.exe)|*.exe|All Files (*.*)|*.*", // Filter for executable files
                    Title = "Select Game Executable (USB or Local)", // Set the dialog title
                    InitialDirectory = localGamesFolder, // Set the initial directory to the local games folder
                    RestoreDirectory = true // Restore the directory to the previously selected directory after closing the dialog
                };


                if (openFileDialog.ShowDialog() == true) // Show the dialog and check if the user selected a file
                {
                    string sourceExePath = openFileDialog.FileName; // Get the selected executable file path

                    Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background); // Process any pending UI events to ensure the dialog closes properly
                    var mainWindow = Window.GetWindow(this); // Get the main window of the application
                    if (mainWindow != null) // Check if the main window is not null
                    {
                        mainWindow.Activate(); // Activate the main window to bring it to the foreground
                        mainWindow.Focus(); // Set focus to the main window to ensure it is ready for user interaction
                    }

                    // Ensure the GamesList is initialized before adding a new game
                    if (GameSelection.GamesList == null)
                    {
                        GameSelection.GamesList = new List<Game>();
                    }


                    FileInfo newFileInfo = new FileInfo(sourceExePath); // Create a FileInfo object for the selected executable to access its properties

                    bool isDuplicateGame = GameSelection.GamesList.Any(g => // Check for duplicate games based on executable path and file properties
                    {
                        if (string.IsNullOrEmpty(g.ExecutablePath)) return false;

                        string existingFullPath = Path.IsPathRooted(g.ExecutablePath) //Check if the existing executable path is rooted (absolute) or relative
                            ? g.ExecutablePath // If rooted, use it directly
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, g.ExecutablePath); // If relative, combine it with the base directory to get the full path

                        if (!File.Exists(existingFullPath)) return false; // If the existing file doesn't exist, skip this game

                        FileInfo existingFileInfo = new FileInfo(existingFullPath); // Create a FileInfo object for the existing executable to access its properties

                        return existingFileInfo.Name.Equals(newFileInfo.Name, StringComparison.OrdinalIgnoreCase) && // Compare the file names (case-insensitive)
                               existingFileInfo.Length == newFileInfo.Length; // Compare the file sizes to ensure they are identical
                    });

                    // Warning message in case of duplicated game.
                    if (isDuplicateGame)
                    {
                        MessageBox.Show("This game has already been added to the application.", "Duplicate Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string defaultTitle = Path.GetFileNameWithoutExtension(sourceExePath); // Get the default title for the game based on the executable file name without extension

                    // Prompt the user to enter a custom title for the game using the touch keyboard
                    string customTitle = KeyboardHelper.PromptWithKeyboard(
                        "Enter the display title for this game:",
                        "Game Title",
                        defaultTitle);

                    if (string.IsNullOrWhiteSpace(customTitle)) return; // If the user cancels or enters an empty title, exit the method
                    customTitle = customTitle.Trim(); // Trim any leading or trailing whitespace from the custom title


                    // Check for duplicate titles in the existing games list (case-insensitive)
                    if (GameSelection.GamesList.Any(g => g.Title.Equals(customTitle, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("There is a game with this title already in the list.", "Duplicate Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    //Custom Cateory
                    string customCategory = KeyboardHelper.PromptWithKeyboard(
                        "Enter the category (e.g., Water Safety, Puzzle, ...):",
                        "Game Category",
                        "Added Games");

                    if (string.IsNullOrWhiteSpace(customCategory)) customCategory = "Added Games"; // Default category if the user cancels or enters an empty category

                    //Custom Description
                    string customDescription = KeyboardHelper.PromptWithKeyboard(
                        "Enter a short description for the game:",
                        "Game Description",
                        "Custom added game build for the kiosk.");

                    if (string.IsNullOrWhiteSpace(customDescription)) customDescription = "Custom added game build.";


                    string safeFolderName = string.Join("_", customTitle.Split(Path.GetInvalidFileNameChars())); // Create a safe folder name by replacing invalid characters in the custom title with underscores
                    string baseAppDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar; // Get the base application directory and ensure it ends with a directory separator
                    string sourceDirectory = Path.GetDirectoryName(sourceExePath); // Get the directory of the selected executable file
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", safeFolderName); // Define the target directory for the game based on the safe folder name
                    string finalStoredPath; // Variable to hold the final stored path for the executable, which will be used in the Game object

                    if (!sourceExePath.StartsWith(baseAppDirectory, StringComparison.OrdinalIgnoreCase)) // Check if the source executable is not already in the base application directory
                    {
                        if (!string.IsNullOrEmpty(sourceDirectory) && Directory.Exists(sourceDirectory)) // Check if the source directory is valid and exists   
                        {
                            CopyDirectory(sourceDirectory, targetDirectory); // Copy the entire source directory (including the executable and any related files) to the target directory
                        }

                        string exeFileName = Path.GetFileName(sourceExePath); // Get the executable file name from the source path
                        finalStoredPath = Path.Combine("Games", safeFolderName, exeFileName); // Store the relative path to the executable in the Game object, which will be used for launching the game later
                    }
                    else
                    {
                        finalStoredPath = sourceExePath.Substring(baseAppDirectory.Length); // If the source executable is already in the base application directory, store the relative path to it in the Game object
                    }

                    List<string> customImageList = new List<string>();
                    string imageTargetDirectory = Path.Combine(targetDirectory, "Images"); // Define the target directory for storing custom images (thumbnails) for the game

                    MessageBox.Show("You can now select up to 4 custom screenshots/thumbnails for this game. Click 'Cancel' at any time to finish selecting.", "Custom Thumbnails", MessageBoxButton.OK, MessageBoxImage.Information); //Window informing about custom thumbnails


                    // Loop to allow the user to select up to 4 custom images for the game
                    for (int i = 1; i <= 4; i++) 
                    {
                        OpenFileDialog imageDialog = new OpenFileDialog
                        {
                            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
                            Title = $"Select Thumbnail {i} of 4 (Optional - Click Cancel to stop)"
                        };

                        if (imageDialog.ShowDialog() == true) // If the user selects an image, copy it to the target directory and add its relative path to the customImageList
                        {
                            Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background); // Process any pending UI events to ensure the dialog closes properly
                            if (mainWindow != null)
                            {
                                mainWindow.Activate();
                                mainWindow.Focus();
                            }

                            try
                            {
                                // Ensure the Images directory exists before copying the selected image
                                Directory.CreateDirectory(imageTargetDirectory); // Create the Images directory if it doesn't exist
                                string sourceImgPath = imageDialog.FileName; // Get the full path of the selected image file
                                string imgFileName = Path.GetFileName(sourceImgPath); // Get the file name of the selected image (without the directory path)
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName); // Define the destination path for the image in the Images directory

                                File.Copy(sourceImgPath, destImgPath, overwrite: true); // Copy the selected image to the Images directory, overwriting any existing file with the same name

                                string relativeImgPath = Path.Combine("Games", safeFolderName, "Images", imgFileName).Replace("\\", "/"); // Create a relative path for the image to be stored in the Game object, replacing backslashes with forward slashes for consistency
                                customImageList.Add("/" + relativeImgPath); // Add the relative path of the copied image to the customImageList, which will be used for displaying thumbnails in the kiosk
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Could not copy thumbnail {i}: {ex.Message}", "Image Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // Show a warning message if there was an error copying the selected image, but continue allowing the user to select additional images
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (customImageList.Count == 0)
                    {
                        customImageList.Add("/Images/DefaultGameMenu.png"); //Default image in case there are no custom images
                    }

                    Game newGame = new Game // Create a new Game object with the provided metadata and paths
                    {
                        Title = customTitle.Trim(),
                        ExecutablePath = finalStoredPath,
                        Category = customCategory.Trim(),
                        Description = customDescription.Trim(),
                        ImagePath = customImageList[0],
                        ImageList = customImageList,
                        IsActive = true
                    };

                    newGame.Id = App.Db.InsertGame(newGame.Title); // Insert the new game into the database and retrieve its assigned ID
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
                KeyboardHelper.CloseTouchKeyboard();
            }
        }


        private static void CopyDirectory(string sourceDir, string targetDir) // Recursively copies the contents of the source directory to the target directory, including all files and subdirectories
        {
            Directory.CreateDirectory(targetDir); // Ensure the target directory exists before copying files and subdirectories

            foreach (string file in Directory.GetFiles(sourceDir)) // Iterate through all files in the source directory and copy them to the target directory
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, overwrite: true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir)) // Iterate through all subdirectories in the source directory and recursively copy them to the target directory
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, targetSubDir);
            }
        }


        private void ManageGameListButton_Click(object sender, RoutedEventArgs e) // Event handler for the "Manage Game List" button click event. Displays the game management panel and populates the game list view with the current games.
        {
            PanelMenuButtons.Visibility = Visibility.Collapsed; // Hide the main menu buttons panel to focus on game management
            PanelManageGames.Visibility = Visibility.Visible; // Show the game management panel where users can edit game metadata, replace thumbnails, or remove games
            GameListView.ItemsSource = null; // Clear the existing items in the game list view to ensure it is refreshed with the latest data
            GameListView.ItemsSource = GameSelection.GamesList; // Set the items source of the game list view to the current list of games, allowing users to select and manage them
        }

        private void CloseManagePanel_Click(object sender, RoutedEventArgs e) // Event handler for the "Close" button click event in the game management panel. Hides the game management panel and returns to the main menu.
        {
            PanelManageGames.Visibility = Visibility.Collapsed;
            PanelMenuButtons.Visibility = Visibility.Visible;
        }

        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e) // Event handler for the selection change event of the game list view. Updates the text boxes with the selected game's metadata for editing.
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                TxtEditTitle.Text = selectedGame.Title;
                TxtEditCategory.Text = selectedGame.Category;
                TxtEditDescription.Text = selectedGame.Description;
            }
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e) // Event handler for the "Save Changes" button click event in the game management panel. Saves the edited metadata of the selected game and updates the game list.
        {
            try
            {
                if (GameListView.SelectedItem is Game selectedGame)
                {
                    selectedGame.Title = TxtEditTitle.Text.Trim();
                    selectedGame.Category = TxtEditCategory.Text.Trim();
                    selectedGame.Description = TxtEditDescription.Text.Trim();

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
                KeyboardHelper.CloseTouchKeyboard();
            }

            PanelManageGames.Visibility = Visibility.Collapsed;
            PanelMenuButtons.Visibility = Visibility.Visible;
        }


        private void BtnReplaceThumbnails_Click(object sender, RoutedEventArgs e) // Event handler for the "Replace Thumbnails" button click event in the game management panel. Allows the user to select new thumbnails for the selected game and updates the game's image list.
        {
            try
            {
                if (GameListView.SelectedItem is Game selectedGame)
                {
                    string safeFolderName = string.Join("_", selectedGame.Title.Split(Path.GetInvalidFileNameChars())); // Create a safe folder name by replacing invalid characters in the selected game's title with underscores
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", safeFolderName); // Define the target directory for the selected game based on the safe folder name
                    string imageTargetDirectory = Path.Combine(targetDirectory, "Images"); // Define the target directory for the game's images

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
                                Directory.CreateDirectory(imageTargetDirectory); // Ensure the Images directory exists before copying the selected image
                                string sourceImgPath = imageDialog.FileName; // Get the full path of the selected image file
                                string imgFileName = Path.GetFileName(sourceImgPath); // Get the file name of the selected image (without the directory path)
                                string destImgPath = Path.Combine(imageTargetDirectory, imgFileName); // Define the destination path for the image in the Images directory

                                File.Copy(sourceImgPath, destImgPath, overwrite: true); // Copy the selected image to the Images directory, overwriting any existing file with the same name

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
                        selectedGame.ImageList = newImageList; // Update the selected game's image list with the new thumbnails
                        selectedGame.ImagePath = newImageList[0]; // Update the selected game's main image path to the first new thumbnail
                        GameSelection.SaveGamesList(); // Save the updated game list to persist the changes

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


        private void RemoveGameButton_Click(object sender, RoutedEventArgs e)// Event handler for the "Remove Game" button click event in the game management panel. Prompts the user to enter the title of the game to remove and deletes it from the kiosk system.
        {
            try
            {
                if (GameSelection.GamesList == null || GameSelection.GamesList.Count == 0)
                {
                    MessageBox.Show("No games available to remove.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string currentGames = string.Join("\n", GameSelection.GamesList.Select(g => g.Title)); // Create a string listing all current game titles for display in the prompt 

                string inputTitle = KeyboardHelper.PromptWithKeyboard(
                    $"Current games on kiosk:\n{currentGames}\n\nEnter the exact title of the game you want to remove:", // Prompt the user to enter the title of the game to remove, displaying the current games for reference
                    "Remove Game",
                    "");

                if (string.IsNullOrWhiteSpace(inputTitle)) return;

                var match = GameSelection.GamesList.FirstOrDefault(g => g.Title.Equals(inputTitle.Trim(), StringComparison.OrdinalIgnoreCase)); // Find the game in the list that matches the entered title (case-insensitive)

                if (match != null)
                {
                    if (!string.IsNullOrEmpty(match.ExecutablePath))
                    {
                        try
                        {
                            string fullPath = Path.IsPathRooted(match.ExecutablePath) //Check if the executable path is rooted (absolute) or relative
                                ? match.ExecutablePath
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, match.ExecutablePath);

                            string gameDirectory = Path.GetDirectoryName(fullPath); // Get the directory of the executable to delete the entire game folder

                            if (!string.IsNullOrEmpty(gameDirectory) && // Ensure the game directory is valid and exists
                                gameDirectory.Contains("Games", StringComparison.OrdinalIgnoreCase) &&
                                Directory.Exists(gameDirectory))
                            {
                                foreach (string file in Directory.GetFiles(gameDirectory, "*.*", SearchOption.AllDirectories)) // Iterate through all files in the game directory and its subdirectories to remove any read-only attributes before deletion
                                {
                                    File.SetAttributes(file, FileAttributes.Normal); // Remove read-only attribute to ensure the file can be deleted
                                }

                                Directory.Delete(gameDirectory, recursive: true); // Delete the entire game directory and its contents recursively
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Warning: Could not delete physical folder: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // Show a warning message if there was an error deleting the physical folder, but continue with the removal process
                        }
                    }

                    GameSelection.GamesList.Remove(match); // Remove the game from the in-memory list of games
                    GameSelection.SaveGamesList(); // Save the updated game list to persist the removal

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

        private void ChkIsActive_Click(object sender, RoutedEventArgs e) // Event handler for the "IsActive" checkbox click event in the game management panel. Saves the updated game list when the active status of a game is changed.
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Game clickedGame) // Check if the sender is a CheckBox and its DataContext is a Game object
            {
                GameSelection.SaveGamesList(); // Save the updated game list to persist the change in the active status of the clicked game
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) // Event handler for the GotFocus event of text boxes in the game management panel. Opens the touch keyboard when a text box gains focus.
        {
            KeyboardHelper.OpenTouchKeyboard();
        }
    }
}