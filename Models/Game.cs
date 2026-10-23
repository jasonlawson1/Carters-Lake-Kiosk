using System.Collections.Generic;
using System.IO;

namespace Capstone_UI.Models
{
    public class Game
    {
        public int Id { get; set; }

        public string Title { get; set; }


        public string Category { get; set; }

        public string Description { get; set; }

        public string ExecutablePath { get; set; }

        public string ImagePath { get; set; }


        public List<string> ImageList { get; set; } = new List<string>();


        public bool IsActive { get; set; } = true;


        public string SafeImagePath // Property to get a valid image path, falling back to a default if necessary
        {
            get
            {
                if (!string.IsNullOrEmpty(ImagePath)) // Check if the primary image path is set
                {
                    if (ImagePath.StartsWith("/Images/", System.StringComparison.OrdinalIgnoreCase)) // If the image path is already a valid relative path, return it
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

                if (ImageList != null && ImageList.Count > 0) // Check if there are any images in the list
                {
                    foreach (var img in ImageList) // Iterate through the image list to find a valid image
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

                return "/Images/DefaultGameMenu.png";
            }
        }
    }
}