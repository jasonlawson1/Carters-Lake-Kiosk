namespace Capstone_UI.Models
{
    public class LeaderboardEntry
    {
        // Friendly messages shown instead of a score for games that don't report one
        public static readonly string[] Affirmations =
        {
            "Good Job!",
            "Great Work!",
            "Well Played!",
            "Awesome!",
            "Nice Going!",
            "Way to Go!",
            "Super Star!",
            "Fantastic!"
        };

        public int Rank { get; set; }

        public string Username { get; set; } = "";

        public int? Score { get; set; } // null when the game doesn't report a score

        public bool GameHasScoreboard { get; set; } // Set from the game's HasScoreboard flag

        // Show the score if there is one. Without one: scoreboard games mean the player didn't finish, other games get a friendly message
        public string DisplayScore => Score.HasValue
            ? Score.Value.ToString("N0")
            : GameHasScoreboard ? "Did Not Finish :(" : PickAffirmation();

        // Picks a message from the player's name so the same entry shows the same message every time the leaderboard refreshes
        private string PickAffirmation()
        {
            int total = 0;
            foreach (char c in Username)
                total += c;

            return Affirmations[total % Affirmations.Length];
        }
    }
}
