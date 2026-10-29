using Capstone_UI.Services;

namespace CapstoneUI.Tests;

[TestClass]
public sealed class LiveScoreReaderTests : IDisposable
{
    private readonly string _scoreFile;

    // Runs before EACH test: every test gets its own score file path (not created yet)
    public LiveScoreReaderTests()
    {
        _scoreFile = Path.Combine(Path.GetTempPath(), $"live_score_{Guid.NewGuid()}.json");
    }

    // Runs after EACH test: delete the score file if the test made one
    public void Dispose()
    {
        File.Delete(_scoreFile);
    }

    private void WriteScoreFile(string json) => File.WriteAllText(_scoreFile, json);

    private static string IsoTime(DateTimeOffset time) => time.ToString("O");

    [TestMethod]
    public void TryReadScore_NoFile_ReturnsNull()
    {
        Assert.IsNull(LiveScoreReader.TryReadScore(DateTimeOffset.UtcNow, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_WrittenAfterLaunch_ReturnsScore()
    {
        var launchedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        WriteScoreFile($$"""{ "player": "Alice", "gameId": "boat", "score": 1200, "updatedAt": "{{IsoTime(DateTimeOffset.UtcNow)}}" }""");

        Assert.AreEqual(1200, LiveScoreReader.TryReadScore(launchedAt, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_FileSavedBeforeLaunch_ReturnsNull()
    {
        // A non-scoring game: the file is left over from an earlier play of the scoring game
        WriteScoreFile("""{ "player": "Alice", "score": 1200 }""");
        File.SetLastWriteTimeUtc(_scoreFile, DateTime.UtcNow.AddMinutes(-10));

        Assert.IsNull(LiveScoreReader.TryReadScore(DateTimeOffset.UtcNow, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_IgnoresUpdatedAtInWrongTimeZone()
    {
        // The game's own timestamp claims the future, but the file was really saved before launch
        WriteScoreFile($$"""{ "player": "Alice", "score": 1200, "updatedAt": "{{IsoTime(DateTimeOffset.UtcNow.AddHours(5))}}" }""");
        File.SetLastWriteTimeUtc(_scoreFile, DateTime.UtcNow.AddMinutes(-10));

        Assert.IsNull(LiveScoreReader.TryReadScore(DateTimeOffset.UtcNow, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_NoUpdatedAt_ReturnsScore()
    {
        var launchedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        WriteScoreFile("""{ "player": "Alice", "score": 300 }""");

        Assert.AreEqual(300, LiveScoreReader.TryReadScore(launchedAt, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_ScoreAsText_ReturnsScore()
    {
        var launchedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        WriteScoreFile("""{ "player": "Alice", "score": "450" }""");

        Assert.AreEqual(450, LiveScoreReader.TryReadScore(launchedAt, _scoreFile));
    }

    [TestMethod]
    public void TryReadScore_HalfWrittenFile_ReturnsNull()
    {
        var launchedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        WriteScoreFile("""{ "player": "Alice", "sco""");

        Assert.IsNull(LiveScoreReader.TryReadScore(launchedAt, _scoreFile));
    }
}
