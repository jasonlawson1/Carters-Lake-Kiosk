using System.Collections.Generic;
using System.IO;

namespace Capstone_UI.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ExecutablePath { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public List<string> ImageList { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;

        // SAFE IMAGE PROPERTY: Automatically handles missing or empty thumbnails
        public string SafeImagePath
        {
            get
            {
                // 1. Check if the primary ImagePath is valid and exists on disk (or is a resource)
                if (!string.IsNullOrEmpty(ImagePath))
                {
                    if (ImagePath.StartsWith("/Images/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return ImagePath;
                    }

                    string cleanPath = ImagePath.TrimStart('/', '\\');
                    string fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cleanPath);
                    if (File.Exists(fullPath))
                    {
                        return ImagePath;
                    }
                }

                // 2. Check if any image in the ImageList exists
                if (ImageList != null && ImageList.Count > 0)
                {
                    foreach (var img in ImageList)
                    {
                        if (!string.IsNullOrEmpty(img))
                        {
                            if (img.StartsWith("/Images/", System.StringComparison.OrdinalIgnoreCase)) return img;

                            string cleanPath = img.TrimStart('/', '\\');
                            string fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cleanPath);
                            if (File.Exists(fullPath)) return img;
                        }
                    }
                }

                // 3. Ultimate Fallback if everything else is missing or deleted
                return "/Images/DefaultGameMenu.png";
            }
        }
    }
}