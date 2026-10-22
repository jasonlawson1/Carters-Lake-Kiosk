using System.Collections.Generic;
using System.IO;

namespace Capstone_UI.Models
{
    /// <summary>
    /// Represents an individual interactive kiosk game, including its metadata, file paths, 
    /// display properties, and fallback image safety logic.
    /// </summary>
    public class Game
    {
        public int Id { get; set; }
        // ==========================================
        // Metadata & Descriptive Properties
        // ==========================================

        /// <summary>
        /// Gets or sets the display title of the game.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the category or classification of the game (e.g., "Water Safety", "Boat Safety").
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the detailed summary or description of the game.
        /// </summary>
        public string Description { get; set; }


        // ==========================================
        // File System & Launch Paths
        // ==========================================

        /// <summary>
        /// Gets or sets the relative or absolute file path to the game's executable (.exe).
        /// </summary>
        public string ExecutablePath { get; set; }


        // ==========================================
        // Image & Thumbnail Management
        // ==========================================

        /// <summary>
        /// Gets or sets the primary image path or resource URI used for the game thumbnail.
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Gets or sets a collection of image paths used for cycling preview thumbnails.
        /// </summary>
        public List<string> ImageList { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the game is currently active and visible to kiosk visitors.
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ==========================================
        // Computed Properties
        // ==========================================

        /// <summary>
        /// Gets a safe image path. Automatically validates primary or secondary image files 
        /// against the disk; falls back to the default menu image if files are missing or deleted.
        /// </summary>
        public string SafeImagePath
        {
            get
            {
                // Tier 1: Check if the primary ImagePath is valid and exists on disk (or is a resource URI)
                if (!string.IsNullOrEmpty(ImagePath))
                {
                    // Built-in resource check
                    if (ImagePath.StartsWith("/Images/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return ImagePath;
                    }

                    // Local disk file existence check
                    string cleanPath = ImagePath.TrimStart('/', '\\');
                    string fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cleanPath);
                    if (File.Exists(fullPath))
                    {
                        return ImagePath;
                    }
                }

                // Tier 2: Check if any alternative image in the ImageList exists on disk or as a resource
                if (ImageList != null && ImageList.Count > 0)
                {
                    foreach (var img in ImageList)
                    {
                        if (!string.IsNullOrEmpty(img))
                        {
                            if (img.StartsWith("/Images/", System.StringComparison.OrdinalIgnoreCase))
                            {
                                return img;
                            }

                            string cleanPath = img.TrimStart('/', '\\');
                            string fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                            if (File.Exists(fullPath))
                            {
                                return img;
                            }
                        }
                    }
                }

                // Tier 3: Ultimate Fallback if all custom images are missing, invalid, or deleted
                return "/Images/DefaultGameMenu.png";
            }
        }
    }
}