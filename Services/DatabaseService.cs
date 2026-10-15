using System;
using System.IO;
using System.Windows.Media;
using Microsoft.Data.Sqlite;

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
}
