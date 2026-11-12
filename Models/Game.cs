using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization; // Make sure this namespace is included
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Capstone_UI.Models
{
    public class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Id { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public string ExecutablePath { get; set; }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    _cachedImagePath = null;
                    _cachedImageSource = null;

                    OnPropertyChanged(nameof(ImagePath));
                    OnPropertyChanged(nameof(SafeImagePath));
                }
            }
        }

        private List<string> _imageList = new List<string>();
        public List<string> ImageList
        {
            get => _imageList;
            set { _imageList = value; OnPropertyChanged(nameof(ImageList)); }
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }

        public bool HasScoreboard { get; set; }

        private string _gameStatus = "Healthy";
        public string GameStatus
        {
            get => _gameStatus;
            set { _gameStatus = value; OnPropertyChanged(nameof(GameStatus)); }
        }

        private string _lastReportDate = "--";
        public string LastReportDate
        {
            get => _lastReportDate;
            set { _lastReportDate = value; OnPropertyChanged(nameof(LastReportDate)); }
        }

        // Ignore cache properties from JSON serialization
        [JsonIgnore]
        private string _cachedImagePath;

        [JsonIgnore]
        private ImageSource _cachedImageSource;

        [JsonIgnore]
        public ImageSource SafeImagePath
        {
            get
            {
                try
                {
                    string path = string.IsNullOrEmpty(ImagePath) ? "Images/DefaultGameMenu.png" : ImagePath;
                    if (path.StartsWith("/")) path = path.Substring(1);
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                    if (_cachedImagePath != fullPath || _cachedImageSource == null)
                    {
                        if (File.Exists(fullPath))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                            bitmap.EndInit();
                            bitmap.Freeze();

                            _cachedImagePath = fullPath;
                            _cachedImageSource = bitmap;
                        }
                        else
                        {
                            _cachedImageSource = null;
                        }
                    }

                    return _cachedImageSource;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}