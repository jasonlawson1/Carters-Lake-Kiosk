using Capstone_UI.Models;
using Capstone_UI.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Capstone_UI
{
    public partial class GamePopularityPage : Page
    {
        public GamePopularityPage(
            DateTime startDate,
            DateTime endDate)
        {
            InitializeComponent();

            GamePopularityList.ItemsSource =
                MockAnalyticsService.GetGamePopularity(
                    startDate,
                    endDate);
        }

        private void GameRow_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (button.DataContext
                is not GamePopularityItem game)
            {
                return;
            }

            NavigationService?.Navigate(
                new GameDetailPage(
                    game.GameId,
                    game.GameName));
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }
    }
}
