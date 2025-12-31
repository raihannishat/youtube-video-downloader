using Serilog.Events;

namespace YoutubeVideoDownloader.Console.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static ILogger ConfigureLogger()
    {
        // Determine log directory - use AppData if installed in Program Files
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string logDirectory;
        
        // Check if we're in Program Files (requires admin to write)
        if (baseDirectory.Contains("Program Files", StringComparison.OrdinalIgnoreCase))
        {
            // Use user's AppData folder for logs (same as config and history)
            logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YoutubeVideoDownloader",
                "logs");
        }
        else
        {
            // Use application directory for portable/development installations
            logDirectory = Path.Combine(baseDirectory, "logs");
        }
        
        // Ensure log directory exists
        Directory.CreateDirectory(logDirectory);
        
        var logPath = Path.Combine(logDirectory, "youtube-downloader-.log");

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

