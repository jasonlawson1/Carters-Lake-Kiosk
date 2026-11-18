using Capstone_UI.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Capstone_UI
{
    public partial class EducationalCategoriesPage : Page
    {
        public EducationalCategoriesPage(
            DateTime startDate,
            DateTime endDate)
        {
            InitializeComponent();

            CategoryList.ItemsSource =
                MockAnalyticsService.GetCategoryUsage(
                    startDate,
                    endDate);
        }
    }
}
