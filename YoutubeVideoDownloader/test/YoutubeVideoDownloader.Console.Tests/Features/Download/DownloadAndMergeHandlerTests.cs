namespace YoutubeVideoDownloader.Console.Tests.Features.Download;

public class DownloadAndMergeHandlerTests
{
    private readonly Mock<IDownloadService> _downloadServiceMock;
    private readonly Mock<IFFmpegService> _ffmpegServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly DownloadAndMergeHandler _handler;

    public DownloadAndMergeHandlerTests()
    {
        _downloadServiceMock = new Mock<IDownloadService>();
        _ffmpegServiceMock = new Mock<IFFmpegService>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new DownloadAndMergeHandler(
            _downloadServiceMock.Object,
            _ffmpegServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeHandler()
    {
        // Act
        var handler = new DownloadAndMergeHandler(
            _downloadServiceMock.Object,
            _ffmpegServiceMock.Object,
            _loggerMock.Object);

        // Assert
        handler.Should()
            .NotBeNull()
            .And.BeOfType<DownloadAndMergeHandler>()
            .And.BeAssignableTo<IDownloadAndMergeService>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDownloadServiceIsNull()
    {
        // Act & Assert
        var action = () => new DownloadAndMergeHandler(
            null!,
            _ffmpegServiceMock.Object,
            _loggerMock.Object);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("downloadService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFFmpegServiceIsNull()
    {
        // Act & Assert
        var action = () => new DownloadAndMergeHandler(
            _downloadServiceMock.Object,
            null!,
            _loggerMock.Object);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("ffmpegService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        var action = () => new DownloadAndMergeHandler(
            _downloadServiceMock.Object,
            _ffmpegServiceMock.Object,
            null!);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}

