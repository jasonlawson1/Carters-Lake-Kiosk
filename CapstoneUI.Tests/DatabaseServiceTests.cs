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
}