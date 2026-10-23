using System;
using System.Collections.Generic;
using Xunit;
using Capstone_UI.Models;
using Capstone_UI;

namespace Capstone_UI.Tests
{
    public class KioskAppTests
    {
        [Fact]
        public void Game_SafeImagePath_FallsBackToDefault_WhenPathsAreEmpty() // Ensures that default image pulls when Image List is empty
        {

            var game = new Game
            {
                Id = 99,
                Title = "Test Game",
                ImagePath = string.Empty,
                ImageList = new List<string>()
            };

        
            string resolvedPath = game.SafeImagePath;


            Xunit.Assert.Equal("/Images/DefaultGameMenu.png", resolvedPath); 
        }

        [Fact]
        public void Game_SafeImagePath_ReturnsValidRelativePath_WhenSet() // Ensures relative path is returned valid for games
        { 
            var game = new Game
            {
                Id = 100,
                Title = "Valid Game",
                ImagePath = "/Images/I Spy Water Safety Menu.png"
            };


            string resolvedPath = game.SafeImagePath;

            Xunit.Assert.Equal("/Images/I Spy Water Safety Menu.png", resolvedPath);
        }

        [Fact]
        public void GameSelection_DefaultList_InitializesCoreGames() // Ensures default comes load when gamelist is empty
        {

            var games = GameSelection.GamesList;


            Xunit.Assert.NotNull(games);
            Xunit.Assert.True(games.Count >= 4, "Expected default kiosk games to be initialized.");

            var waterGame = games.Find(g => g.Id == 1);
            Xunit.Assert.NotNull(waterGame);
            Xunit.Assert.Equal("I Spy Water Safety", waterGame.Title);
            Xunit.Assert.True(waterGame.IsActive);
        }

        [Fact]
        public void GameSelection_RemoveGameByTitle_RemovesGameFromList() // Ensures games are removed correctly when name is written
        {

            var testGameTitle = "Temp Game Removal Test";

            var list = GameSelection.GamesList;
            list.Add(new Game
            {
                Id = 999,
                Title = testGameTitle,
                Category = "Test Category",
                IsActive = true
            });


            GameSelection.RemoveGameByTitle(testGameTitle);


            var removedGame = GameSelection.GamesList.Find(g => g.Title.Equals(testGameTitle, StringComparison.OrdinalIgnoreCase));
            Xunit.Assert.Null(removedGame);
        }

        [Fact]
        public void KeyboardHelper_CloseTouchKeyboard_ExecutesWithoutException() // Ensures keyboard closes correctly
        {

            var exception = Xunit.Record.Exception(() =>
            {
                KeyboardHelper.CloseTouchKeyboard();
            });

            Xunit.Assert.Null(exception);
        }

        [Fact]
        public void RangerTools_GameMetadataUpdate_ModifiesAndPersistsProperties() // Ensures edited games from game editor in Ranger tools values are updated amd correct after edited
        {

            var testGame = new Game
            {
                Id = 888,
                Title = "Original Title",
                Category = "Original Category",
                Description = "Original Description",
                IsActive = true
            };


            testGame.Title = "Updated Ranger Title";
            testGame.Category = "Updated Category";
            testGame.Description = "Updated Description";

    
            Xunit.Assert.Equal("Updated Ranger Title", testGame.Title);
            Xunit.Assert.Equal("Updated Category", testGame.Category);
            Xunit.Assert.Equal("Updated Description", testGame.Description);
        }
    }
}