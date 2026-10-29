using Capstone_UI.Models;
using Capstone_UI.Services;
using Microsoft.Data.Sqlite;

namespace CapstoneUI.Tests;

[TestClass]
public sealed class DatabaseServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DatabaseService _db;

    // Runs before EACH test: every test gets a brand-new, empty database file
    public DatabaseServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _db = new DatabaseService(_dbPath);
    }

    // Runs after EACH test: release the file and delete it
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    // Helper: runs a query that returns one value.
    // Only used with SQL written in this file, never user input.
    private object? QueryScalar(string sql)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }

    private long CountRows(string table) => (long)QueryScalar($"SELECT COUNT(*) FROM {table}")!;

    // ---------- InsertUser ----------

    [TestMethod]
    public void InsertUser_ReturnsIncreasingIds()
    {
        int first = _db.InsertUser("Alice", true);
        int second = _db.InsertUser("Bob", false);

        Assert.AreEqual(1, first);
        Assert.AreEqual(2, second);
    }

    [TestMethod]
    public void InsertUser_SavesNameAndAdultFlag()
    {
        int id = _db.InsertUser("Alice", true);

        Assert.AreEqual("Alice", (string?)QueryScalar($"SELECT username FROM users WHERE id = {id}"));
        Assert.AreEqual(1L, (long)QueryScalar($"SELECT is_adult FROM users WHERE id = {id}")!);
    }

    // ---------- InsertGameData ----------

    [TestMethod]
    public void InsertGameData_SavesOneRow()
    {
        _db.EnsureGame(1, "Test Game");
        int userId = _db.InsertUser("Alice", true);

        _db.InsertGameData(userId, 1, 0, 42);

        Assert.AreEqual(1L, CountRows("game_data"));
    }

    [TestMethod]
    public void InsertGameData_SavesTimePlayed()
    {
        _db.EnsureGame(1, "Test Game");
        int userId = _db.InsertUser("Alice", true);

        _db.InsertGameData(userId, 1, 0, 42);

        Assert.AreEqual(42L, (long)QueryScalar("SELECT time_played FROM game_data")!);
    }

    [TestMethod]
    public void InsertGameData_WithUnknownUser_Throws()
    {
        _db.EnsureGame(1, "Test Game");

        Assert.ThrowsExactly<SqliteException>(() => _db.InsertGameData(9999, 1, 0, 42));
    }

    [TestMethod]
    public void InsertGameData_WithUnknownGame_Throws()
    {
        int userId = _db.InsertUser("Alice", true);

        Assert.ThrowsExactly<SqliteException>(() => _db.InsertGameData(userId, 9999, 0, 42));
    }

    // ---------- EnsureGame ----------

    [TestMethod]
    public void EnsureGame_CalledTwice_AddsOnlyOneRow()
    {
        _db.EnsureGame(1, "Test Game");
        _db.EnsureGame(1, "Test Game");

        Assert.AreEqual(1L, CountRows("games"));
    }

    [TestMethod]
    public void EnsureGame_SavesName()
    {
        _db.EnsureGame(1, "Test Game");

        Assert.AreEqual("Test Game", (string?)QueryScalar("SELECT name FROM games WHERE id = 1"));
    }

    // ---------- InsertGame ----------

    [TestMethod]
    public void InsertGame_AfterDefaultGames_ReturnsNextId()
    {
        _db.EnsureGame(1, "Game 1");
        _db.EnsureGame(2, "Game 2");
        _db.EnsureGame(3, "Game 3");
        _db.EnsureGame(4, "Game 4");

        int newId = _db.InsertGame("Added Game");

        Assert.AreEqual(5, newId);
    }

    [TestMethod]
    public void InsertGame_SavesName()
    {
        int id = _db.InsertGame("Added Game");

        Assert.AreEqual("Added Game", (string?)QueryScalar($"SELECT name FROM games WHERE id = {id}"));
    }

    // ---------- Old placeholder scores ----------

    [TestMethod]
    public void Startup_ClearsOldPlaceholderZeroScores_Once()
    {
        _db.EnsureGame(1, "Test Game");
        int userId = _db.InsertUser("Alice", true);

        // Simulate a database from before scores existed: placeholder 0 and user_version not set yet
        QueryScalar($"INSERT INTO game_data (user_id, game_id, score, time_played, played_at) VALUES ({userId}, 1, 0, 10, 'x')");
        QueryScalar("PRAGMA user_version = 0");

        new DatabaseService(_dbPath); // App starts: placeholder 0 becomes "no score"
        Assert.IsNull(_db.GetLeaderboard(1)[0].Score);

        _db.InsertGameData(userId, 1, 0, 10); // A real score of 0 saved after the cleanup
        new DatabaseService(_dbPath); // Next startup must not clear it

        Assert.AreEqual(1L, (long)QueryScalar("SELECT COUNT(*) FROM game_data WHERE score = 0")!);
    }

    // ---------- GetLeaderboard ----------

    [TestMethod]
    public void GetLeaderboard_NoPlays_ReturnsEmpty()
    {
        _db.EnsureGame(1, "Test Game");

        Assert.AreEqual(0, _db.GetLeaderboard(1).Count);
    }

    [TestMethod]
    public void GetLeaderboard_OnlyIncludesSelectedGame()
    {
        _db.EnsureGame(1, "Game 1");
        _db.EnsureGame(2, "Game 2");
        _db.InsertGameData(_db.InsertUser("Alice", true), 1, 0, 10);
        _db.InsertGameData(_db.InsertUser("Bob", false), 2, 0, 10);

        var leaderboard = _db.GetLeaderboard(1);

        Assert.AreEqual(1, leaderboard.Count);
        Assert.AreEqual("Alice", leaderboard[0].Username);
    }

    [TestMethod]
    public void GetLeaderboard_RespectsLimitAndNumbersRanks()
    {
        _db.EnsureGame(1, "Test Game");
        for (int i = 0; i < 7; i++)
            _db.InsertGameData(_db.InsertUser($"Player {i}", true), 1, 0, 10);

        var leaderboard = _db.GetLeaderboard(1, 5);

        Assert.AreEqual(5, leaderboard.Count);
        Assert.AreEqual(1, leaderboard[0].Rank);
        Assert.AreEqual(5, leaderboard[4].Rank);
    }

    [TestMethod]
    public void GetLeaderboard_HighestScoreFirst()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Low", true), 1, 10, 10);
        _db.InsertGameData(_db.InsertUser("High", true), 1, 90, 10);

        Assert.AreEqual("High", _db.GetLeaderboard(1)[0].Username);
    }

    [TestMethod]
    public void GetLeaderboard_NoScore_DisplaysAffirmation()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Alice", true), 1, null, 10);

        CollectionAssert.Contains(LeaderboardEntry.Affirmations, _db.GetLeaderboard(1)[0].DisplayScore);
    }

    [TestMethod]
    public void GetLeaderboard_NoScore_SamePlayerGetsSameAffirmationEachTime()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Alice", true), 1, null, 10);

        Assert.AreEqual(_db.GetLeaderboard(1)[0].DisplayScore, _db.GetLeaderboard(1)[0].DisplayScore);
    }

    [TestMethod]
    public void GetLeaderboard_NoScoreOnScoreboardGame_DisplaysDidNotFinish()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Alice", true), 1, null, 10);

        var entry = _db.GetLeaderboard(1)[0];
        entry.GameHasScoreboard = true;

        Assert.AreEqual("Did Not Finish :(", entry.DisplayScore);
    }

    [TestMethod]
    public void GetLeaderboard_WithScore_DisplaysScore()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Alice", true), 1, 1500, 10);

        Assert.AreEqual(1500.ToString("N0"), _db.GetLeaderboard(1)[0].DisplayScore);
    }

    [TestMethod]
    public void GetLeaderboard_PlaysWithoutScore_SortAfterScoredPlays()
    {
        _db.EnsureGame(1, "Test Game");
        _db.InsertGameData(_db.InsertUser("Scored", true), 1, 5, 10);
        _db.InsertGameData(_db.InsertUser("NoScore", true), 1, null, 10);

        Assert.AreEqual("Scored", _db.GetLeaderboard(1)[0].Username);
    }
}