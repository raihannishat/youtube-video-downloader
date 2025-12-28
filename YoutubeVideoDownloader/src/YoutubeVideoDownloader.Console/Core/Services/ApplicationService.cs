namespace YoutubeVideoDownloader.Console.Core.Services;

public class ApplicationService : IApplicationService
{
    private readonly IYouTubeService _youTubeService;
    private readonly IDownloadService _downloadService;
    private readonly IDownloadAndMergeService _downloadAndMergeService;
    private readonly ILoggerService _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IDownloadHistoryService _downloadHistoryService;

    public ApplicationService(
        IYouTubeService youTubeService,
        IDownloadService downloadService,
        IDownloadAndMergeService downloadAndMergeService,
        ILoggerService logger,
        IConfigurationService configurationService,
        IDownloadHistoryService downloadHistoryService)
    {
        _youTubeService = youTubeService;
        _downloadService = downloadService;
        _downloadAndMergeService = downloadAndMergeService;
        _logger = logger;
        _configurationService = configurationService;
        _downloadHistoryService = downloadHistoryService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Application started");

        // Display header
        ConsoleUI.DisplayHeader();

        while (true)
        {
            try
            {
                var url = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold cyan]📺[/] Enter YouTube Video/Playlist URL\n[dim]('q'/'Q' for quit)[/]\n[dim]('a'/'i' for about)[/]\n[dim]('b'/'B' for batch)[/]\n[dim]('c'/'C' for config)[/]\n[dim]('h'/'H' for history)[/]\n[bold cyan]Please URL :[/]")
                        .PromptStyle("cyan")
                        .AllowEmpty());

                if (string.IsNullOrWhiteSpace(url))
                {
                    ConsoleUI.DisplayCenteredMessage("✗ Please enter a valid URL.", Color.Red);
                    _logger.LogWarning("Empty URL entered");
                    continue;
                }

                if (url.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested to quit");
                    ConsoleUI.DisplayCenteredPanel("[bold green]Thank you for using our app![/]\n[dim]See you again soon![/]", Color.Green);
                    break;
                }

                if (url.Equals("a", StringComparison.OrdinalIgnoreCase) || url.Equals("i", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested about page");
                    AboutHandler.ShowAboutPage();
                    continue;
                }

                if (url.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested batch download");
                    await HandleBatchDownloadAsync();
                    continue;
                }

                if (url.Equals("c", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested configuration");
                    HandleConfiguration();
                    continue;
                }

                if (url.Equals("h", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested download history");
                    HandleDownloadHistory();
                    continue;
                }

                // Normalize URL
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                _logger.LogInformation($"Processing URL: {url}");

                // Try to parse as playlist first, then video
                YoutubeExplode.Playlists.PlaylistId? playlistId = null;
                try
                {
                    playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(url);
                }
                catch
                {
                    // Not a playlist URL, continue with video parsing
                }

                if (playlistId != null)
                {
                    await HandlePlaylistDownloadAsync(url);
                    continue;
                }

                // Extract video ID and get video info
                var videoId = VideoId.Parse(url);
                
                var video = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync("Fetching video information...", async ctx =>
                    {
                        return await _youTubeService.GetVideoAsync(videoId);
                    });

                // Show video info if configured to do so
                var config = _configurationService.GetConfiguration();
                if (config.ShowVideoInfoBeforeDownload)
                {
                    VideoInfoHandler.DisplayVideoInfo(video);
                }

                // Get available streams
                var streamManifest = await _youTubeService.GetStreamManifestAsync(videoId);
                var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                // Display streams
                var table = StreamSelectionHandler.DisplayStreams(muxedStreams, videoStreams, audioStreams);
                
                if (muxedStreams.Count == 0 && audioStreams.Count == 0)
                {
                    _logger.LogWarning($"No streams available for video: {videoId}");
                    ConsoleUI.DisplayCenteredMessage("✗ No streams available for this video.", Color.Red);
                    continue;
                }

                AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
                
                var choice = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold cyan]🎯[/] Select quality (number or Enter for highest):")
                        .PromptStyle("cyan")
                        .AllowEmpty());

                // Get default download location
                var defaultDownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(defaultDownloadsFolder))
                {
                    defaultDownloadsFolder = Directory.GetCurrentDirectory();
                }

                // Get output directory from user
                var downloadsFolder = DirectorySelectionHandler.GetOutputDirectory(defaultDownloadsFolder, _logger);

                string outputFilePath;

                if (string.IsNullOrWhiteSpace(choice))
                {
                    // Default: highest quality
                    if (videoStreams.Count != 0 && audioStreams.Count != 0)
                    {
                        var bestVideo = videoStreams.First();
                        var bestAudio = audioStreams.First();
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                        _logger.LogInformation($"Downloading highest quality: {outputFilePath}");
                        await _downloadAndMergeService.DownloadAndMergeAsync(bestVideo, bestAudio, outputFilePath);
                    }
                    else if (muxedStreams.Count != 0)
                    {
                        var bestMuxed = muxedStreams.First();
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                        _logger.LogInformation($"Downloading muxed stream: {outputFilePath}");
                        await _downloadService.DownloadWithProgressAsync(bestMuxed, outputFilePath, "Downloading Video+Audio");
                    }
                    else if (audioStreams.Count != 0)
                    {
                        var bestAudio = audioStreams.First();
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestAudio.Container}");
                        _logger.LogInformation($"Downloading audio: {outputFilePath}");
                        await _downloadService.DownloadWithProgressAsync(bestAudio, outputFilePath, "Downloading Audio");
                    }
                    else
                    {
                        _logger.LogWarning("No streams available");
                        ConsoleUI.DisplayCenteredMessage("✗ No streams available.", Color.Red);
                        continue;
                    }
                }
                else if (choice.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                {
                    // Audio only
                    var audioIndex = int.Parse(choice.Substring(1)) - 1;
                    if (audioIndex >= 0 && audioIndex < audioStreams.Count)
                    {
                        var selectedAudio = audioStreams[audioIndex];
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                        _logger.LogInformation($"Downloading audio option {audioIndex + 1}: {outputFilePath}");
                        await _downloadService.DownloadWithProgressAsync(selectedAudio, outputFilePath, "Downloading Audio");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid audio selection: {choice}");
                        ConsoleUI.DisplayCenteredMessage("✗ Invalid selection.", Color.Red);
                        continue;
                    }
                }
                else
                {
                    // Video quality selection
                    var selectedIndex = int.Parse(choice) - 1;
                    var muxedCount = muxedStreams.Count;
                    
                    if (selectedIndex >= 0 && selectedIndex < muxedCount)
                    {
                        // Muxed stream selected
                        var selectedMuxed = muxedStreams[selectedIndex];
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                        _logger.LogInformation($"Downloading muxed stream option {selectedIndex + 1}: {outputFilePath}");
                        await _downloadService.DownloadWithProgressAsync(selectedMuxed, outputFilePath, "Downloading Video+Audio");
                    }
                    else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoStreams.Count && audioStreams.Count != 0)
                    {
                        // Video stream selected - will merge with audio
                        var videoIndex = selectedIndex - muxedCount;
                        var selectedVideo = videoStreams[videoIndex];
                        var bestAudio = audioStreams.First();
                        outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                        _logger.LogInformation($"Downloading and merging video option {videoIndex + 1}: {outputFilePath}");
                        await _downloadAndMergeService.DownloadAndMergeAsync(selectedVideo, bestAudio, outputFilePath);
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid selection: {choice}");
                        ConsoleUI.DisplayCenteredMessage("✗ Invalid selection.", Color.Red);
                        continue;
                    }
                }

