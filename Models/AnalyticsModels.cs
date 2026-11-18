using System;
using System.Collections.Generic;

namespace Capstone_UI.Models
{
    public class DashboardSummary
    {
        public int TotalSessions { get; set; }
        public string MostPopularGame { get; set; } = "";
        public double AveragePlaytimeSeconds { get; set; }
        public double CompletionRate { get; set; }

        public string AveragePlaytimeDisplay
        {
            get
            {
                int totalSeconds = (int)Math.Round(AveragePlaytimeSeconds);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                return $"{minutes}m {seconds}s";
            }
        }

        public string CompletionRateDisplay
        {
            get
            {
                return $"{CompletionRate:P0}";
            }
        }
    }

    public class ActivityPoint
    {
        public string Label { get; set; } = "";
        public int SessionCount { get; set; }
        public double BarHeight { get; set; }
    }

    public class GamePopularityItem
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = "";
        public int SessionCount { get; set; }
        public bool IsEnabled { get; set; }
        public double BarWidth { get; set; }

        public string StatusDisplay
        {
            get
            {
                return IsEnabled ? "Enabled" : "Disabled";
            }
        }
    }

    public class CategoryUsageItem
    {
        public string CategoryName { get; set; } = "";
        public int SessionCount { get; set; }
        public double BarWidth { get; set; }
        public List<string> Games { get; set; } = new List<string>();

        public string GamesDisplay
        {
            get
            {
                if (Games.Count == 0)
                {
                    return "No games";
                }

                return string.Join(", ", Games);
            }
        }
    }

    public class GameDetailSummary
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = "";
        public int TotalSessions { get; set; }
        public double AveragePlaytimeSeconds { get; set; }
        public double CompletionRate { get; set; }
        public int? BestScore { get; set; }
        public int Completed { get; set; }
        public int Abandoned { get; set; }
        public int Timeout { get; set; }
        public int TechnicalIssues { get; set; }
        public int OtherExits { get; set; }

        public string AveragePlaytimeDisplay
        {
            get
            {
                int totalSeconds = (int)Math.Round(AveragePlaytimeSeconds);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                return $"{minutes}m {seconds}s";
            }
        }

        public string CompletionRateDisplay
        {
            get
            {
                return $"{CompletionRate:P0}";
            }
        }

        public string BestScoreDisplay
        {
            get
            {
                return BestScore.HasValue
                    ? BestScore.Value.ToString()
                    : "N/A";
            }
        }
    }
}
