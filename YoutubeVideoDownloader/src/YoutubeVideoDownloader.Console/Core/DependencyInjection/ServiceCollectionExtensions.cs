namespace YoutubeVideoDownloader.Console.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Configure Serilog
        var logger = LoggingConfiguration.ConfigureLogger();
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<ILoggerService, SerilogLoggerService>();

        // Register configuration service first (others may depend on it)
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Register download history service
        services.AddSingleton<IDownloadHistoryService, DownloadHistoryService>();

        // Register services
        services.AddSingleton<IYouTubeService, YouTubeService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IFFmpegService, FFmpegService>();
        services.AddSingleton<IDownloadAndMergeService, DownloadAndMergeHandler>();
        services.AddSingleton<IApplicationService, ApplicationService>();

        return services;
    }
}

