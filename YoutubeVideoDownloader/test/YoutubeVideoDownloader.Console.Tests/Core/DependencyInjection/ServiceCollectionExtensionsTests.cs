namespace YoutubeVideoDownloader.Console.Tests.Core.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IYouTubeService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<IYouTubeService>();
            
        serviceProvider.GetService<IDownloadService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<IDownloadService>();
            
        serviceProvider.GetService<IFFmpegService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<IFFmpegService>();
            
        serviceProvider.GetService<IDownloadAndMergeService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<IDownloadAndMergeService>();
            
        serviceProvider.GetService<IApplicationService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<IApplicationService>();
            
        serviceProvider.GetService<ILoggerService>()
            .Should().NotBeNull()
            .And.BeAssignableTo<ILoggerService>();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterServicesAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Get same instance twice
        var service1 = serviceProvider.GetService<IYouTubeService>();
        var service2 = serviceProvider.GetService<IYouTubeService>();
        
        service1.Should()
            .NotBeNull()
            .And.BeSameAs(service2);
            
        service2.Should()
            .NotBeNull()
            .And.BeSameAs(service1);
    }

    [Fact]
    public void AddApplicationServices_ShouldNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var action = () => services.AddApplicationServices();

        // Assert
        action.Should().NotThrow();
        services.Should().NotBeEmpty();
    }
}

