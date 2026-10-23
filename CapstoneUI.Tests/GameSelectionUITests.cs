using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.IO;

[assembly: DoNotParallelize]

[TestClass]
[DoNotParallelize]
public class GameSelectionUiTests
{
    private Application _app;
    private Window _window;
    private UIA3Automation _automation;

    [TestInitialize]
    public void Setup()
    {
        string exePath = Path.Combine(AppContext.BaseDirectory, "Capstone UI.exe");
        Assert.IsTrue(File.Exists(exePath), $"Application executable was not found at: {exePath}. Build the Capstone UI project first.");

        string buildDir = Path.GetDirectoryName(exePath);

        // Ensure the test JSON file exists in that build folder
        string jsonPath = Path.Combine(buildDir, "games.json");
        string sampleJson = "[ { \"Title\": \"Water Safety Adventure\", \"Description\": \"Test game\" } ]";
        File.WriteAllText(jsonPath, sampleJson);

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = buildDir
        };

        _app = Application.Launch(psi);
        _automation = new UIA3Automation();
        _window = _app.GetMainWindow(_automation);

        // Poll for up to 20 seconds to give the WPF window time to fully render content.
        Button playButton = null;
        for (int i = 0; i < 40 && playButton == null; i++)
        {
            _window = _app.GetMainWindow(_automation) ?? _window;
            var buttonElement = _window.FindFirstDescendant(cf => cf.ByAutomationId("PlayButton"))
                ?? _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button))
                    .FirstOrDefault(element => element.BoundingRectangle.Width >= 600);
            if (buttonElement != null)
            {
                playButton = buttonElement.AsButton();
            }

            System.Threading.Thread.Sleep(500);
        }

        Assert.IsNotNull(playButton, "The PLAY GAMES button was not found after waiting for render.");
        playButton.Click();
        System.Threading.Thread.Sleep(800); // Pause for page transition
    }

    [TestCleanup]
    public void TearDown()
    {
        _app?.Close();
        _automation?.Dispose();
    }

    [TestMethod]
    public void GameList_ShouldLoadGamesAutomatically()
    {
        // Give the UI a moment to load items into the ListBox
        System.Threading.Thread.Sleep(1000);

        var gameListBox = _window.FindFirstDescendant(cf => cf.ByAutomationId("GameListBox"))?.AsListBox();

        Assert.IsNotNull(gameListBox, "The Game ListBox should be present on the selection screen.");
        Assert.IsTrue(gameListBox.Items.Length > 0, "The game library successfully loaded items from JSON.");
    }
}