using System;
using System.IO;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Capstone_UI.Services;

public class DatabaseService
{

    private readonly string _connectionString;

    public DatabaseService(string? dbPath = null)
    {
        if (dbPath == null)
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CapstoneUI");
            Directory.CreateDirectory(folder);
            dbPath = Path.Combine(folder, "gamehub.db");
        }

        _connectionString = $"Data Source={dbPath};Foreign Keys=True";
        CreateTables();
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          CREATE TABLE IF NOT EXISTS users (
                              id INTEGER PRIMARY KEY AUTOINCREMENT,
                              username TEXT NOT NULL,
                              is_adult INTEGER NOT NULL,
                          created_at TEXT NOT NULL
                              );
                              
                          CREATE TABLE IF NOT EXISTS games (
                              id INTEGER PRIMARY KEY AUTOINCREMENT,
                              name TEXT NOT NULL
                              );
                              
                          CREATE TABLE IF NOT EXISTS game_data (
                              id INTEGER PRIMARY KEY AUTOINCREMENT,
                              user_id INTEGER NOT NULL REFERENCES users(id),
                              game_id INTEGER NOT NULL REFERENCES games(id),
                              score INTEGER,
                              time_played INTEGER NOT NULL,
                              played_at TEXT NOT NULL
                              );
                          """;
        cmd.ExecuteNonQuery();
    }

    public int InsertUser(string name, bool isAdult)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          INSERT INTO users (username, is_adult, created_at)
                          VALUES ($username, $isAdult,  $created_at)
                          RETURNING id;
                          """;
        cmd.Parameters.AddWithValue("username", name);
        cmd.Parameters.AddWithValue("$isAdult", isAdult ? 1 : 0);
        cmd.Parameters.AddWithValue("$created_at", DateTime.UtcNow.ToString("O"));

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void InsertGameData(int userId, int gameId, int score, int timePlayed)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          INSERT INTO game_data (user_id, game_id, score, time_played,  played_at)
                          VALUES ($user_id, $game_id, $score, $time_played,  $played_at)
                          """;
        cmd.Parameters.AddWithValue("$user_id", userId);
        cmd.Parameters.AddWithValue("$game_id", gameId);
        cmd.Parameters.AddWithValue("$score", score);
        cmd.Parameters.AddWithValue("$time_played", timePlayed);
        cmd.Parameters.AddWithValue("$played_at", DateTime.UtcNow.ToString("O"));
        
        cmd.ExecuteNonQuery();
    }

    public void EnsureGame(int gameId, string name)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          INSERT or IGNORE INTO games (id, name) VALUES ($game_id, $name)
                          """;
        cmd.Parameters.AddWithValue("$game_id", gameId);
        cmd.Parameters.AddWithValue("$name", name);
        
        cmd.ExecuteNonQuery();
    }

    public int InsertGame(string name)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          INSERT INTO games (name) VALUES ($name)
                          RETURNING id;
                          """;
        cmd.Parameters.AddWithValue("$name", name);
        
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

        public int GetTotalSessions()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM game_data;";

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public double GetAverageSessionDuration()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(AVG(time_played), 0) FROM game_data;";

        return Convert.ToDouble(cmd.ExecuteScalar());
    }

    public int GetAdultSessionCount()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT COUNT(*)
                          FROM game_data gd
                          JOIN users u ON gd.user_id = u.id
                          WHERE u.is_adult = 1;
                          """;

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetUnder18SessionCount()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT COUNT(*)
                          FROM game_data gd
                          JOIN users u ON gd.user_id = u.id
                          WHERE u.is_adult = 0;
                          """;

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<string> GetGameUsage()
    {
        List<string> gameUsage = new List<string>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT g.name, COUNT(gd.id) AS play_count
                          FROM games g
                          LEFT JOIN game_data gd ON g.id = gd.game_id
                          GROUP BY g.id, g.name
                          ORDER BY play_count DESC, g.name ASC;
                          """;

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string gameName = reader.GetString(0);
            int playCount = reader.GetInt32(1);

            gameUsage.Add($"{gameName} - {playCount} play(s)");
        }

        return gameUsage;
    }

    public List<string> GetRecentSessions()
    {
        List<string> recentSessions = new List<string>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT u.username, u.is_adult, g.name, gd.time_played
                          FROM game_data gd
                          JOIN users u ON gd.user_id = u.id
                          JOIN games g ON gd.game_id = g.id
                          ORDER BY gd.played_at DESC
                          LIMIT 10;
                          """;

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string username = reader.GetString(0);
            bool isAdult = reader.GetInt32(1) == 1;
            string gameName = reader.GetString(2);
            int timePlayed = reader.GetInt32(3);

            string ageGroup = isAdult ? "Adult" : "Under 18";
            int minutes = timePlayed / 60;
            int seconds = timePlayed % 60;

            recentSessions.Add($"{username} | {ageGroup} | {gameName} | {minutes}m {seconds}s");
        }

        return recentSessions;
    }

    public void InsertTestAnalyticsData()
    {
        int gameId = 1;
        EnsureGame(gameId, "I Spy Water Safety");

        int adultUserId = InsertUser("Test Adult", true);
        InsertGameData(adultUserId, gameId, 0, 125);

        int under18UserId = InsertUser("Test Student", false);
        InsertGameData(under18UserId, gameId, 0, 80);
    }
}
