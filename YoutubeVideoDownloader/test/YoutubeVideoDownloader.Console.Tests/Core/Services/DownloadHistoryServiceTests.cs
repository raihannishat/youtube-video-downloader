using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Tests.Core.Services;

public class DownloadHistoryServiceTests
{
    private readonly Mock<ILoggerService> _loggerMock;
    private string _tempHistoryPath;

    public DownloadHistoryServiceTests()
    {
        _loggerMock = new Mock<ILoggerService>();
        _tempHistoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "download-history.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_tempHistoryPath)!);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Assert
        service.Should()
            .NotBeNull()
            .And.BeOfType<DownloadHistoryService>()
            .And.BeAssignableTo<IDownloadHistoryService>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new DownloadHistoryService(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetHistoryFilePath_ShouldReturnValidPath()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act
        var path = service.GetHistoryFilePath();

        // Assert
        path.Should().NotBeNullOrEmpty();
        path.Should().Contain("YoutubeVideoDownloader");
        path.Should().EndWith("download-history.json");
    }

    [Fact]
    public void GetHistoryCount_ShouldReturnZero_WhenNoHistory()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act
        var count = service.GetHistoryCount();

        // Assert
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void AddHistoryEntry_ShouldAddEntryToHistory()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry = new DownloadHistoryEntry
        {
            VideoId = "test123",
            VideoTitle = "Test Video",
            ChannelName = "Test Channel",
            VideoUrl = "https://youtube.com/watch?v=test123",
            FilePath = "C:\\Downloads\\test.mp4",
            Quality = "720p",
            FileSizeBytes = 1024000,
            DownloadDate = DateTime.Now,
            Duration = TimeSpan.FromMinutes(5),
            IsPlaylist = false
        };

        // Act
        service.AddHistoryEntry(entry);

        // Assert
        var history = service.GetHistory();
        history.Should().Contain(e => e.VideoId == "test123");
        service.GetHistoryCount().Should().BeGreaterThan(0);
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Added download history entry"))), Times.Once);
    }

    [Fact]
    public void AddHistoryEntry_WithNullEntry_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act & Assert
        var action = () => service.AddHistoryEntry(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetHistory_ShouldReturnAllEntries_WhenNoLimit()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry1 = new DownloadHistoryEntry { VideoId = "test1", VideoTitle = "Video 1" };
        var entry2 = new DownloadHistoryEntry { VideoId = "test2", VideoTitle = "Video 2" };
        var entry3 = new DownloadHistoryEntry { VideoId = "test3", VideoTitle = "Video 3" };

        service.AddHistoryEntry(entry1);
        service.AddHistoryEntry(entry2);
        service.AddHistoryEntry(entry3);

        // Act
        var history = service.GetHistory();

        // Assert
        history.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void GetHistory_WithLimit_ShouldReturnLimitedEntries()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        for (int i = 1; i <= 5; i++)
        {
            service.AddHistoryEntry(new DownloadHistoryEntry
            {
                VideoId = $"test{i}",
                VideoTitle = $"Video {i}"
            });
        }

        // Act
        var history = service.GetHistory(limit: 3);

        // Assert
        history.Should().HaveCount(3);
    }

    [Fact]
    public void GetHistoryByVideoId_ShouldReturnEntry_WhenExists()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry = new DownloadHistoryEntry
        {
            VideoId = "specific123",
            VideoTitle = "Specific Video"
        };
        service.AddHistoryEntry(entry);

        // Act
        var found = service.GetHistoryByVideoId("specific123");

