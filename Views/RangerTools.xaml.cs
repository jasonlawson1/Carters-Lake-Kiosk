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
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Game Executables (*.exe)|*.exe|All Files (*.*)|*.*",
                    Title = "Select Game Executable (USB or Local)"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string exePath = openFileDialog.FileName;
                    string defaultTitle = System.IO.Path.GetFileNameWithoutExtension(exePath);

                    // 1. Prompt staff for a custom Title
                    string customTitle = KeyboardHelper.PromptWithKeyboard(
                        "Enter the display title for this game:",
                        "Game Title",
                        defaultTitle);

                    if (string.IsNullOrWhiteSpace(customTitle)) return;

                    // 2. Prompt staff for a custom Category
                    string customCategory = KeyboardHelper.PromptWithKeyboard(
                        "Enter the category (e.g., Water Safety, Puzzle, Added Games):",
                        "Game Category",
                        "Added Games");

                    if (string.IsNullOrWhiteSpace(customCategory)) customCategory = "Added Games";

                    // 3. Prompt staff for a custom Description
                    string customDescription = KeyboardHelper.PromptWithKeyboard(
                        "Enter a short description for the game:",
                        "Game Description",
                        "Custom added game build for the kiosk.");

                    if (string.IsNullOrWhiteSpace(customDescription)) customDescription = "Custom added game build.";

                    // 4. Create the fully customized game object
                    Game newGame = new Game
                    {
                        Title = customTitle.Trim(),
                        ExecutablePath = exePath,
                        Category = customCategory.Trim(),
                        Description = customDescription.Trim(),
                        ImagePath = "/Images/DefaultGameMenu.png",
                        ImageList = new List<string> { "/Images/DefaultGameMenu.png" }
                    };

                    if (GameSelection.GamesList == null)
                    {
                        GameSelection.GamesList = new List<Game>();
                    }

                    // Prevent duplicate titles
                    if (GameSelection.GamesList.Any(g => g.Title.Equals(newGame.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A game with this title already exists. Please choose a unique title.", "Duplicate Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Add and save permanently
                    GameSelection.GamesList.Add(newGame);
                    GameSelection.SaveGamesList();

                    MessageBox.Show($"'{newGame.Title}' was successfully added with its custom details and saved permanently!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not add game: {ex.Message}", "Error Adding Game", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    GameSelection.RemoveGameByTitle(match.Title);
                    MessageBox.Show($"'{match.Title}' was successfully removed from the kiosk and disk permanently!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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