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
    /// <summary>
    /// Interaction logic for GameSelection.xaml. Manages the kiosk game library display, 
    /// active filtering, live thumbnail slideshow animations, executable launching, and JSON data persistence.
    /// </summary>
    public partial class GameSelection : Page
    {
        // ==========================================
        // Static Data Store & Property
        // ==========================================

        private static List<Game> _gamesList;

        /// <summary>
        /// Gets or sets the global list of installed games, automatically loading from disk on first access.
        /// </summary>
        public static List<Game> GamesList
        {
            get
            {
                if (_gamesList == null)
                {
                    LoadGamesListFromDisk();
                }
                return _gamesList;
            }
            set { _gamesList = value; }
        }

        // ==========================================
        // Instance Variables & Constants
        // ==========================================

        private DispatcherTimer _imageCycleTimer;
        private int _currentImageIndex = 0;

        // Path constant pointing to the prebundled default fallback image resource
        private const string DEFAULT_FALLBACK_IMAGE = "pack://application:,,,/Images/DefaultGameMenu.png";


        // ==========================================
        // Constructor & Page Lifecycle
        // ==========================================

        public GameSelection()
        {
            InitializeComponent();

            // Initialize the slideshow timer for cycling game preview thumbnails every 3 seconds
            _imageCycleTimer = new DispatcherTimer();
            _imageCycleTimer.Interval = TimeSpan.FromSeconds(3);
            _imageCycleTimer.Tick += ImageCycleTimer_Tick;

            // Wire up page navigation events
            this.Loaded += GameSelection_Loaded;
            this.Unloaded += GameSelection_Unloaded;
        }

        private void GameSelection_Loaded(object sender, RoutedEventArgs e)
        {
            // Force a reload from disk every time the page loads to stay in sync with administrative updates
            LoadGamesListFromDisk();
            
            foreach (var game in GamesList)
                App.Db.EnsureGame(game.Id, game.Title);
            // Only display games that are marked as active/visible for kiosk visitors

            // Filter the library to display only games marked active and visible for kiosk visitors
            var activeGames = GamesList.Where(g => g.IsActive).ToList();

            GameListBox.ItemsSource = null;
            GameListBox.ItemsSource = activeGames;

            // Automatically select the first game if none are currently highlighted
            if (activeGames.Count > 0 && GameListBox.SelectedItem == null)
            {
                GameListBox.SelectedIndex = 0;
            }
        }

        private void GameSelection_Unloaded(object sender, RoutedEventArgs e)
        {
            // Stop the image cycle timer when leaving the page to save background resources
            _imageCycleTimer.Stop();
        }


        // ==========================================
        // Selection & Thumbnail Slideshow Logic
        // ==========================================

        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem is Game selectedGame)
            {
                // Update text metadata fields
                TxtTitle.Text = selectedGame.Title;
                TxtCategory.Text = selectedGame.Category;
                TxtDescription.Text = selectedGame.Description;

                // Reset and restart the thumbnail cycle timer for the newly selected game
                _imageCycleTimer.Stop();
                _currentImageIndex = 0;

                UpdateThumbnailImage(selectedGame);

                // Start slideshow timer only if there are multiple preview images available
                if (selectedGame.ImageList != null && selectedGame.ImageList.Count > 1)
                {
                    _imageCycleTimer.Start();
                }
            }
        }

        private void ImageCycleTimer_Tick(object sender, EventArgs e)
        {
            // Advance to the next image index in the sequence and refresh thumbnail display
            if (GameListBox.SelectedItem is Game selectedGame && selectedGame.ImageList != null && selectedGame.ImageList.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex + 1) % selectedGame.ImageList.Count;
                UpdateThumbnailImage(selectedGame);
            }
        }

        /// <summary>
        /// Updates the displayed thumbnail image. Safely falls back to the default fallback image 
        /// if files are missing on disk or invalid.
        /// </summary>
        private void UpdateThumbnailImage(Game game)
        {
            try
            {
                string imageToLoad = game.ImagePath;

                if (game.ImageList != null && game.ImageList.Count > _currentImageIndex)
                {
                    imageToLoad = game.ImageList[_currentImageIndex];
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                if (!string.IsNullOrEmpty(imageToLoad) && imageToLoad.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase))
                {
                    // Built-in compiled resource pack URI reference
                    string cleanPath = imageToLoad.TrimStart('/');
                    bitmap.UriSource = new Uri($"pack://application:,,,/{cleanPath}", UriKind.Absolute);
                }
                else if (!string.IsNullOrEmpty(imageToLoad))
                {
                    // Local physical file path on disk
                    string cleanPath = imageToLoad.TrimStart('/', '\\');
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                    if (File.Exists(fullPath))
                    {
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    }
                    else
                    {
                        // Fallback: Physical file was deleted or missing
                        bitmap.UriSource = new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute);
                    }
                }
                else
                {
                    // Fallback: Path string was null or empty
                    bitmap.UriSource = new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute);
                }

                bitmap.EndInit();
                ImgThumbnail.Source = bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load thumbnail: {ex.Message}");

                // Hard catch-all safety net: Load default image directly
                try
                {
                    ImgThumbnail.Source = new BitmapImage(new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute));
                }
                catch
                {
                    ImgThumbnail.Source = null;
                }
            }
        }


        // ==========================================
        // Navigation & Administrative Actions
        // ==========================================

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

        private void gameSelectBackButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear current frame navigation content to go back
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }


        // ==========================================
        // Game Launch Execution Logic
        // ==========================================

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedItem is Game selectedGame)
            {
                if (string.IsNullOrWhiteSpace(selectedGame.ExecutablePath))
                {
                    MessageBox.Show("No game path specified.", "Missing Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Resolve absolute or relative executable paths
                string fullExecutablePath;
                if (Path.IsPathRooted(selectedGame.ExecutablePath))
                {
                    fullExecutablePath = selectedGame.ExecutablePath;
                }
                else
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    fullExecutablePath = Path.Combine(baseDir, selectedGame.ExecutablePath);
                }

                if (!File.Exists(fullExecutablePath))
                {
                    MessageBox.Show($"Game executable not found at:\n{fullExecutablePath}\n\nMake sure the USB drive or game folder is accessible.", "Game Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    // Show visual loading overlay while launching process initializes
                    LoadingOverlay.Visibility = Visibility.Visible;

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = fullExecutablePath,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(fullExecutablePath),
                        UseShellExecute = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process == null)
                    {
                        MessageBox.Show("The game didn't start.");
                        return;
                    }

                    var stopwatch = Stopwatch.StartNew();
                    await process.WaitForExitAsync();   // waits here while they play
                    stopwatch.Stop();
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = fullExecutablePath,
                            WorkingDirectory = Path.GetDirectoryName(fullExecutablePath),
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                        Task.Delay(1500).Wait(); // Brief pause for smooth transition
                    });

                    LoadingOverlay.Visibility = Visibility.Collapsed;

                    // Game is over: bring the kiosk back to the front, then ask for their info
                    var owner = Window.GetWindow(this);
                    owner?.Activate();

                    var prompt = new PlayerInfoWindow { Owner = owner };
                    if (prompt.ShowDialog() != true)
                        return;   // they skipped the prompt, so nothing is saved

                    int userId = App.Db.InsertUser(prompt.Username, prompt.IsAdult);
                    int score = 0;   // placeholder until you decide how games report scores
                    App.Db.InsertGameData(userId, selectedGame.Id, score, (int)stopwatch.Elapsed.TotalSeconds);
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


        // ==========================================
        // JSON Disk Storage & Management
        // ==========================================

        private static string GetJsonFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installedGames.json");
        }

        /// <summary>
        /// Loads the game library from the local `installedGames.json` file. 
        /// Initializes default games if the file does not exist or is corrupted.
        /// </summary>
        public static void LoadGamesListFromDisk()
        {
            string filePath = GetJsonFilePath();

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _gamesList = System.Text.Json.JsonSerializer.Deserialize<List<Game>>(json);

                    // Migration helper: Ensure IsActive property defaults to true if missing from older JSON records
                    if (_gamesList != null && !json.Contains("IsActive", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var g in _gamesList)
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

            // Fallback initialization if JSON file is missing or failed to parse
            if (_gamesList == null)
            {
                _gamesList = new List<Game>
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

        /// <summary>
        /// Serializes the current in-memory game collection to JSON and saves it to disk.
        /// </summary>
        public static void SaveGamesList()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(_gamesList, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(GetJsonFilePath(), json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save games: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a game by title, clears read-only file attributes, and recursively deletes 
        /// its physical asset directory from disk.
        /// </summary>
        public static void RemoveGameByTitle(string title)
        {
            if (_gamesList != null)
            {
                var gameToRemove = _gamesList.FirstOrDefault(g => g.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (gameToRemove != null)
                {
                    if (!string.IsNullOrEmpty(gameToRemove.ExecutablePath))
                    {
                        try
                        {
                            string fullPath = Path.IsPathRooted(gameToRemove.ExecutablePath)
                                ? gameToRemove.ExecutablePath
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameToRemove.ExecutablePath);

                            string gameDirectory = Path.GetDirectoryName(fullPath);

                            if (!string.IsNullOrEmpty(gameDirectory) &&
                                gameDirectory.Contains("Games", StringComparison.OrdinalIgnoreCase) &&
                                Directory.Exists(gameDirectory))
                            {
                                // Reset file attributes in case any files were marked read-only
                                foreach (string file in Directory.GetFiles(gameDirectory, "*.*", SearchOption.AllDirectories))
                                {
                                    File.SetAttributes(file, FileAttributes.Normal);
                                }

                                Directory.Delete(gameDirectory, recursive: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Could not delete physical game directory: {ex.Message}");
                        }
                    }

                    _gamesList.Remove(gameToRemove);
                    SaveGamesList();
                }
            }
        }
    }
}