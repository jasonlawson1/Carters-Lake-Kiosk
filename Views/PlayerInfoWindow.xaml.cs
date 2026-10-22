using System.Windows;
using System;
using System.Windows.Threading;

namespace Capstone_UI.Views;

public partial class PlayerInfoWindow : Window
{
    public string Username { get; private set; } = "";
    public bool IsAdult { get; private set; }

    private readonly DispatcherTimer _idleTimer;
    public PlayerInfoWindow()
    {
        InitializeComponent();
        
        _idleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _idleTimer.Tick += idleTimer_Tick;

        PreviewKeyDown += (s, e) => ResetIdleTimer();
        PreviewMouseDown += (s, e) => ResetIdleTimer();
        PreviewTouchDown += (s, e) => ResetIdleTimer();
        
        Loaded += (s,e) => _idleTimer.Start();
        Closed += (s,e) => _idleTimer.Stop();
    }

    private void ResetIdleTimer()
    {
        _idleTimer.Stop();
        _idleTimer.Start();
    }

    private void idleTimer_Tick(object sender, EventArgs e)
    {
        _idleTimer.Stop();
        DialogResult = false; //closes the window as if the user pressed cancel
    }

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