using System.Windows;

namespace Capstone_UI
{
    public partial class GameManagerWindow : Window
    {
        public GameManagerWindow()
        {
            InitializeComponent();

            // Load the full list of games (both active and inactive) for staff to manage
            dgGames.ItemsSource = GameSelection.GamesList;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Automatically persist any changes made via the checkboxes when closing
            GameSelection.SaveGamesList();
            this.Close();
        }
    }
}