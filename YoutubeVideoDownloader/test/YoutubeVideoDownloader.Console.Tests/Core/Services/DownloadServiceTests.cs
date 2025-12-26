namespace YoutubeVideoDownloader.Console.Tests.Core.Services;

public class DownloadServiceTests
{
    private readonly Mock<IYouTubeService> _youTubeServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly DownloadService _service;

    public DownloadServiceTests()
    {
        _youTubeServiceMock = new Mock<IYouTubeService>();
        _loggerMock = new Mock<ILoggerService>();
        _service = new DownloadService(_youTubeServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new DownloadService(_youTubeServiceMock.Object, _loggerMock.Object);

        // Assert
        service.Should()
            .NotBeNull()
            .And.BeOfType<DownloadService>()
            .And.BeAssignableTo<IDownloadService>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenYouTubeServiceIsNull()
    {
        // Act & Assert
        var action = () => new DownloadService(null!, _loggerMock.Object);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("youTubeService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        var action = () => new DownloadService(_youTubeServiceMock.Object, null!);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}

