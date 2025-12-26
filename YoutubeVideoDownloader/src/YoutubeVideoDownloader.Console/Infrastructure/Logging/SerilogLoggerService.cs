namespace YoutubeVideoDownloader.Console.Infrastructure.Logging;

public class SerilogLoggerService : ILoggerService
{
    private readonly ILogger _logger;

    public SerilogLoggerService(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        _logger = logger;
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
            _logger.Error(exception, message);
        else
            _logger.Error(message);
    }

    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }
}

