namespace YoutubeVideoDownloader.Console.Features.Download;

public class DownloadAndMergeHandler : IDownloadAndMergeService
{
    private readonly IDownloadService _downloadService;
    private readonly IFFmpegService _ffmpegService;
    private readonly ILoggerService _logger;

    public DownloadAndMergeHandler(
        IDownloadService downloadService,
        IFFmpegService ffmpegService,
        ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(downloadService);
        ArgumentNullException.ThrowIfNull(ffmpegService);
        ArgumentNullException.ThrowIfNull(logger);
        
        _downloadService = downloadService;
        _ffmpegService = ffmpegService;
        _logger = logger;
    }

    public async Task DownloadAndMergeAsync(
        IVideoStreamInfo videoStream, 
        IAudioStreamInfo audioStream, 
        string outputPath)
    {
        var tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{videoStream.Container}");
        var tempAudioPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{audioStream.Container}");
        
        try
        {
            _logger.LogInformation($"Starting download and merge: {outputPath}");
            
            // Download video and audio streams
            await _downloadService.DownloadWithProgressAsync(videoStream, tempVideoPath, "Downloading Video Stream");
            await _downloadService.DownloadWithProgressAsync(audioStream, tempAudioPath, "Downloading Audio Stream");
            
            // Merge video and audio
            _logger.LogInformation("Merging video and audio streams...");
            AnsiConsole.Write(new Align(new Markup("[cyan]Merging video and audio streams...[/]"), HorizontalAlignment.Center));
            
            await _ffmpegService.MergeVideoAndAudioAsync(tempVideoPath, tempAudioPath, outputPath);
            
            AnsiConsole.Write(new Align(new Markup("[bold green]✓ Merge completed![/]"), HorizontalAlignment.Center));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Download and merge failed: {outputPath}", ex);
            throw;
        }
        finally
        {
            // Clean up temp files
            try
            {
                if (File.Exists(tempVideoPath)) File.Delete(tempVideoPath);
                if (File.Exists(tempAudioPath)) File.Delete(tempAudioPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to clean up temp files: {ex.Message}");
            }
        }
    }
}
