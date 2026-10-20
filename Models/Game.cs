using System.Collections.Generic;

namespace Capstone_UI.Models
{
    public class Game
    {
        public string Title { get; set; }
        public string ExecutablePath { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public List<string> ImageList { get; set; }
        public bool IsActive { get; set; } = true; // Add this line here
    }
}