                _logger.LogInformation($"Download completed successfully: {outputFilePath}");

                // Save to download history
                try
                {
                    var fileInfo = new FileInfo(outputFilePath);
                    var historyEntry = new DownloadHistoryEntry
                    {
                        VideoId = video.Id.ToString(),
                        VideoTitle = video.Title,
                        ChannelName = video.Author.ChannelTitle,
                        VideoUrl = url,
                        FilePath = outputFilePath,
                        Quality = choice == string.Empty ? "highest" : choice,
                        FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                        DownloadDate = DateTime.Now,
                        Duration = video.Duration ?? TimeSpan.Zero,
                        IsPlaylist = false
                    };
                    _downloadHistoryService.AddHistoryEntry(historyEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to save download history: {ex.Message}");
                }

                // Success message
                var successPanel = new Panel($"[bold green]✓ Successfully downloaded![/]\n\n[cyan]📁 File:[/] {Path.GetFileName(outputFilePath)}\n[cyan]📂 Location:[/] {outputFilePath}");
                successPanel.Border = BoxBorder.Rounded;
                successPanel.BorderColor(Color.Green);
                successPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (YoutubeExplode.Exceptions.PlaylistUnavailableException ex)
            {
                _logger.LogError($"Playlist unavailable: {ex.Message}", ex);
                var errorPanel = new Panel($"[bold red]✗ Playlist is unavailable or private.[/]\n\n[dim]Details: {ex.Message}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (YoutubeExplode.Exceptions.VideoUnavailableException ex)
            {
                _logger.LogError($"Video unavailable: {ex.Message}", ex);
                var errorPanel = new Panel($"[bold red]✗ Video is unavailable or private.[/]\n\n[dim]Details: {ex.Message}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (YoutubeExplode.Exceptions.VideoUnplayableException ex)
            {
                _logger.LogError($"Video unplayable: {ex.Message}", ex);
                var errorPanel = new Panel($"[bold red]✗ Video cannot be played or downloaded.[/]\n\n[dim]Details: {ex.Message}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (YoutubeExplode.Exceptions.RequestLimitExceededException ex)
            {
                _logger.LogError($"Request limit exceeded: {ex.Message}", ex);
                var errorPanel = new Panel($"[bold red]✗ YouTube request limit exceeded.[/]\n\n[dim]Please try again later.\nDetails: {ex.Message}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error: {ex.Message}", ex);
                var errorPanel = new Panel($"[bold red]✗ Network error occurred.[/]\n\n[dim]Please check your internet connection.\nDetails: {ex.Message}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}", ex);
                var innerMsg = ex.InnerException != null ? $"\n[dim]Inner: {ex.InnerException.Message}[/]" : "";
                var errorPanel = new Panel($"[bold red]✗ Error: {ex.Message}[/]\n\n[dim]Type: {ex.GetType().Name}{innerMsg}[/]");
                errorPanel.Border = BoxBorder.Rounded;
                errorPanel.BorderColor(Color.Red);
                errorPanel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
        }
    }

    private async Task HandlePlaylistDownloadAsync(string url)
    {
        try
        {
            // Extract playlist ID
            var playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(url);
            
            // Get playlist info
            var playlist = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Fetching playlist information...", async ctx =>
                {
                    return await _youTubeService.GetPlaylistAsync(playlistId);
                });

            PlaylistHandler.DisplayPlaylistInfo(playlist);

            // Get all videos from playlist first to count them
            var playlistVideos = new List<YoutubeExplode.Playlists.PlaylistVideo>();
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Fetching playlist videos...", async ctx =>
                {
                    await foreach (var playlistVideo in _youTubeService.GetPlaylistVideosAsync(playlistId))
                    {
                        playlistVideos.Add(playlistVideo);
                    }
                });

            _logger.LogInformation($"Found {playlistVideos.Count} videos in playlist");

            // Confirm download
            var confirm = PlaylistHandler.ConfirmPlaylistDownload(playlistVideos.Count);
            if (!confirm)
            {
                _logger.LogInformation("User cancelled playlist download");
                ConsoleUI.DisplayCenteredMessage("Playlist download cancelled.", Color.Yellow);
                return;
            }

            // Get first video to show available quality options
            if (playlistVideos.Count == 0)
            {
                _logger.LogWarning("Playlist is empty");
                ConsoleUI.DisplayCenteredMessage("✗ Playlist is empty.", Color.Red);
                return;
            }

            // Fetch first video to get stream options
            var firstVideo = await _youTubeService.GetVideoAsync(playlistVideos[0].Id);
            
            // Get available streams for quality selection
            var streamManifest = await _youTubeService.GetStreamManifestAsync(firstVideo.Id);
            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
            var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
            var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

            // Display available quality options
            if (muxedStreams.Count == 0 && videoStreams.Count == 0 && audioStreams.Count == 0)
            {
                _logger.LogWarning($"No streams available for sample video: {firstVideo.Id}");
                ConsoleUI.DisplayCenteredMessage("✗ No streams available for this playlist.", Color.Red);
                return;
            }

            var qualityTable = StreamSelectionHandler.DisplayStreams(muxedStreams, videoStreams, audioStreams);
            AnsiConsole.Write(new Align(qualityTable, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();

            // Get quality choice
            var qualityChoice = PlaylistHandler.GetQualityChoice();

            // Get default download location
            var defaultDownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(defaultDownloadsFolder))
            {
                defaultDownloadsFolder = Directory.GetCurrentDirectory();
            }

            // Get output directory from user
            var downloadsFolder = DirectorySelectionHandler.GetOutputDirectory(defaultDownloadsFolder, _logger);

            // Create playlist folder inside the selected directory
            var playlistFolder = Path.Combine(downloadsFolder, FileUtils.SanitizeFileName(playlist.Title));
            Directory.CreateDirectory(playlistFolder);
            _logger.LogInformation($"Created playlist folder: {playlistFolder}");

            // Convert PlaylistVideo to Video by fetching full video info
            var videos = new List<Video>();
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Fetching video details...", async ctx =>
                {
                    foreach (var playlistVideo in playlistVideos)
                    {
                        try
                        {
                            var video = await _youTubeService.GetVideoAsync(playlistVideo.Id);
                            videos.Add(video);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to fetch video {playlistVideo.Id}: {ex.Message}");
                        }
                    }
                });

            // Download each video
            var successCount = 0;
            var failCount = 0;
            var totalVideos = videos.Count;

            for (int i = 0; i < videos.Count; i++)
            {
                var video = videos[i];
                try
                {
                    AnsiConsole.WriteLine();
                    var progressPanel = new Panel($"[bold cyan]Downloading video {i + 1} of {totalVideos}[/]\n[dim]{video.Title}[/]");
                    progressPanel.Border = BoxBorder.Rounded;
                    progressPanel.BorderColor(Color.Cyan);
                    progressPanel.Padding = new Padding(1, 1);
                    AnsiConsole.Write(new Align(progressPanel, HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();

                    // Get stream manifest for this video
                    var videoStreamManifest = await _youTubeService.GetStreamManifestAsync(video.Id);
                    var videoMuxedStreams = videoStreamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    var videoVideoStreams = videoStreamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    var videoAudioStreams = videoStreamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                    if (videoMuxedStreams.Count == 0 && videoVideoStreams.Count == 0 && videoAudioStreams.Count == 0)
                    {
                        _logger.LogWarning($"No streams available for video: {video.Id}");
                        failCount++;
                        continue;
                    }

                    string? outputFilePath = null;

                    // Apply quality choice
                    if (string.IsNullOrWhiteSpace(qualityChoice))
                    {
                        // Default: highest quality
                        if (videoVideoStreams.Count != 0 && videoAudioStreams.Count != 0)
                        {
                            var bestVideo = videoVideoStreams.First();
                            var bestAudio = videoAudioStreams.First();
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                            await _downloadAndMergeService.DownloadAndMergeAsync(bestVideo, bestAudio, outputFilePath);
                        }
                        else if (videoMuxedStreams.Count != 0)
                        {
                            var bestMuxed = videoMuxedStreams.First();
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                            await _downloadService.DownloadWithProgressAsync(bestMuxed, outputFilePath, $"Downloading {i + 1}/{totalVideos}");
                        }
                        else if (videoAudioStreams.Count != 0)
                        {
                            var bestAudio = videoAudioStreams.First();
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestAudio.Container}");
                            await _downloadService.DownloadWithProgressAsync(bestAudio, outputFilePath, $"Downloading {i + 1}/{totalVideos}");
                        }
                        else
                        {
                            failCount++;
                            continue;
                        }
                    }
                    else if (qualityChoice.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                    {
                        // Audio only
                        var audioIndex = int.Parse(qualityChoice.Substring(1)) - 1;
                        if (audioIndex >= 0 && audioIndex < videoAudioStreams.Count)
                        {
                            var selectedAudio = videoAudioStreams[audioIndex];
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                            await _downloadService.DownloadWithProgressAsync(selectedAudio, outputFilePath, $"Downloading {i + 1}/{totalVideos}");
                        }
                        else
                        {
                            failCount++;
                            continue;
                        }
                    }
                    else
                    {
                        // Video quality selection
                        var selectedIndex = int.Parse(qualityChoice) - 1;
                        var muxedCount = videoMuxedStreams.Count;
                        
                        if (selectedIndex >= 0 && selectedIndex < muxedCount)
                        {
                            var selectedMuxed = videoMuxedStreams[selectedIndex];
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                            await _downloadService.DownloadWithProgressAsync(selectedMuxed, outputFilePath, $"Downloading {i + 1}/{totalVideos}");
                        }
                        else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoVideoStreams.Count && videoAudioStreams.Count != 0)
                        {
                            var videoIndex = selectedIndex - muxedCount;
                            var selectedVideo = videoVideoStreams[videoIndex];
                            var bestAudio = videoAudioStreams.First();
                            outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                            await _downloadAndMergeService.DownloadAndMergeAsync(selectedVideo, bestAudio, outputFilePath);
                        }
                        else
                        {
                            failCount++;
                            continue;
                        }
                    }

                    successCount++;
                    _logger.LogInformation($"Downloaded video {i + 1}/{totalVideos}: {video.Title}");

                    // Save to download history only if download was successful
                    if (outputFilePath != null)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(outputFilePath);
                            var historyEntry = new DownloadHistoryEntry
                            {
                                VideoId = video.Id.ToString(),
                                VideoTitle = video.Title,
                                ChannelName = video.Author.ChannelTitle,
                                VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                                FilePath = outputFilePath,
                                Quality = qualityChoice == string.Empty ? "highest" : qualityChoice,
                                FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                                DownloadDate = DateTime.Now,
                                Duration = video.Duration ?? TimeSpan.Zero,
                                IsPlaylist = true,
                                PlaylistTitle = playlist.Title
                            };
                            _downloadHistoryService.AddHistoryEntry(historyEntry);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to save download history for video {video.Title}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError($"Failed to download video {video.Title}: {ex.Message}", ex);
                }
            }

            // Summary
            var summaryPanel = new Panel(
                $"[bold green]✓ Playlist download completed![/]\n\n" +
                $"[cyan]📁 Folder:[/] {playlistFolder}\n" +
                $"[green]✓ Success:[/] {successCount} videos\n" +
                $"[red]✗ Failed:[/] {failCount} videos\n" +
                $"[yellow]📊 Total:[/] {totalVideos} videos");
            summaryPanel.Border = BoxBorder.Rounded;
            summaryPanel.BorderColor(successCount == totalVideos ? Color.Green : Color.Yellow);
            summaryPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(summaryPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
        catch (YoutubeExplode.Exceptions.PlaylistUnavailableException)
        {
            throw; // Re-throw to be handled by outer catch
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing playlist: {ex.Message}", ex);
            throw; // Re-throw to be handled by outer catch
        }
    }

    private async Task HandleBatchDownloadAsync()
    {
        try
        {
            // Get batch file path or URL
            var input = BatchDownloadHandler.GetBatchFileOrUrl();
            
            List<string> urls;
            string sourceInfo;

            // Check if input is a file or a direct URL
            if (File.Exists(input))
            {
                // Read URLs from file
                urls = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync("Reading URLs from file...", async ctx =>
                    {
                        return BatchDownloadHandler.ReadUrlsFromFile(input);
                    });

                sourceInfo = input;
            }
            else
            {
                // Treat as direct URL - normalize it
                var normalizedUrl = input;
                if (!normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedUrl = "https://" + normalizedUrl;
                }
                urls = new List<string> { normalizedUrl };
                sourceInfo = "Direct URL";
            }

            if (urls.Count == 0)
            {
                ConsoleUI.DisplayCenteredMessage("✗ No valid URLs found.", Color.Red);
                _logger.LogWarning($"No URLs found: {input}");
                return;
            }

            // If it's a single URL (not from file), check if it's a playlist
            // If it's a single video URL, redirect to normal video download flow
            if (!File.Exists(input) && urls.Count == 1)
            {
                var singleUrl = urls[0];
                
                // Try to parse as playlist
                YoutubeExplode.Playlists.PlaylistId? playlistId = null;
                try
                {
                    playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(singleUrl);
                }
                catch
                {
                    // Not a playlist URL
                }

                // If it's not a playlist, treat as single video and redirect to normal flow
                if (playlistId == null)
                {
                    // It's a single video URL - redirect to normal video download
                    _logger.LogInformation($"Single video URL detected in batch mode, redirecting to normal download: {singleUrl}");
                    
                    // Normalize URL if needed
                    if (!singleUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !singleUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        singleUrl = "https://" + singleUrl;
                    }

                    // Extract video ID and get video info
                    var videoId = VideoId.Parse(singleUrl);
                    
                    var video = await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Dots)
                        .SpinnerStyle(Style.Parse("cyan"))
                        .StartAsync("Fetching video information...", async ctx =>
                        {
                            return await _youTubeService.GetVideoAsync(videoId);
                        });

                    VideoInfoHandler.DisplayVideoInfo(video);

                    // Get available streams
                    var streamManifest = await _youTubeService.GetStreamManifestAsync(videoId);
                    var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                    // Display streams
                    var table = StreamSelectionHandler.DisplayStreams(muxedStreams, videoStreams, audioStreams);
                    
                    if (muxedStreams.Count == 0 && audioStreams.Count == 0)
                    {
                        _logger.LogWarning($"No streams available for video: {videoId}");
                        ConsoleUI.DisplayCenteredMessage("✗ No streams available for this video.", Color.Red);
                        return;
                    }

                    AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();
                    
                    var choice = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold cyan]🎯[/] Select quality (number or Enter for highest):")
                            .PromptStyle("cyan")
                            .AllowEmpty());

                    // Get default download location
                    var defaultVideoDownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    if (!Directory.Exists(defaultVideoDownloadsFolder))
                    {
                        defaultVideoDownloadsFolder = Directory.GetCurrentDirectory();
                    }

                    // Get output directory from user
                    var videoDownloadsFolder = DirectorySelectionHandler.GetOutputDirectory(defaultVideoDownloadsFolder, _logger);

                    string outputFilePath;

                    if (string.IsNullOrWhiteSpace(choice))
                    {
                        // Default: highest quality
                        if (videoStreams.Count != 0 && audioStreams.Count != 0)
                        {
                            var bestVideo = videoStreams.First();
                            var bestAudio = audioStreams.First();
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                            _logger.LogInformation($"Downloading highest quality: {outputFilePath}");
                            await _downloadAndMergeService.DownloadAndMergeAsync(bestVideo, bestAudio, outputFilePath);
                        }
                        else if (muxedStreams.Count != 0)
                        {
                            var bestMuxed = muxedStreams.First();
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                            _logger.LogInformation($"Downloading muxed stream: {outputFilePath}");
                            await _downloadService.DownloadWithProgressAsync(bestMuxed, outputFilePath, "Downloading Video+Audio");
                        }
                        else if (audioStreams.Count != 0)
                        {
                            var bestAudio = audioStreams.First();
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestAudio.Container}");
                            _logger.LogInformation($"Downloading audio: {outputFilePath}");
                            await _downloadService.DownloadWithProgressAsync(bestAudio, outputFilePath, "Downloading Audio");
                        }
                        else
                        {
                            _logger.LogWarning("No streams available");
                            ConsoleUI.DisplayCenteredMessage("✗ No streams available.", Color.Red);
                            return;
                        }
                    }
                    else if (choice.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                    {
                        // Audio only
                        var audioIndex = int.Parse(choice.Substring(1)) - 1;
                        if (audioIndex >= 0 && audioIndex < audioStreams.Count)
                        {
                            var selectedAudio = audioStreams[audioIndex];
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                            _logger.LogInformation($"Downloading audio option {audioIndex + 1}: {outputFilePath}");
                            await _downloadService.DownloadWithProgressAsync(selectedAudio, outputFilePath, "Downloading Audio");
                        }
                        else
                        {
                            _logger.LogWarning($"Invalid audio selection: {choice}");
                            ConsoleUI.DisplayCenteredMessage("✗ Invalid selection.", Color.Red);
                            return;
                        }
                    }
                    else
                    {
                        // Video quality selection
                        var selectedIndex = int.Parse(choice) - 1;
                        var muxedCount = muxedStreams.Count;
                        
                        if (selectedIndex >= 0 && selectedIndex < muxedCount)
                        {
                            // Muxed stream selected
                            var selectedMuxed = muxedStreams[selectedIndex];
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                            _logger.LogInformation($"Downloading muxed stream option {selectedIndex + 1}: {outputFilePath}");
                            await _downloadService.DownloadWithProgressAsync(selectedMuxed, outputFilePath, "Downloading Video+Audio");
                        }
                        else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoStreams.Count && audioStreams.Count != 0)
                        {
                            // Video stream selected - will merge with audio
                            var videoIndex = selectedIndex - muxedCount;
                            var selectedVideo = videoStreams[videoIndex];
                            var bestAudio = audioStreams.First();
                            outputFilePath = Path.Combine(videoDownloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                            _logger.LogInformation($"Downloading and merging video option {videoIndex + 1}: {outputFilePath}");
                            await _downloadAndMergeService.DownloadAndMergeAsync(selectedVideo, bestAudio, outputFilePath);
                        }
                        else
                        {
                            _logger.LogWarning($"Invalid selection: {choice}");
                            ConsoleUI.DisplayCenteredMessage("✗ Invalid selection.", Color.Red);
                            return;
                        }
                    }

                    _logger.LogInformation($"Download completed successfully: {outputFilePath}");

                    // Save to download history
                    try
                    {
                        var fileInfo = new FileInfo(outputFilePath);
                        var historyEntry = new DownloadHistoryEntry
                        {
                            VideoId = video.Id.ToString(),
                            VideoTitle = video.Title,
                            ChannelName = video.Author.ChannelTitle,
                            VideoUrl = singleUrl,
                            FilePath = outputFilePath,
                            Quality = choice == string.Empty ? "highest" : choice,
                            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                            DownloadDate = DateTime.Now,
                            Duration = video.Duration ?? TimeSpan.Zero,
                            IsPlaylist = false
                        };
                        _downloadHistoryService.AddHistoryEntry(historyEntry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to save download history: {ex.Message}");
                    }

                    // Success message
                    var successPanel = new Panel($"[bold green]✓ Successfully downloaded![/]\n\n[cyan]📁 File:[/] {Path.GetFileName(outputFilePath)}\n[cyan]📂 Location:[/] {outputFilePath}");
                    successPanel.Border = BoxBorder.Rounded;
                    successPanel.BorderColor(Color.Green);
                    successPanel.Padding = new Padding(1, 1);
                    AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();
                    
                    return; // Exit batch mode, video downloaded
                }
            }

            // Display batch info (for files or playlists)
            if (File.Exists(input))
            {
                BatchDownloadHandler.DisplayBatchInfo(urls.Count, sourceInfo);
            }

            // Confirm batch download (only for files or playlists)
            var confirm = BatchDownloadHandler.ConfirmBatchDownload(urls.Count);
            if (!confirm)
            {
                _logger.LogInformation("User cancelled batch download");
                ConsoleUI.DisplayCenteredMessage("Batch download cancelled.", Color.Yellow);
                return;
            }

            // Get default download location
            var defaultDownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(defaultDownloadsFolder))
            {
                defaultDownloadsFolder = Directory.GetCurrentDirectory();
            }

            // Get output directory from user
            var downloadsFolder = DirectorySelectionHandler.GetOutputDirectory(defaultDownloadsFolder, _logger);

            // Get quality options from first URL to show quality list
            if (urls.Count > 0)
            {
                var firstUrl = urls[0];
                // Normalize URL
                if (!firstUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !firstUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    firstUrl = "https://" + firstUrl;
                }

                // Try to get quality options from first URL
                try
                {
                    YoutubeExplode.Playlists.PlaylistId? playlistId = null;
                    try
                    {
                        playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(firstUrl);
                    }
                    catch
                    {
                        // Not a playlist URL
                    }

                    if (playlistId == null)
                    {
                        // It's a video URL - get stream manifest
                        var videoId = VideoId.Parse(firstUrl);
                        var streamManifest = await _youTubeService.GetStreamManifestAsync(videoId);
                        var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                        var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                        var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                        // Display quality options
                        if (muxedStreams.Count > 0 || videoStreams.Count > 0 || audioStreams.Count > 0)
                        {
                            var qualityTable = StreamSelectionHandler.DisplayStreams(muxedStreams, videoStreams, audioStreams);
                            AnsiConsole.Write(new Align(qualityTable, HorizontalAlignment.Center));
                            AnsiConsole.WriteLine();
                        }
                    }
                    else if (playlistId != null)
                    {
                        // It's a playlist URL - get first video from playlist
                        var playlist = await _youTubeService.GetPlaylistAsync(playlistId.Value);
                        
                        // Get first video from playlist
                        VideoId? firstVideoId = null;
                        await foreach (var playlistVideo in _youTubeService.GetPlaylistVideosAsync(playlistId.Value))
                        {
                            firstVideoId = playlistVideo.Id;
                            break; // Get only the first one
                        }

                        if (firstVideoId != null)
                        {
                            var streamManifest = await _youTubeService.GetStreamManifestAsync(firstVideoId.Value);
                            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                            var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                            var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                            // Display quality options
                            if (muxedStreams.Count > 0 || videoStreams.Count > 0 || audioStreams.Count > 0)
                            {
                                var qualityTable = StreamSelectionHandler.DisplayStreams(muxedStreams, videoStreams, audioStreams);
                                AnsiConsole.Write(new Align(qualityTable, HorizontalAlignment.Center));
                                AnsiConsole.WriteLine();
                            }
                        }
                    }
                }
                catch
                {
                    // If we can't get quality options, continue without showing the list
                }
            }

            // Get quality choice (will be applied to all videos)
            AnsiConsole.WriteLine();
            var qualityChoice = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold cyan]🎯[/] Select quality for all videos (number or Enter for highest):")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            // Process each URL
            var successCount = 0;
            var failCount = 0;
            var totalUrls = urls.Count;

            for (int i = 0; i < urls.Count; i++)
            {
                var currentUrl = urls[i];
                try
                {
                    AnsiConsole.WriteLine();
                    var progressPanel = new Panel($"[bold cyan]Processing URL {i + 1} of {totalUrls}[/]\n[dim]{currentUrl}[/]");
                    progressPanel.Border = BoxBorder.Rounded;
                    progressPanel.BorderColor(Color.Cyan);
                    progressPanel.Padding = new Padding(1, 1);
                    AnsiConsole.Write(new Align(progressPanel, HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();

                    // Normalize URL
                    if (!currentUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !currentUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        currentUrl = "https://" + currentUrl;
                    }

                    _logger.LogInformation($"Processing batch URL {i + 1}/{totalUrls}: {currentUrl}");

                    // Try to parse as playlist first, then video
                    YoutubeExplode.Playlists.PlaylistId? playlistId = null;
                    try
                    {
                        playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(currentUrl);
                    }
                    catch
                    {
                        // Not a playlist URL, continue with video parsing
                    }

                    if (playlistId != null)
                    {
                        // Handle playlist download
                        await HandlePlaylistDownloadFromBatchAsync(currentUrl, downloadsFolder, qualityChoice);
                        successCount++;
                    }
                    else
                    {
                        // Handle single video download
                        await HandleVideoDownloadFromBatchAsync(currentUrl, downloadsFolder, qualityChoice);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError($"Failed to process URL {currentUrl}: {ex.Message}", ex);
                    var errorMsg = new Panel($"[bold red]✗ Failed:[/] {currentUrl}\n[dim]{ex.Message}[/]");
                    errorMsg.Border = BoxBorder.Rounded;
                    errorMsg.BorderColor(Color.Red);
                    errorMsg.Padding = new Padding(1, 1);
                    AnsiConsole.Write(new Align(errorMsg, HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();
                }
            }

            // Summary
            AnsiConsole.WriteLine();
            var summaryPanel = new Panel(
                $"[bold green]✓ Batch download completed![/]\n\n" +
                $"[cyan]📁 Folder:[/] {downloadsFolder}\n" +
                $"[green]✓ Success:[/] {successCount} URLs\n" +
                $"[red]✗ Failed:[/] {failCount} URLs\n" +
                $"[yellow]📊 Total:[/] {totalUrls} URLs");
            summaryPanel.Border = BoxBorder.Rounded;
            summaryPanel.BorderColor(successCount == totalUrls ? Color.Green : Color.Yellow);
            summaryPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(summaryPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"Batch file not found: {ex.Message}", ex);
            var errorPanel = new Panel($"[bold red]✗ File not found.[/]\n\n[dim]{ex.Message}[/]");
            errorPanel.Border = BoxBorder.Rounded;
            errorPanel.BorderColor(Color.Red);
            errorPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing batch download: {ex.Message}", ex);
            var errorPanel = new Panel($"[bold red]✗ Error: {ex.Message}[/]");
            errorPanel.Border = BoxBorder.Rounded;
            errorPanel.BorderColor(Color.Red);
            errorPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }

    private async Task HandleVideoDownloadFromBatchAsync(string url, string downloadsFolder, string qualityChoice)
    {
        var videoId = VideoId.Parse(url);
        
        var video = await _youTubeService.GetVideoAsync(videoId);
        
        // Get available streams
        var streamManifest = await _youTubeService.GetStreamManifestAsync(videoId);
        var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
        var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
        var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

        if (muxedStreams.Count == 0 && videoStreams.Count == 0 && audioStreams.Count == 0)
        {
            throw new Exception("No streams available for this video");
        }

        string outputFilePath;

        // Apply quality choice
        if (string.IsNullOrWhiteSpace(qualityChoice))
        {
            // Default: highest quality
            if (videoStreams.Count != 0 && audioStreams.Count != 0)
            {
                var bestVideo = videoStreams.First();
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                await _downloadAndMergeService.DownloadAndMergeAsync(bestVideo, bestAudio, outputFilePath);
            }
            else if (muxedStreams.Count != 0)
            {
                var bestMuxed = muxedStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                await _downloadService.DownloadWithProgressAsync(bestMuxed, outputFilePath, "Downloading Video+Audio");
            }
            else if (audioStreams.Count != 0)
            {
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestAudio.Container}");
                await _downloadService.DownloadWithProgressAsync(bestAudio, outputFilePath, "Downloading Audio");
            }
            else
            {
                throw new Exception("No streams available");
            }
        }
        else if (qualityChoice.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        {
            // Audio only
            var audioIndex = int.Parse(qualityChoice.Substring(1)) - 1;
            if (audioIndex >= 0 && audioIndex < audioStreams.Count)
            {
                var selectedAudio = audioStreams[audioIndex];
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                await _downloadService.DownloadWithProgressAsync(selectedAudio, outputFilePath, "Downloading Audio");
            }
            else
            {
                throw new Exception("Invalid audio selection");
            }
        }
        else
        {
            // Video quality selection
            var selectedIndex = int.Parse(qualityChoice) - 1;
            var muxedCount = muxedStreams.Count;
            
            if (selectedIndex >= 0 && selectedIndex < muxedCount)
            {
                var selectedMuxed = muxedStreams[selectedIndex];
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                await _downloadService.DownloadWithProgressAsync(selectedMuxed, outputFilePath, "Downloading Video+Audio");
            }
            else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoStreams.Count && audioStreams.Count != 0)
            {
                var videoIndex = selectedIndex - muxedCount;
                var selectedVideo = videoStreams[videoIndex];
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                await _downloadAndMergeService.DownloadAndMergeAsync(selectedVideo, bestAudio, outputFilePath);
            }
            else
            {
                throw new Exception("Invalid selection");
            }
        }

        _logger.LogInformation($"Batch video downloaded: {outputFilePath}");

        // Save to download history
        try
        {
            var fileInfo = new FileInfo(outputFilePath);
            var historyEntry = new DownloadHistoryEntry
            {
                VideoId = video.Id.ToString(),
                VideoTitle = video.Title,
                ChannelName = video.Author.ChannelTitle,
                VideoUrl = url,
                FilePath = outputFilePath,
                Quality = qualityChoice == string.Empty ? "highest" : qualityChoice,
                FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                DownloadDate = DateTime.Now,
                Duration = video.Duration ?? TimeSpan.Zero,
                IsPlaylist = false
            };
            _downloadHistoryService.AddHistoryEntry(historyEntry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to save download history: {ex.Message}");
        }
    }

    private async Task HandlePlaylistDownloadFromBatchAsync(string url, string downloadsFolder, string qualityChoice)
    {
        var playlistId = YoutubeExplode.Playlists.PlaylistId.Parse(url);
        var playlist = await _youTubeService.GetPlaylistAsync(playlistId);

        // Get all videos from playlist
        var playlistVideos = new List<YoutubeExplode.Playlists.PlaylistVideo>();
        await foreach (var playlistVideo in _youTubeService.GetPlaylistVideosAsync(playlistId))
        {
            playlistVideos.Add(playlistVideo);
        }

        if (playlistVideos.Count == 0)
        {
            throw new Exception("Playlist is empty");
        }

        // Create playlist folder
        var playlistFolder = Path.Combine(downloadsFolder, FileUtils.SanitizeFileName(playlist.Title));
        Directory.CreateDirectory(playlistFolder);

        // Convert PlaylistVideo to Video
        var videos = new List<Video>();
        foreach (var playlistVideo in playlistVideos)
        {
            try
            {
                var video = await _youTubeService.GetVideoAsync(playlistVideo.Id);
                videos.Add(video);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to fetch video {playlistVideo.Id}: {ex.Message}");
            }
        }

        // Download each video
        for (int i = 0; i < videos.Count; i++)
        {
            var video = videos[i];
            try
            {
                // Get stream manifest
                var videoStreamManifest = await _youTubeService.GetStreamManifestAsync(video.Id);
                var videoMuxedStreams = videoStreamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
                var videoVideoStreams = videoStreamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                var videoAudioStreams = videoStreamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

                if (videoMuxedStreams.Count == 0 && videoVideoStreams.Count == 0 && videoAudioStreams.Count == 0)
                {
                    continue;
                }

                string? outputFilePath = null;

                // Apply quality choice (same logic as regular playlist download)
                if (string.IsNullOrWhiteSpace(qualityChoice))
                {
                    if (videoVideoStreams.Count != 0 && videoAudioStreams.Count != 0)
                    {
                        var bestVideo = videoVideoStreams.First();
                        var bestAudio = videoAudioStreams.First();
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                        await _downloadAndMergeService.DownloadAndMergeAsync(bestVideo, bestAudio, outputFilePath);
                    }
                    else if (videoMuxedStreams.Count != 0)
                    {
                        var bestMuxed = videoMuxedStreams.First();
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                        await _downloadService.DownloadWithProgressAsync(bestMuxed, outputFilePath, $"Downloading {i + 1}/{videos.Count}");
                    }
                    else if (videoAudioStreams.Count != 0)
                    {
                        var bestAudio = videoAudioStreams.First();
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{bestAudio.Container}");
                        await _downloadService.DownloadWithProgressAsync(bestAudio, outputFilePath, $"Downloading {i + 1}/{videos.Count}");
                    }
                }
                else if (qualityChoice.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                {
                    var audioIndex = int.Parse(qualityChoice.Substring(1)) - 1;
                    if (audioIndex >= 0 && audioIndex < videoAudioStreams.Count)
                    {
                        var selectedAudio = videoAudioStreams[audioIndex];
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                        await _downloadService.DownloadWithProgressAsync(selectedAudio, outputFilePath, $"Downloading {i + 1}/{videos.Count}");
                    }
                }
                else
                {
                    var selectedIndex = int.Parse(qualityChoice) - 1;
                    var muxedCount = videoMuxedStreams.Count;
                    
                    if (selectedIndex >= 0 && selectedIndex < muxedCount)
                    {
                        var selectedMuxed = videoMuxedStreams[selectedIndex];
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                        await _downloadService.DownloadWithProgressAsync(selectedMuxed, outputFilePath, $"Downloading {i + 1}/{videos.Count}");
                    }
                    else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoVideoStreams.Count && videoAudioStreams.Count != 0)
                    {
                        var videoIndex = selectedIndex - muxedCount;
                        var selectedVideo = videoVideoStreams[videoIndex];
                        var bestAudio = videoAudioStreams.First();
                        outputFilePath = Path.Combine(playlistFolder, $"{FileUtils.SanitizeFileName(video.Title)}.mp4");
                        await _downloadAndMergeService.DownloadAndMergeAsync(selectedVideo, bestAudio, outputFilePath);
                    }
                }

                // Save to download history only if download was successful
                if (outputFilePath != null)
                {
                    try
                    {
                        var fileInfo = new FileInfo(outputFilePath);
                        var historyEntry = new DownloadHistoryEntry
                        {
                            VideoId = video.Id.ToString(),
                            VideoTitle = video.Title,
                            ChannelName = video.Author.ChannelTitle,
                            VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                            FilePath = outputFilePath,
                            Quality = qualityChoice == string.Empty ? "highest" : qualityChoice,
                            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                            DownloadDate = DateTime.Now,
                            Duration = video.Duration ?? TimeSpan.Zero,
                            IsPlaylist = true,
                            PlaylistTitle = playlist.Title
                        };
                        _downloadHistoryService.AddHistoryEntry(historyEntry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to save download history for video {video.Title}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to download video {video.Title}: {ex.Message}");
            }
        }
    }

    private void HandleConfiguration()
    {
        AnsiConsole.WriteLine();
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold cyan]⚙️ Configuration Menu[/]")
                .AddChoices(new[] { "View Configuration", "Edit Configuration", "Reset to Defaults", "Back" }));

        switch (action)
        {
            case "View Configuration":
                ConfigurationHandler.ShowConfiguration(_configurationService);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                break;

            case "Edit Configuration":
                ConfigurationHandler.EditConfiguration(_configurationService);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                break;

            case "Reset to Defaults":
                ConfigurationHandler.ResetConfiguration(_configurationService);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                break;

            case "Back":
                break;
        }
    }

    private void HandleDownloadHistory()
    {
        AnsiConsole.WriteLine();
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold cyan]📜 Download History Menu[/]")
                .AddChoices(new[] { "View History (Summary)", "View History (Detailed)", "View Statistics", "Clear History", "Back" }));

        switch (action)
        {
            case "View History (Summary)":
                DownloadHistoryHandler.ShowHistory(_downloadHistoryService);
                break;

            case "View History (Detailed)":
                DownloadHistoryHandler.ShowDetailedHistory(_downloadHistoryService);
                break;

            case "View Statistics":
                DownloadHistoryHandler.ShowHistoryStats(_downloadHistoryService);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                break;

            case "Clear History":
                DownloadHistoryHandler.ClearHistory(_downloadHistoryService);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                break;

            case "Back":
                break;
        }
    }
}

