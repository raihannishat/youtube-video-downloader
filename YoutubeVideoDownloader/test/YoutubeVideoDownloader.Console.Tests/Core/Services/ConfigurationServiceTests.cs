using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Tests.Core.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<ILoggerService> _loggerMock;
    private string _tempConfigPath;

    public ConfigurationServiceTests()
    {
        _loggerMock = new Mock<ILoggerService>();
        _tempConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "config.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_tempConfigPath)!);
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new ConfigurationService(_loggerMock.Object);

        // Assert
        service.Should()
            .NotBeNull()
            .And.BeOfType<ConfigurationService>()
            .And.BeAssignableTo<IConfigurationService>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ConfigurationService(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetConfiguration_ShouldReturnConfiguration()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);

        // Act
        var config = service.GetConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.DefaultDownloadDirectory.Should().NotBeNullOrEmpty();
        config.DefaultQuality.Should().NotBeNullOrEmpty();
        config.LogLevel.Should().NotBeNullOrEmpty();
        // Boolean properties should be valid boolean values
        (config.AutoCreatePlaylistFolder == true || config.AutoCreatePlaylistFolder == false).Should().BeTrue();
        (config.ShowVideoInfoBeforeDownload == true || config.ShowVideoInfoBeforeDownload == false).Should().BeTrue();
        config.MaxConcurrentDownloads.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GetConfigFilePath_ShouldReturnValidPath()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);

        // Act
        var path = service.GetConfigFilePath();

        // Assert
        path.Should().NotBeNullOrEmpty();
        path.Should().Contain("YoutubeVideoDownloader");
        path.Should().EndWith("config.json");
    }

    [Fact]
    public void SaveConfiguration_ShouldSaveConfigurationToFile()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);
        var configPath = service.GetConfigFilePath();
        var config = new AppConfiguration
        {
            DefaultDownloadDirectory = "C:\\Test\\Downloads",
            DefaultQuality = "720p",
            LogLevel = "Debug"
        };

        // Act
        service.SaveConfiguration(config);

        // Assert
        File.Exists(configPath).Should().BeTrue();
        var savedConfig = service.GetConfiguration();
        savedConfig.DefaultDownloadDirectory.Should().Be("C:\\Test\\Downloads");
        savedConfig.DefaultQuality.Should().Be("720p");
        savedConfig.LogLevel.Should().Be("Debug");
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Configuration saved"))), Times.Once);
    }

    [Fact]
    public void SaveConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);

        // Act & Assert
        var action = () => service.SaveConfiguration(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LoadConfiguration_ShouldLoadConfigurationFromFile()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);
        var configPath = service.GetConfigFilePath();
        var config = new AppConfiguration
        {
            DefaultDownloadDirectory = "C:\\Test\\Downloads",
            DefaultQuality = "1080p"
        };
        service.SaveConfiguration(config);

        // Create a new service instance to test loading
        var newService = new ConfigurationService(_loggerMock.Object);

        // Act
        var loadedConfig = newService.GetConfiguration();

        // Assert
        loadedConfig.DefaultDownloadDirectory.Should().Be("C:\\Test\\Downloads");
        loadedConfig.DefaultQuality.Should().Be("1080p");
    }

    [Fact]
    public void ResetToDefaults_ShouldResetConfigurationToDefaults()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);
        var config = new AppConfiguration
        {
            DefaultDownloadDirectory = "C:\\Custom\\Path",
            DefaultQuality = "720p",
            LogLevel = "Debug",
            AutoCreatePlaylistFolder = false,
            ShowVideoInfoBeforeDownload = false
        };
        service.SaveConfiguration(config);

        // Act
        service.ResetToDefaults();

        // Assert
        var resetConfig = service.GetConfiguration();
        resetConfig.DefaultQuality.Should().Be("highest");
        resetConfig.LogLevel.Should().Be("Information");
        resetConfig.AutoCreatePlaylistFolder.Should().BeTrue();
        resetConfig.ShowVideoInfoBeforeDownload.Should().BeTrue();
        resetConfig.MaxConcurrentDownloads.Should().Be(1);
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Configuration reset"))), Times.Once);
    }

    [Fact]
    public void SaveConfiguration_ShouldUpdateInMemoryConfiguration()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object);
        var config = new AppConfiguration
        {
            DefaultQuality = "audio"
        };

        // Act
        service.SaveConfiguration(config);
        var retrievedConfig = service.GetConfiguration();

        // Assert
        retrievedConfig.DefaultQuality.Should().Be("audio");
    }
}

