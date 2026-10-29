namespace Capstone_UI.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }

        public string Username { get; set; } = "";

        public int Score { get; set; }

        // Most games don't report a real score yet, so every entry shows a friendly message instead.
        // Swap this for Score.ToString() once games pass their scores back to the kiosk.
        public string DisplayScore => "Good Job!";
    }
}
