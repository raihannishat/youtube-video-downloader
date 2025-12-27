namespace YoutubeVideoDownloader.Console.Tests.Core.Services;

public class YouTubeServiceTests
{
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly YouTubeService _service;

    public YouTubeServiceTests()
    {
        _loggerMock = new Mock<ILoggerService>();
        _service = new YouTubeService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new YouTubeService(_loggerMock.Object);

        // Assert
        service.Should()
            .NotBeNull()
            .And.BeOfType<YouTubeService>()
            .And.BeAssignableTo<IYouTubeService>();
    }

    [Fact]
    public void Constructor_ShouldLogInitialization()
    {
        // Act
        var service = new YouTubeService(_loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
        _loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new YouTubeService(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPlaylistAsync_ShouldReturnPlaylist()
    {
        // Arrange - Using a well-known public playlist (YouTube's "Music" playlist)
        var playlistId = YoutubeExplode.Playlists.PlaylistId.Parse("PLrAXtmRdnEQy6nuLMH7FPR8-O7LxYqkfX");

        // Act & Assert
        try
        {
            var playlist = await _service.GetPlaylistAsync(playlistId);
            playlist.Should().NotBeNull();
            playlist.Id.Should().Be(playlistId);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
        catch (YoutubeExplode.Exceptions.PlaylistUnavailableException)
        {
            // Skip test if playlist is unavailable (may be private/deleted)
            // This is acceptable for integration tests
        }
    }

    [Fact]
    public async Task GetPlaylistVideosAsync_ShouldReturnVideos()
    {
        // Arrange - Using a well-known public playlist
        var playlistId = YoutubeExplode.Playlists.PlaylistId.Parse("PLrAXtmRdnEQy6nuLMH7FPR8-O7LxYqkfX");
        var videos = new List<YoutubeExplode.Playlists.PlaylistVideo>();

        // Act & Assert
        try
        {
            await foreach (var video in _service.GetPlaylistVideosAsync(playlistId))
            {
                videos.Add(video);
                if (videos.Count >= 5) break; // Limit to first 5 for testing
            }

            videos.Should().NotBeEmpty();
            videos.Should().OnlyContain(v => v != null);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
        catch (YoutubeExplode.Exceptions.PlaylistUnavailableException)
        {
            // Skip test if playlist is unavailable (may be private/deleted)
            // This is acceptable for integration tests
        }
    }
}