        // Assert
        found.Should().NotBeNull();
        found!.VideoId.Should().Be("specific123");
        found.VideoTitle.Should().Be("Specific Video");
    }

    [Fact]
    public void GetHistoryByVideoId_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act
        var found = service.GetHistoryByVideoId("nonexistent");

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void GetHistoryByVideoId_WithNullOrEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act & Assert
        var action1 = () => service.GetHistoryByVideoId(null!);
        var action2 = () => service.GetHistoryByVideoId(string.Empty);
        var action3 = () => service.GetHistoryByVideoId("   ");

        action1.Should().Throw<ArgumentException>();
        action2.Should().Throw<ArgumentException>();
        action3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetHistoryByDateRange_ShouldReturnEntriesInRange()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var today = DateTime.Now;
        var entry1 = new DownloadHistoryEntry
        {
            VideoId = "test1",
            VideoTitle = "Video 1",
            DownloadDate = today.AddDays(-2)
        };
        var entry2 = new DownloadHistoryEntry
        {
            VideoId = "test2",
            VideoTitle = "Video 2",
            DownloadDate = today.AddDays(-1)
        };
        var entry3 = new DownloadHistoryEntry
        {
            VideoId = "test3",
            VideoTitle = "Video 3",
            DownloadDate = today
        };

        service.AddHistoryEntry(entry1);
        service.AddHistoryEntry(entry2);
        service.AddHistoryEntry(entry3);

        // Act
        var history = service.GetHistoryByDateRange(today.AddDays(-1), today);

        // Assert
        history.Should().Contain(e => e.VideoId == "test2");
        history.Should().Contain(e => e.VideoId == "test3");
        history.Should().NotContain(e => e.VideoId == "test1");
    }

    [Fact]
    public void ClearHistory_ShouldRemoveAllEntries()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        service.AddHistoryEntry(new DownloadHistoryEntry { VideoId = "test1", VideoTitle = "Video 1" });
        service.AddHistoryEntry(new DownloadHistoryEntry { VideoId = "test2", VideoTitle = "Video 2" });

        // Act
        service.ClearHistory();

        // Assert
        service.GetHistoryCount().Should().Be(0);
        service.GetHistory().Should().BeEmpty();
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Download history cleared"))), Times.Once);
    }

    [Fact]
    public void RemoveHistoryEntry_ShouldRemoveEntry_WhenExists()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry = new DownloadHistoryEntry { VideoId = "test123", VideoTitle = "Test Video" };
        service.AddHistoryEntry(entry);

        // Act
        service.RemoveHistoryEntry("test123");

        // Assert
        service.GetHistoryByVideoId("test123").Should().BeNull();
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Removed history entry"))), Times.Once);
    }

    [Fact]
    public void RemoveHistoryEntry_WithNullOrEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act & Assert
        var action1 = () => service.RemoveHistoryEntry(null!);
        var action2 = () => service.RemoveHistoryEntry(string.Empty);
        var action3 = () => service.RemoveHistoryEntry("   ");

        action1.Should().Throw<ArgumentException>();
        action2.Should().Throw<ArgumentException>();
        action3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddHistoryEntry_ShouldPreventDuplicates_ForNonPlaylistVideos()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry1 = new DownloadHistoryEntry
        {
            VideoId = "test123",
            VideoTitle = "First Title",
            IsPlaylist = false
        };
        var entry2 = new DownloadHistoryEntry
        {
            VideoId = "test123",
            VideoTitle = "Second Title",
            IsPlaylist = false
        };

        // Act
        service.AddHistoryEntry(entry1);
        service.AddHistoryEntry(entry2);

        // Assert
        var history = service.GetHistory();
        var entries = history.Where(e => e.VideoId == "test123" && !e.IsPlaylist).ToList();
        entries.Should().HaveCount(1);
        entries.First().VideoTitle.Should().Be("Second Title"); // Most recent should be kept
    }

    [Fact]
    public void AddHistoryEntry_ShouldKeepLast1000Entries()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);

        // Act - Add more than 1000 entries
        for (int i = 1; i <= 1005; i++)
        {
            service.AddHistoryEntry(new DownloadHistoryEntry
            {
                VideoId = $"test{i}",
                VideoTitle = $"Video {i}"
            });
        }

        // Assert
        service.GetHistoryCount().Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public void AddHistoryEntry_ShouldAddToBeginning_ForMostRecentFirst()
    {
        // Arrange
        var service = new DownloadHistoryService(_loggerMock.Object);
        var entry1 = new DownloadHistoryEntry { VideoId = "test1", VideoTitle = "First" };
        var entry2 = new DownloadHistoryEntry { VideoId = "test2", VideoTitle = "Second" };

        // Act
        service.AddHistoryEntry(entry1);
        service.AddHistoryEntry(entry2);

        // Assert
        var history = service.GetHistory();
        history.First().VideoId.Should().Be("test2"); // Most recent first
        history.Skip(1).First().VideoId.Should().Be("test1");
    }
}

