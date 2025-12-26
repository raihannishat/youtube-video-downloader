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
}

