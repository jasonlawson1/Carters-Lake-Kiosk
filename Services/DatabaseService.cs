using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Capstone_UI.Services;

public class DatabaseService
{

    private readonly string _connectionString;

    public DatabaseService()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CapstoneUI");
        Directory.CreateDirectory(folder);
        string dbPath = Path.Combine(folder, "gamehub.db");

        _connectionString = $"Data Source={dbPath}; Foreign Keys=True";

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
}
