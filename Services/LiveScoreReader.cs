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

            using var doc = JsonDocument.Parse(File.ReadAllText(scoreFile));
            JsonElement root = doc.RootElement;

            // Use the game's own timestamp when it has one, otherwise when the file was last saved
            DateTimeOffset writtenAt = File.GetLastWriteTimeUtc(scoreFile);
            if (root.TryGetProperty("updatedAt", out JsonElement updatedAt) &&
                updatedAt.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(updatedAt.GetString(), out DateTimeOffset parsed))
            {
                writtenAt = parsed;
            }

            if (writtenAt < launchedAt)
                return null;

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
