using System;
using System.IO;
using System.Text.Json;

namespace Capstone_UI.Services;

// Reads the score file that scoring games write while they run:
// %LOCALAPPDATA%\KSUGameLauncher\live_score.json  ->  { "player", "gameId", "score", "updatedAt" }
public static class LiveScoreReader
{
    public static readonly string DefaultScoreFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KSUGameLauncher", "live_score.json");

    // Returns the score only if the file was written after the game was launched,
    // so a leftover file from an earlier play (or another game) is never reused.
    // Returns null when the game didn't report a score.
    public static int? TryReadScore(DateTimeOffset launchedAt, string? scoreFile = null)
    {
        scoreFile ??= DefaultScoreFile;

        try
        {
            if (!File.Exists(scoreFile))
                return null;

            // Use when Windows last saved the file, not the game's "updatedAt" text, which could be in the wrong time zone.
            // Games that don't report scores never touch this file, so for them it's always older than the launch.
            DateTimeOffset writtenAt = File.GetLastWriteTimeUtc(scoreFile);
            if (writtenAt < launchedAt)
                return null;

            using var doc = JsonDocument.Parse(File.ReadAllText(scoreFile));
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("score", out JsonElement score))
                return null;

            if (score.ValueKind == JsonValueKind.Number && score.TryGetInt32(out int number))
                return number;

            if (score.ValueKind == JsonValueKind.String && int.TryParse(score.GetString(), out int fromText))
                return fromText;

            return null;
        }
        catch (Exception) // Missing, locked, or half-written file: treat it as no score
        {
            return null;
        }
    }
}
