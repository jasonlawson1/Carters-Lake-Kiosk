using Capstone_UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Capstone_UI.Views;

namespace Capstone_UI
{
    public partial class GameSelection : Page
    {
        private static List<Game> _gamesList; // Static field to hold the list of games

        public static List<Game> GamesList // Static property to access the list of games
        {
            get
            {
                if (_gamesList == null) // If the list is null, load it from disk
                {
                    LoadGamesListFromDisk();
                }
                return _gamesList;
            }
            set { _gamesList = value; }
        }

        private DispatcherTimer _imageCycleTimer; // Timer for cycling through game images
        private int _currentImageIndex = 0; // Index to keep track of the current image being displayed

        private const string DEFAULT_FALLBACK_IMAGE = "pack://application:,,,/Images/DefaultGameMenu.png"; // Default fallback image path

        public GameSelection()
        {
            InitializeComponent();

            _imageCycleTimer = new DispatcherTimer(); // Initialize the timer
            _imageCycleTimer.Interval = TimeSpan.FromSeconds(3); // Set the interval to 3 seconds
            _imageCycleTimer.Tick += ImageCycleTimer_Tick; // Subscribe to the Tick event of the timer

            this.Loaded += GameSelection_Loaded; // Subscribe to the Loaded event of the page
            this.Unloaded += GameSelection_Unloaded; // Subscribe to the Unloaded event of the page
        }

        private void GameSelection_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGamesListFromDisk(); // Load the games list from disk when the page is loaded

            foreach (var game in GamesList) // Iterate through each game in the list
                App.Db.EnsureGame(game.Id, game.Title); // Ensure that each game is present in the database

            var activeGames = GamesList.Where(g => g.IsActive).ToList(); // Filter the list to only include active games

            GameListBox.ItemsSource = null; // Clear the current items source of the ListBox
            GameListBox.ItemsSource = activeGames; // Set the items source of the ListBox to the list of active games

            if (activeGames.Count > 0 && GameListBox.SelectedItem == null)
            {
                GameListBox.SelectedIndex = 0;
            }
        }

        private void GameSelection_Unloaded(object sender, RoutedEventArgs e) // Stop the image cycle timer when the page is unloaded
        {
            _imageCycleTimer.Stop();
        }

        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Event handler for when the selection in the ListBox changes
        {
            if (GameListBox.SelectedItem is Game selectedGame) // Check if the selected item is a Game object
            {
                // Update the UI elements with the selected game's details
                TxtTitle.Text = selectedGame.Title;
                TxtCategory.Text = selectedGame.Category; 
                TxtDescription.Text = selectedGame.Description;

                _imageCycleTimer.Stop();
                _currentImageIndex = 0;

                UpdateThumbnailImage(selectedGame);

                if (selectedGame.ImageList != null && selectedGame.ImageList.Count > 1) // If the selected game has multiple images, start the image cycle timer
                {
                    _imageCycleTimer.Start();
                }
            }
        }

