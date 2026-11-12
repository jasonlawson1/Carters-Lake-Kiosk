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
using Capstone_UI.Services;

namespace Capstone_UI
{
    public partial class GameSelection : Page
    {
        private static List<Game> _gamesList; // Static field to hold the list of games

        public static List<Game> GamesList // Static property to access the list of games
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

        private DispatcherTimer _imageCycleTimer;
        private int _currentImageIndex = 0;

        private const string DEFAULT_FALLBACK_IMAGE = "pack://application:,,,/Images/DefaultGameMenu.png";

        public GameSelection()
        {
            InitializeComponent();

            _imageCycleTimer = new DispatcherTimer();
            _imageCycleTimer.Interval = TimeSpan.FromSeconds(3);
            _imageCycleTimer.Tick += ImageCycleTimer_Tick;

            this.Loaded += GameSelection_Loaded;
            this.Unloaded += GameSelection_Unloaded;
        }

        public static void RefreshActiveGamesList()
        {
            LoadGamesListFromDisk();
        }

        private void GameSelection_Loaded(object sender, RoutedEventArgs e)
        {
            if (_gamesList == null)
            {
                LoadGamesListFromDisk();
            }

            foreach (var game in GamesList)
                App.Db.EnsureGame(game.Id, game.Title);

            var activeGames = GamesList.Where(g => g.IsActive).ToList();

            GameListBox.ItemsSource = null;
            GameListBox.ItemsSource = activeGames;

            if (activeGames.Count > 0 && GameListBox.SelectedItem == null)
            {
                GameListBox.SelectedIndex = 0;
            }
            else if (GameListBox.SelectedItem is Game selectedGame)
            {
                UpdateThumbnailImage(selectedGame);
            }
        }

        private void GameSelection_Unloaded(object sender, RoutedEventArgs e)
        {
            _imageCycleTimer.Stop();
        }

        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem is Game selectedGame)
            {
                TxtTitle.Text = selectedGame.Title;
                TxtCategory.Text = selectedGame.Category;
                TxtDescription.Text = selectedGame.Description;

                _imageCycleTimer.Stop();
                _currentImageIndex = 0;

                UpdateThumbnailImage(selectedGame);
                LoadLeaderboard(selectedGame);

                if (selectedGame.ImageList != null && selectedGame.ImageList.Count > 1)
                {
                    _imageCycleTimer.Start();
                }
            }
        }

        private void ImageCycleTimer_Tick(object sender, EventArgs e)
        {
            if (GameListBox.SelectedItem is Game selectedGame && selectedGame.ImageList != null && selectedGame.ImageList.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex + 1) % selectedGame.ImageList.Count;
                UpdateThumbnailImage(selectedGame);
            }
        }

        private void UpdateThumbnailImage(Game game)
        {
            try
            {
                string imageToLoad = game.ImagePath; // Default to primary image

                if (game.ImageList != null && game.ImageList.Count > 1 && _currentImageIndex > 0 && _currentImageIndex < game.ImageList.Count)
                {
                    imageToLoad = game.ImageList[_currentImageIndex]; // Use cycling index if active
                }

                if (!string.IsNullOrEmpty(imageToLoad))
                {
                    string cleanPath = imageToLoad.TrimStart('/', '\\');
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                    if (File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load fully into RAM
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.EndInit();
                        bitmap.Freeze(); // Release file lock instantly

                        ImgThumbnail.Source = bitmap;
                        return;
                    }
                }

                // Fallback if path is empty or file doesn't exist
                ImgThumbnail.Source = new BitmapImage(new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load thumbnail: {ex.Message}");
                ImgThumbnail.Source = new BitmapImage(new Uri(DEFAULT_FALLBACK_IMAGE, UriKind.Absolute));
            }
        }

        private void LoadLeaderboard(Game game)
        {
            try
            {
                var entries = App.Db.GetLeaderboard(game.Id);
                foreach (var entry in entries)
                    entry.GameHasScoreboard = game.HasScoreboard;
                LeaderboardList.ItemsSource = entries;
                TxtLeaderboardEmpty.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load leaderboard: {ex.Message}");
                LeaderboardList.ItemsSource = null;
                TxtLeaderboardEmpty.Visibility = Visibility.Visible;
            }
        }

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
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedItem is Game selectedGame)
            {
                if (string.IsNullOrWhiteSpace(selectedGame.ExecutablePath))
                {
                    MessageBox.Show("No game path specified.", "Missing Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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
                    LoadingOverlay.Visibility = Visibility.Visible;

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = fullExecutablePath,
                        WorkingDirectory = Path.GetDirectoryName(fullExecutablePath),
                        UseShellExecute = true
                    };

                    var launchedAt = DateTimeOffset.UtcNow;
                    using var process = Process.Start(startInfo);
                    if (process == null)
                    {
                        MessageBox.Show("The game didn't start.");
                        return;
                    }

                    var stopwatch = Stopwatch.StartNew();
                    await process.WaitForExitAsync();
                    stopwatch.Stop();

                    LoadingOverlay.Visibility = Visibility.Collapsed;

                    var owner = Window.GetWindow(this);
                    owner?.Activate();

                    var prompt = new PlayerInfoWindow { Owner = owner };
                    if (prompt.ShowDialog() != true)
                        return;

                    int userId = App.Db.InsertUser(prompt.Username, prompt.IsAdult);
                    int? score = LiveScoreReader.TryReadScore(launchedAt);
                    App.Db.InsertGameData(userId, selectedGame.Id, score, (int)stopwatch.Elapsed.TotalSeconds);

                    LoadLeaderboard(selectedGame);
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

        private static string GetJsonFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installedGames.json");
        }

        public static void LoadGamesListFromDisk()
        {
            string filePath = GetJsonFilePath();

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _gamesList = System.Text.Json.JsonSerializer.Deserialize<List<Game>>(json);

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

            if (_gamesList == null)
            {
                _gamesList = new List<Game>();
                SaveGamesList();
            }
        }

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