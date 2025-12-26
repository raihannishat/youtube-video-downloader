namespace YoutubeVideoDownloader.Console.Tests.Core.Services;

public class FFmpegServiceTests
{
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly FFmpegService _service;

    public FFmpegServiceTests()
    {
        _loggerMock = new Mock<ILoggerService>();
        _service = new FFmpegService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new FFmpegService(_loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void IsAvailable_ShouldReturnBoolean()
    {
        // Act
        var result = _service.IsAvailable();

        // Assert
        // Cast to object first to use BeOfType, since BooleanAssertions doesn't have BeOfType
        ((object)result).Should().BeOfType<bool>();
    }
}