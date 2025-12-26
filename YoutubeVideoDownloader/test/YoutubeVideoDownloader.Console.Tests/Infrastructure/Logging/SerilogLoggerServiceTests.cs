using YoutubeVideoDownloader.Console.Infrastructure.Logging;

namespace YoutubeVideoDownloader.Console.Tests.Infrastructure.Logging;

public class SerilogLoggerServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly SerilogLoggerService _service;

    public SerilogLoggerServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _service = new SerilogLoggerService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new SerilogLoggerService(_loggerMock.Object);

        // Assert
        service.Should()
            .NotBeNull()
            .And.BeOfType<SerilogLoggerService>()
            .And.BeAssignableTo<ILoggerService>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        var action = () => new SerilogLoggerService(null!);
        
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void LogInformation_ShouldCallLogger()
    {
        // Arrange
        var message = "Test message";

        // Act
        _service.LogInformation(message);

        // Assert
        _loggerMock.Verify(x => x.Information(message), Times.Once);
        _service.Should().NotBeNull();
    }

    [Fact]
    public void LogInformation_ShouldNotThrowException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var action = () => _service.LogInformation(message);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void LogWarning_ShouldCallLogger()
    {
        // Arrange
        var message = "Warning message";

        // Act
        _service.LogWarning(message);

        // Assert
        _loggerMock.Verify(x => x.Warning(message), Times.Once);
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LogWarning_ShouldNotThrowException()
    {
        // Arrange
        var message = "Warning message";

        // Act
        var action = () => _service.LogWarning(message);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void LogError_ShouldCallLogger_WhenNoException()
    {
        // Arrange
        var message = "Error message";

        // Act
        _service.LogError(message);

        // Assert
        _loggerMock.Verify(x => x.Error(message), Times.Once);
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LogError_ShouldCallLogger_WhenExceptionProvided()
    {
        // Arrange
        var message = "Error message";
        var exception = new Exception("Test exception");

        // Act
        _service.LogError(message, exception);

        // Assert
        _loggerMock.Verify(x => x.Error(exception, message), Times.Once);
        exception.Should().NotBeNull();
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LogError_ShouldNotThrowException_WhenExceptionIsNull()
    {
        // Arrange
        var message = "Error message";

        // Act
        var action = () => _service.LogError(message, null);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void LogDebug_ShouldCallLogger()
    {
        // Arrange
        var message = "Debug message";

        // Act
        _service.LogDebug(message);

        // Assert
        _loggerMock.Verify(x => x.Debug(message), Times.Once);
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LogDebug_ShouldNotThrowException()
    {
        // Arrange
        var message = "Debug message";

        // Act
        var action = () => _service.LogDebug(message);

        // Assert
        action.Should().NotThrow();
    }
}

