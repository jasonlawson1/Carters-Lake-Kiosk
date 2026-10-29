namespace Capstone_UI.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }

        public string Username { get; set; } = "";

        public int? Score { get; set; } // null when the game doesn't report a score

        // Games that report a score show it; games that don't show a friendly message instead
        public string DisplayScore => Score.HasValue ? Score.Value.ToString("N0") : "Good Job!";
    }
}
