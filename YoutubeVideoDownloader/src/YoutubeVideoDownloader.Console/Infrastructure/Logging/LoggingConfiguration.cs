using Serilog.Events;

namespace YoutubeVideoDownloader.Console.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static ILogger ConfigureLogger()
    {
        var logPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "logs",
            "youtube-downloader-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}

