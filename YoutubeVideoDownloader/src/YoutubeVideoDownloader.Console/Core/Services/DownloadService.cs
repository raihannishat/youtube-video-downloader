namespace YoutubeVideoDownloader.Console.Core.Services;

public class DownloadService : IDownloadService
{
    private readonly IYouTubeService _youTubeService;
    private readonly ILoggerService _logger;

    public DownloadService(IYouTubeService youTubeService, ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(youTubeService);
        ArgumentNullException.ThrowIfNull(logger);
        
        _youTubeService = youTubeService;
        _logger = logger;
    }

    public async Task DownloadWithProgressAsync(IStreamInfo streamInfo, string filePath, string label)
    {
        var totalBytes = streamInfo.Size.Bytes;
        var stopwatch = Stopwatch.StartNew();
        var lastBytes = 0L;
        var lastTime = DateTime.Now;
        
        _logger.LogInformation($"Download started: {label}, Size: {FileUtils.FormatFileSize(totalBytes)}");
        
        var downloadPanel = new Panel($"[bold yellow]{label}[/]\n\n[cyan]📄 File:[/] {Path.GetFileName(filePath)}\n[magenta]📊 Size:[/] {FileUtils.FormatFileSize(totalBytes)}");
        downloadPanel.Border = BoxBorder.Rounded;
        downloadPanel.BorderColor(Color.Cyan1);
        downloadPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(downloadPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
        
        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[cyan]{label}[/]", maxValue: 100);
                
                await _youTubeService.DownloadStreamAsync(streamInfo, filePath, new Progress<double>(progress =>
                {
                    var currentBytes = (long)(totalBytes * progress);
                    var downloadedBytes = currentBytes - lastBytes;
                    var elapsed = (DateTime.Now - lastTime).TotalSeconds;
                    
                    if (elapsed >= 0.1)
                    {
                        var speed = elapsed > 0 ? downloadedBytes / elapsed : 0;
                        var remainingBytes = totalBytes - currentBytes;
                        var avgSpeed = stopwatch.Elapsed.TotalSeconds > 0 ? currentBytes / stopwatch.Elapsed.TotalSeconds : 0;
                        var etaSeconds = avgSpeed > 0 ? remainingBytes / avgSpeed : 0;
                        
                        task.Value = progress * 100;
                        task.Description = $"[cyan]{label}[/] | [yellow]{FileUtils.FormatFileSize((long)speed)}/s[/] | [magenta]ETA: {FileUtils.FormatTime((long)etaSeconds)}[/]";
                        
                        lastBytes = currentBytes;
                        lastTime = DateTime.Now;
                    }
                }));
                
                task.Value = 100;
            });
        
        stopwatch.Stop();
        var elapsedTime = FileUtils.FormatTime((long)stopwatch.Elapsed.TotalSeconds);
        _logger.LogInformation($"Download completed in {elapsedTime}: {filePath}");
        AnsiConsole.Write(new Align(new Markup($"[bold green]✓ Download completed in {elapsedTime}[/]"), HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

}

