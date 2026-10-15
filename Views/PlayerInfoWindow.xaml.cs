using System.Windows;

namespace Capstone_UI.Views;

public partial class PlayerInfoWindow : Window
{
    public PlayerInfoWindow()
    {
        InitializeComponent();
    }
    public string Username { get; private set; } = "";
    public bool IsAdult { get; private set; }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Please enter a name.");
            return;   // keep the window open
        }

        //Username is used in gameselection.xaml.cs to pull what the user types
        Username = NameBox.Text.Trim();
        //same case here
        IsAdult = AdultCheckBox.IsChecked == true;
        DialogResult = true;   // closes the window; ShowDialog() returns true
    }
    
}