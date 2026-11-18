using Capstone_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capstone_UI.Services
{
    public static class MockAnalyticsService
    {
        public static DashboardSummary GetDashboardSummary(
            DateTime startDate,
            DateTime endDate)
        {
            return new DashboardSummary
            {
                TotalSessions = 1284,
                MostPopularGame = "I Spy Water Safety",
                AveragePlaytimeSeconds = 256,
                CompletionRate = 0.78
            };
        }

        public static List<ActivityPoint> GetActivityData(
            string period,
            DateTime referenceDate)
        {
            List<ActivityPoint> points = new List<ActivityPoint>();

            Random random = new Random(
                referenceDate.Year +
                referenceDate.Month +
                referenceDate.Day +
                period.Length);

            switch (period)
            {
                case "Day":
                    for (int hour = 9; hour <= 16; hour++)
                    {
                        points.Add(
                            new ActivityPoint
                            {
                                Label = FormatHour(hour),
                                SessionCount = random.Next(5, 24)
                            });
                    }

                    break;

                case "Week":
                    string[] days =
                    {
                        "Mon",
                        "Tue",
                        "Wed",
                        "Thu",
                        "Fri",
                        "Sat",
                        "Sun"
                    };

                    foreach (string day in days)
                    {
                        points.Add(
                            new ActivityPoint
                            {
                                Label = day,
                                SessionCount = random.Next(15, 75)
                            });
                    }

                    break;

                case "Quarter":
                    for (int week = 1; week <= 13; week++)
                    {
                        points.Add(
                            new ActivityPoint
                            {
                                Label = $"W{week}",
                                SessionCount = random.Next(80, 220)
                            });
                    }

                    break;

                case "Year":
                    string[] months =
                    {
                        "Jan",
                        "Feb",
                        "Mar",
                        "Apr",
                        "May",
                        "Jun",
                        "Jul",
                        "Aug",
                        "Sep",
                        "Oct",
                        "Nov",
                        "Dec"
                    };

                    foreach (string month in months)
                    {
                        points.Add(
                            new ActivityPoint
                            {
                                Label = month,
                                SessionCount = random.Next(250, 900)
                            });
                    }

                    break;

                default:
                    int daysInMonth = DateTime.DaysInMonth(
                        referenceDate.Year,
                        referenceDate.Month);

                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        points.Add(
                            new ActivityPoint
                            {
                                Label = day.ToString(),
                                SessionCount = random.Next(25, 125)
                            });
                    }

                    break;
            }

            SetActivityBarHeights(points);

            return points;
        }

        public static List<GamePopularityItem> GetGamePopularity(
            DateTime startDate,
            DateTime endDate)
        {
            List<GamePopularityItem> items =
                new List<GamePopularityItem>
                {
                    new GamePopularityItem
                    {
                        GameId = 1,
                        GameName = "I Spy Water Safety",
                        SessionCount = 324,
                        IsEnabled = true
                    },
                    new GamePopularityItem
                    {
                        GameId = 2,
                        GameName = "Water Management",
                        SessionCount = 281,
                        IsEnabled = true
                    },
                    new GamePopularityItem
                    {
                        GameId = 3,
                        GameName = "A Boating Game",
                        SessionCount = 239,
                        IsEnabled = true
                    },
                    new GamePopularityItem
                    {
                        GameId = 4,
                        GameName = "Drawing the Night Sky",
                        SessionCount = 197,
                        IsEnabled = true
                    },
                    new GamePopularityItem
                    {
                        GameId = 5,
                        GameName = "Legacy Safety Game",
                        SessionCount = 83,
                        IsEnabled = false
                    },
                    new GamePopularityItem
                    {
                        GameId = 6,
                        GameName = "New Game",
                        SessionCount = 0,
                        IsEnabled = false
                    }
                };

            int maxSessions = Math.Max(
                1,
                items.Max(item => item.SessionCount));

            foreach (GamePopularityItem item in items)
            {
                item.BarWidth =
                    item.SessionCount == 0
                    ? 4
                    : Math.Max(
                        12,
                        item.SessionCount /
                        (double)maxSessions *
                        500);
            }

            return items
                .OrderByDescending(item => item.SessionCount)
                .ToList();
        }

        public static List<CategoryUsageItem> GetCategoryUsage(
            DateTime startDate,
            DateTime endDate)
        {
            List<CategoryUsageItem> items =
                new List<CategoryUsageItem>
                {
                    new CategoryUsageItem
                    {
                        CategoryName = "Water Safety",
                        SessionCount = 510,
                        Games = new List<string>
                        {
                            "I Spy Water Safety",
                            "A Boating Game"
                        }
                    },
                    new CategoryUsageItem
                    {
                        CategoryName = "Environmental",
                        SessionCount = 295,
                        Games = new List<string>
                        {
                            "Water Management"
                        }
                    },
                    new CategoryUsageItem
                    {
                        CategoryName = "General Safety",
                        SessionCount = 190,
                        Games = new List<string>
                        {
                            "Legacy Safety Game"
                        }
                    },
                    new CategoryUsageItem
                    {
                        CategoryName = "Corps Mission",
                        SessionCount = 145,
                        Games = new List<string>
                        {
                            "Water Management"
                        }
                    },
                    new CategoryUsageItem
                    {
                        CategoryName = "Cultural/Historical",
                        SessionCount = 95,
                        Games = new List<string>
                        {
                            "Drawing the Night Sky"
                        }
                    },
                    new CategoryUsageItem
                    {
                        CategoryName = "Project Tour",
                        SessionCount = 49,
                        Games = new List<string>()
                    }
                };

            int maxSessions = Math.Max(
                1,
                items.Max(item => item.SessionCount));

            foreach (CategoryUsageItem item in items)
            {
                item.BarWidth =
                    item.SessionCount == 0
                    ? 4
                    : Math.Max(
                        12,
                        item.SessionCount /
                        (double)maxSessions *
                        500);
            }

            return items
                .OrderByDescending(item => item.SessionCount)
                .ToList();
        }

        public static GameDetailSummary GetGameDetails(
            int gameId,
            string gameName)
        {
            return new GameDetailSummary
            {
                GameId = gameId,
                GameName = gameName,
                TotalSessions = 324,
                AveragePlaytimeSeconds = 287,
                CompletionRate = 0.81,
                BestScore = 9400,
                Completed = 262,
                Abandoned = 28,
                Timeout = 12,
                TechnicalIssues = 8,
                OtherExits = 14
            };
        }

        private static void SetActivityBarHeights(
            List<ActivityPoint> points)
        {
            int maxSessions = Math.Max(
                1,
                points.Max(point => point.SessionCount));

            foreach (ActivityPoint point in points)
            {
                point.BarHeight = Math.Max(
                    8,
                    point.SessionCount /
                    (double)maxSessions *
                    165);
            }
        }

        private static string FormatHour(int hour)
        {
            if (hour == 12)
            {
                return "12 PM";
            }

            if (hour > 12)
            {
                return $"{hour - 12} PM";
            }

            return $"{hour} AM";
        }
    }
}
