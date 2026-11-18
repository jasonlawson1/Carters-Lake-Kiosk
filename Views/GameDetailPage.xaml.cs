using Capstone_UI.Models;
using Capstone_UI.Services;
using System.Windows;
using System.Windows.Controls;

namespace Capstone_UI
{
    public partial class GameDetailPage : Page
    {
        public GameDetailPage(
            int gameId,
            string gameName)
        {
            InitializeComponent();

            GameDetailSummary details =
                MockAnalyticsService.GetGameDetails(
                    gameId,
                    gameName);

            GameNameText.Text =
                details.GameName;

            SessionsText.Text =
                details.TotalSessions.ToString("N0");

            AverageText.Text =
                details.AveragePlaytimeDisplay;

            CompletionText.Text =
                details.CompletionRateDisplay;

            BestScoreText.Text =
                details.BestScoreDisplay;

            OutcomeText.Text =
                $"Completed: {details.Completed}\n" +
                $"Abandoned: {details.Abandoned}\n" +
                $"Timeout: {details.Timeout}\n" +
                $"Technical Issues: {details.TechnicalIssues}\n" +
                $"Other Exits: {details.OtherExits}";
        }
    }
}
