using Capstone_UI.Models;
using Capstone_UI.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Capstone_UI
{
    public partial class AnalyticsDashboard : Page
    {
        private string _selectedPeriod = "Month";

        private DateTime _referenceDate = DateTime.Now;

        public AnalyticsDashboard()
        {
            InitializeComponent();

            UpdatePeriodButtonStyles();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            DateTime startDate = GetPeriodStart();
            DateTime endDate = GetPeriodEnd();

            DashboardSummary summary =
                MockAnalyticsService.GetDashboardSummary(
                    startDate,
                    endDate);

            TotalSessionsText.Text =
                summary.TotalSessions.ToString("N0");

            PopularGameText.Text =
                summary.MostPopularGame;

            AveragePlaytimeText.Text =
                summary.AveragePlaytimeDisplay;

            CompletionRateText.Text =
                summary.CompletionRateDisplay;

            ActivityChart.ItemsSource =
                MockAnalyticsService.GetActivityData(
                    _selectedPeriod,
                    _referenceDate);

            PeriodLabel.Text =
                GetPeriodLabel();

            ActivitySubtitle.Text =
                GetActivitySubtitle();
        }

        private void PeriodButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            _selectedPeriod =
                button.Content?.ToString() ?? "Month";

            _referenceDate =
                DateTime.Now;

            UpdatePeriodButtonStyles();
            LoadDashboard();
        }

        private void PreviousPeriod_Click(
            object sender,
            RoutedEventArgs e)
        {
            _referenceDate =
                _selectedPeriod switch
                {
                    "Day" =>
                        _referenceDate.AddDays(-1),

                    "Week" =>
                        _referenceDate.AddDays(-7),

                    "Quarter" =>
                        _referenceDate.AddMonths(-3),

                    "Year" =>
                        _referenceDate.AddYears(-1),

                    _ =>
                        _referenceDate.AddMonths(-1)
                };

            LoadDashboard();
        }

        private void NextPeriod_Click(
            object sender,
            RoutedEventArgs e)
        {
            DateTime nextPeriod =
                _selectedPeriod switch
                {
                    "Day" =>
                        _referenceDate.AddDays(1),

                    "Week" =>
                        _referenceDate.AddDays(7),

                    "Quarter" =>
                        _referenceDate.AddMonths(3),

                    "Year" =>
                        _referenceDate.AddYears(1),

                    _ =>
                        _referenceDate.AddMonths(1)
                };

            if (nextPeriod > DateTime.Now)
            {
                return;
            }

            _referenceDate =
                nextPeriod;

            LoadDashboard();
        }

        private void GamePopularity_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new GamePopularityPage(
                    GetPeriodStart(),
                    GetPeriodEnd()));
        }

        private void EducationalCategories_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new EducationalCategoriesPage(
                    GetPeriodStart(),
                    GetPeriodEnd()));
        }

        private void UpdatePeriodButtonStyles()
        {
            List<Button> buttons =
                new List<Button>
                {
                    DayButton,
                    WeekButton,
                    MonthButton,
                    QuarterButton,
                    YearButton
                };

            foreach (Button button in buttons)
            {
                bool isSelected =
                    button.Content?.ToString() ==
                    _selectedPeriod;

                if (isSelected)
                {
                    button.Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                30,
                                86,
                                49));

                    button.Foreground =
                        Brushes.White;
                }
                else
                {
                    button.Background =
                        Brushes.White;

                    button.Foreground =
                        Brushes.Black;
                }
            }
        }

        private DateTime GetPeriodStart()
        {
            if (_selectedPeriod == "Day")
            {
                return _referenceDate.Date;
            }

            if (_selectedPeriod == "Week")
            {
                int dayOffset =
                    ((int)_referenceDate.DayOfWeek + 6)
                    % 7;

                return _referenceDate
                    .Date
                    .AddDays(-dayOffset);
            }

            if (_selectedPeriod == "Quarter")
            {
                int quarterStartMonth =
                    ((_referenceDate.Month - 1) / 3)
                    * 3 + 1;

                return new DateTime(
                    _referenceDate.Year,
                    quarterStartMonth,
                    1);
            }

            if (_selectedPeriod == "Year")
            {
                return new DateTime(
                    _referenceDate.Year,
                    1,
                    1);
            }

            return new DateTime(
                _referenceDate.Year,
                _referenceDate.Month,
                1);
        }

        private DateTime GetPeriodEnd()
        {
            DateTime startDate =
                GetPeriodStart();

            return _selectedPeriod switch
            {
                "Day" =>
                    startDate.AddDays(1),

                "Week" =>
                    startDate.AddDays(7),

                "Quarter" =>
                    startDate.AddMonths(3),

                "Year" =>
                    startDate.AddYears(1),

                _ =>
                    startDate.AddMonths(1)
            };
        }

        private string GetPeriodLabel()
        {
            if (_selectedPeriod == "Day")
            {
                return _referenceDate
                    .ToString("MMMM d, yyyy");
            }

            if (_selectedPeriod == "Week")
            {
                DateTime startDate =
                    GetPeriodStart();

                DateTime endDate =
                    startDate.AddDays(6);

                return
                    $"{startDate:MMM d} - " +
                    $"{endDate:MMM d, yyyy}";
            }

            if (_selectedPeriod == "Quarter")
            {
                int quarter =
                    ((_referenceDate.Month - 1) / 3)
                    + 1;

                return
                    $"Q{quarter} " +
                    $"{_referenceDate.Year}";
            }

            if (_selectedPeriod == "Year")
            {
                return _referenceDate
                    .Year
                    .ToString();
            }

            return _referenceDate
                .ToString("MMMM yyyy");
        }

        private string GetActivitySubtitle()
        {
            return _selectedPeriod switch
            {
                "Day" =>
                    "Sessions by Hour",

                "Week" =>
                    "Sessions by Day",

                "Month" =>
                    "Sessions by Day",

                "Quarter" =>
                    "Sessions by Week",

                "Year" =>
                    "Sessions by Month",

                _ =>
                    "Sessions"
            };
        }
    }
}
