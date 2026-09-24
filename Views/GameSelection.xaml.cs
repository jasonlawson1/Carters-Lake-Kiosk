using Capstone_UI.Models;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Capstone_UI
{
    public partial class GameSelection : Page
    {
        public List<Game> GamesList { get; set; } // List holding the games info to be displayed in the GameSelection page

        private DispatcherTimer _imageCycleTimer; // Timer to cycle through images for the selected game
        private int _currentImageIndex = 0; // Index to track the current image being displayed for the selected game

        public GameSelection() 
        {
            InitializeComponent(); //Loads UI elements onto screen. DO NOT MOVE AND DO NOT DELETE!

            //3 second timer to cycle through images for the selected game
            _imageCycleTimer = new DispatcherTimer();
            _imageCycleTimer.Interval = TimeSpan.FromSeconds(3);
            _imageCycleTimer.Tick += ImageCycleTimer_Tick;

            // Register page lifecycle events for safe startup and resource cleanup
            this.Loaded += GameSelection_Loaded;
            this.Unloaded += GameSelection_Unloaded;
        }

        private void GameSelection_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Adds the games to the list for display in the GameSelection page. Fills in the Title, ImagePath, Category, Description, ExecutablePath, and ImageList for each game.
            //Consult Reese if you need to add more games to the list. Make sure to add the game folder to the Games folder in the project directory.
            //See Game.cs for object attributes. Any changes to Game.cs object attributes WILL BREAK GAMESLIST. Ensure GamesList is updated correctly with required changes to Game.cs! 
            GamesList = new List<Game>
            {
                new Game
                {
                    Title = "I Spy Water Safety",
                    ImagePath = "/Images/I Spy Water Safety Menu.png",
                    ImageList = new List<string> {"/Images/I Spy Water Safety Menu.png","/Images/I Spy Water Safety Character Select.png","/Images/I Spy Water Safety Gameplay.png"},
                    Category = "Water Safety",
                    Description = "Water Safety Game (Will display later)",
                    ExecutablePath = @"Games\Ashley KSU Games\I Spy Water Safety\Team 1\waterfinalbuild\waterfinalbuild\Capstone1.0.exe"
                },
                new Game
                {
                    Title = "Water Resource Game",
                    ImagePath = "/Images/Water Management Menu.png",
                    ImageList = new List<string> {"/Images/Water Management Menu.png","/Images/Water Management Gameplay.png","/Images/Water Management Gameplay 2.png","/Images/Water Management Gameplay 3.png" },
                    Category = "Water Resource",
                    Description = "Water Resource Game (Will display later).",
                    ExecutablePath = @"Games\Ashley KSU Games\Water Resource Game\New Build\Builds\Windows\x64\Water Management - A Flowing Puzzle.exe"
                },
                new Game
                {
                    Title = "Carters Lake Boating Safety",
                    ImagePath = "/Images/A Boating Game Menu.png",
                    ImageList = new List<string> {"/Images/A Boating Game Menu.png","/Images/A Boating Game Character Select.png","/Images/A Boating Game Gameplay.png","/Images/A Boating Game GamePlay 2.png","/Images/A Boating Game Quiz.png"},
                    Category = "Boat Safety",
                    Description = "Carters Lake Boating Safety game teaches players about safe boating practices on Carters Lake.",
                    ExecutablePath = @"Games\Ashley KSU Games\Carters Lake Boating Safety game folder Hard Copie\Windows\A Boating Game.exe"
                },
                new Game
                {
                    Title = "Drawing The Night Sky",
                    ImagePath = "/Images/Drawing The Night Sky Menu.png",
                    ImageList = new List<string> {"/Images/Drawing The Night Sky Menu.png","/Images/Drawing The Night Sky Gameplay 1.png","/Images/Drawing The Night Sky Gameplay 2.png","/Images/Drawing The Night Sky gameplay 3.png" },
                    Category = "Astrology",
                    Description = "Drawing The Night Sky game helps players learn about constellations and celestial objects.",
                    ExecutablePath = @"Games\Ashley KSU Games\DrawingTheNightSky\DrawingTheNightSky.exe"
                }
            };

            // 2. Bind the list to the ListBox
            GameListBox.ItemsSource = GamesList;

            // 3. Select top item by default
            if (GamesList.Count > 0)
            {
                GameListBox.SelectedIndex = 0;
            }
        }

        // Stop the timer when the page is unloaded to prevent it from running in the background
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

                // Stop any running timer and reset the image index
                _imageCycleTimer.Stop();
                _currentImageIndex = 0;

                // Display the primary image immediately
                UpdateThumbnailImage(selectedGame);

                // Start timer if there are multiple images to cycle through
                if (selectedGame.ImageList != null && selectedGame.ImageList.Count > 1)
                {
                    _imageCycleTimer.Start();
                }
            }
        }

        private void ImageCycleTimer_Tick(object sender, EventArgs e) // Event handler for the timer tick to cycle through images
        {
            if (GameListBox.SelectedItem is Game selectedGame && selectedGame.ImageList != null && selectedGame.ImageList.Count > 0)
            {
                // Advance to the next image and loop around
                _currentImageIndex = (_currentImageIndex + 1) % selectedGame.ImageList.Count;
                UpdateThumbnailImage(selectedGame);
            }
        }

        private void UpdateThumbnailImage(Game game) // Method to update the thumbnail image based on the current image index
        {
            try
            {
                string imageToLoad = game.ImagePath;

                if(game.ImageList != null && game.ImageList.Count > _currentImageIndex)
                {
                    imageToLoad = game.ImageList[_currentImageIndex];
                }
                string cleanPath = imageToLoad.TrimStart('/');

                Uri resourceURI = new Uri ($"pack://application:,,,/{cleanPath}", UriKind.Absolute);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = resourceURI;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImgThumbnail.Source = bitmap;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
                ImgThumbnail.Source = null; // Clear the image if loading fails
            }
        }

        private void logoExit(object sender, RoutedEventArgs e) // Event handler for the exit logo button click
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit the program?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void gameSelectBackButton_Click(object sender, RoutedEventArgs e) // Button for returning back to Main Menu
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Content = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) //Play button properties. Launches game on click. Throws error if game is unavailable.
        {
            if (GameListBox.SelectedItem is Game selectedGame)
            {
                if (string.IsNullOrWhiteSpace(selectedGame.ExecutablePath))
                {
                    MessageBox.Show("No game path specified.", "Missing Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullExecutablePath = System.IO.Path.Combine(baseDir, selectedGame.ExecutablePath);

                if (!File.Exists(fullExecutablePath))
                {
                    MessageBox.Show($"Game executable not found at:\n{fullExecutablePath}\n\nMake sure the game folder exists inside the Games folder.", "Game Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = fullExecutablePath,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(fullExecutablePath),
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not launch game:\n{ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a game to launch first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}