        private void ImageCycleTimer_Tick(object sender, EventArgs e) // Event handler for the Tick event of the image cycle timer
        {
            if (GameListBox.SelectedItem is Game selectedGame && selectedGame.ImageList != null && selectedGame.ImageList.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex + 1) % selectedGame.ImageList.Count;
                UpdateThumbnailImage(selectedGame);
            }
        }

        private void UpdateThumbnailImage(Game game) // Method to update the thumbnail image based on the selected game
        {
            try
            {
                string imageToLoad = game.ImagePath; // Default to the main image path

                if (game.ImageList != null && game.ImageList.Count > _currentImageIndex)
                {
                    imageToLoad = game.ImageList[_currentImageIndex]; // Use the current image from the ImageList if available
                }

                BitmapImage bitmap = new BitmapImage(); // Create a new BitmapImage instance
                bitmap.BeginInit(); // Begin initialization of the BitmapImage
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Set the cache option to load the image immediately

                if (!string.IsNullOrEmpty(imageToLoad) && imageToLoad.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase)) // Check if the image path is a relative path within the application
                {
                    string cleanPath = imageToLoad.TrimStart('/');
                    bitmap.UriSource = new Uri($"pack://application:,,,/{cleanPath}", UriKind.Absolute); // Set the UriSource to the relative path within the application
                }
                else if (!string.IsNullOrEmpty(imageToLoad)) // Check if the image path is a relative path on disk
                {
                    string cleanPath = imageToLoad.TrimStart('/', '\\');
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                    if (File.Exists(fullPath))
                    {
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute); // Set the UriSource to the full path on disk if the file exists
                    }
                    else
                    {
                        bitmap.UriSource = new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute); // Use the default fallback image if the file does not exist
                    }
                }
                else
                {
                    bitmap.UriSource = new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute); // Use the default fallback image if the image path is null or empty
                }

                bitmap.EndInit(); // End initialization of the BitmapImage
                ImgThumbnail.Source = bitmap; // Set the source of the thumbnail image to the loaded BitmapImage
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load thumbnail: {ex.Message}");

                try
                {
                    ImgThumbnail.Source = new BitmapImage(new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute)); // Attempt to load the default fallback image if an exception occurs
                }
                catch
                {
                    ImgThumbnail.Source = null; // If even the fallback image fails to load, set the source to null
                }
            }
        }

        // Logic for exit via logo
        private void logoExit(object sender, RoutedEventArgs e)
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

        // Logic fo rback button to return to main menu

        private void gameSelectBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null) // Check if the NavigationService is available
            {
                this.NavigationService.Content = null; // Clear the current content of the NavigationService
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) // Event handler for the Launch Game button click
        {
            if (GameListBox.SelectedItem is Game selectedGame) // Check if a game is selected in the ListBox
            {
                if (string.IsNullOrWhiteSpace(selectedGame.ExecutablePath)) // Check if the executable path is null or empty
                {
                    MessageBox.Show("No game path specified.", "Missing Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fullExecutablePath;
                if (Path.IsPathRooted(selectedGame.ExecutablePath)) // Check if the executable path is an absolute path
                {
                    fullExecutablePath = selectedGame.ExecutablePath; // Use the absolute path directly
                }
                else
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory; // Get the base directory of the application
                    fullExecutablePath = Path.Combine(baseDir, selectedGame.ExecutablePath); // Combine the base directory with the relative path to get the full path
                }

                if (!File.Exists(fullExecutablePath))
                {
                    MessageBox.Show($"Game executable not found at:\n{fullExecutablePath}\n\nMake sure the USB drive or game folder is accessible.", "Game Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    LoadingOverlay.Visibility = Visibility.Visible; // Show the loading overlay while the game is launching

                    var startInfo = new ProcessStartInfo // Create a new ProcessStartInfo instance to launch the game
                    {
                        FileName = fullExecutablePath, // Set the FileName to the full path of the game executable
                        WorkingDirectory = Path.GetDirectoryName(fullExecutablePath),
                        UseShellExecute = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process == null)
                    {
                        MessageBox.Show("The game didn't start.");
                        return;
                    }

                    var stopwatch = Stopwatch.StartNew(); // Start a stopwatch to measure the time taken for the game to run
                    await process.WaitForExitAsync();
                    stopwatch.Stop();

                    LoadingOverlay.Visibility = Visibility.Collapsed; // Hide the loading overlay after the game has exited

                    var owner = Window.GetWindow(this); // Get the owner window of the current page
                    owner?.Activate(); // Activate the owner window to bring it to the foreground

                    var prompt = new PlayerInfoWindow { Owner = owner }; // Create a new instance of the PlayerInfoWindow to prompt for user information
                    if (prompt.ShowDialog() != true)
                        return;

                    int userId = App.Db.InsertUser(prompt.Username, prompt.IsAdult); // Insert the user information into the database and get the user ID
                    int score = 0;
                    App.Db.InsertGameData(userId, selectedGame.Id, score, (int)stopwatch.Elapsed.TotalSeconds); // Insert the game data into the database, including the user ID, game ID, score, and elapsed time
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Something went wrong:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Please select a game to launch first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static string GetJsonFilePath() // Method to get the path of the JSON file that stores the list of installed games
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installedGames.json"); // Return the full path to the installedGames.json file in the application's base directory
        }

        public static void LoadGamesListFromDisk() // Method to load the list of installed games from disk
        {
            string filePath = GetJsonFilePath(); // Get the path of the JSON file that stores the list of installed games

            if (File.Exists(filePath)) // Check if the JSON file exists
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _gamesList = System.Text.Json.JsonSerializer.Deserialize<List<Game>>(json); // Deserialize the JSON content into a list of Game objects

                    if (_gamesList != null && !json.Contains("IsActive", StringComparison.OrdinalIgnoreCase)) // If the JSON does not contain the IsActive property, set all games to active by default
                    {
                        foreach (var g in _gamesList) // Set the IsActive property of each game to true
                        {
                            g.IsActive = true;
                        }
                        SaveGamesList();
                    }
                }
                catch
                {
                    _gamesList = null;
                }
            }

            if (_gamesList == null) // If the list is still null after attempting to load from disk, initialize it with default games
            {
                _gamesList = new List<Game> // Initialize the list of games with default values
                {
                    new Game
                    {
                        Id = 1,
                        Title = "I Spy Water Safety",
                        ImagePath = "/Images/I Spy Water Safety Menu.png",
                        ImageList = new List<string> { "/Images/I Spy Water Safety Menu.png", "/Images/I Spy Water Safety Character Select.png", "/Images/I Spy Water Safety Gameplay.png" },
                        Category = "Water Safety",
                        Description = "Water Safety Game (Will display later)",
                        ExecutablePath = @"Games\Ashley KSU Games\I Spy Water Safety\Team 1\waterfinalbuild\waterfinalbuild\Capstone1.0.exe",
                        IsActive = true
                    },
                    new Game
                    {
                        Id = 2,
                        Title = "Water Resource Game",
                        ImagePath = "/Images/Water Management Menu.png",
                        ImageList = new List<string> { "/Images/Water Management Menu.png", "/Images/Water Management Gameplay.png", "/Images/Water Management Gameplay 2.png", "/Images/Water Management Gameplay 3.png" },
                        Category = "Water Resource",
                        Description = "Water Resource Game (Will display later).",
                        ExecutablePath = @"Games\Ashley KSU Games\Water Resource Game\New Build\Builds\Windows\x64\Water Management - A Flowing Puzzle.exe",
                        IsActive = true
                    },
                    new Game
                    {
                        Id = 3,
                        Title = "Carters Lake Boating Safety",
                        ImagePath = "/Images/A Boating Game Menu.png",
                        ImageList = new List<string> { "/Images/A Boating Game Menu.png", "/Images/A Boating Game Character Select.png", "/Images/A Boating Game Gameplay.png", "/Images/A Boating Game GamePlay 2.png", "/Images/A Boating Game Quiz.png" },
                        Category = "Boat Safety",
                        Description = "Carters Lake Boating Safety game teaches players about safe boating practices on Carters Lake.",
                        ExecutablePath = @"Games\Ashley KSU Games\Carters Lake Boating Safety game folder Hard Copie\Windows\A Boating Game.exe",
                        IsActive = true
                    },
                    new Game
                    {
                        Id = 4,
                        Title = "Drawing The Night Sky",
                        ImagePath = "/Images/Drawing The Night Sky Menu.png",
                        ImageList = new List<string> { "/Images/Drawing The Night Sky Menu.png", "/Images/Drawing The Night Sky Gameplay 1.png", "/Images/Drawing The Night Sky Gameplay 2.png", "/Images/Drawing The Night Sky gameplay 3.png" },
                        Category = "Astrology",
                        Description = "Drawing The Night Sky game helps players learn about constellations and celestial objects.",
                        ExecutablePath = @"Games\Ashley KSU Games\DrawingTheNightSky\DrawingTheNightSky.exe",
                        IsActive = true
                    }
                };

                SaveGamesList();
            }
        }

        public static void SaveGamesList() // Method to save the list of installed games to disk
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(_gamesList, new System.Text.Json.JsonSerializerOptions // Serialize the list of Game objects to JSON format with indented formatting
                {
                    WriteIndented = true
                });
                File.WriteAllText(GetJsonFilePath(), json); // Write the JSON content to the installedGames.json file in the application's base directory
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save games: {ex.Message}");
            }
        }

        public static void RemoveGameByTitle(string title) // Method to remove a game from the list by its title
        {
            if (_gamesList != null)
            {
                var gameToRemove = _gamesList.FirstOrDefault(g => g.Title.Equals(title, StringComparison.OrdinalIgnoreCase)); // Find the game in the list that matches the specified title (case-insensitive)
                if (gameToRemove != null)
                {
                    if (!string.IsNullOrEmpty(gameToRemove.ExecutablePath))
                    {
                        try
                        {
                            string fullPath = Path.IsPathRooted(gameToRemove.ExecutablePath) // Check if the executable path is an absolute path
                                ? gameToRemove.ExecutablePath // Use the absolute path directly
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameToRemove.ExecutablePath); // Combine the application's base directory with the relative path to get the full path

                            string gameDirectory = Path.GetDirectoryName(fullPath); // Get the directory of the game's executable

                            if (!string.IsNullOrEmpty(gameDirectory) && // Check if the game directory is not null or empty
                                gameDirectory.Contains("Games", StringComparison.OrdinalIgnoreCase) &&
                                Directory.Exists(gameDirectory))
                            {
                                foreach (string file in Directory.GetFiles(gameDirectory, "*.*", SearchOption.AllDirectories)) // Iterate through all files in the game directory and its subdirectories
                                {
                                    File.SetAttributes(file, FileAttributes.Normal); // Set the file attributes to normal to ensure that read-only files can be deleted
                                }

                                Directory.Delete(gameDirectory, recursive: true); // Delete the game directory and all its contents recursively
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Could not delete physical game directory: {ex.Message}");
                        }
                    }

                    _gamesList.Remove(gameToRemove); // Remove the game from the list of installed games
                    SaveGamesList(); // Save the updated list of installed games to disk
                }
            }
        }
    }
}