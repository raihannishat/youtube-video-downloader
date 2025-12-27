namespace YoutubeVideoDownloader.Console.Core.Services;

public class ApplicationService : IApplicationService
{
    private readonly IYouTubeService _youTubeService;
    private readonly IDownloadService _downloadService;
    private readonly IDownloadAndMergeService _downloadAndMergeService;
    private readonly ILoggerService _logger;

    public ApplicationService(
        IYouTubeService youTubeService,
        IDownloadService downloadService,
        IDownloadAndMergeService downloadAndMergeService,
        ILoggerService logger)
    {
        _youTubeService = youTubeService;
        _downloadService = downloadService;
        _downloadAndMergeService = downloadAndMergeService;
        _logger = logger;
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
                    new TextPrompt<string>("[bold cyan]📺[/] Enter YouTube Video/Playlist URL (or 'q'/'Q' to quit, 'a'/'i' for about):")
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

                    string outputFilePath;

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
}

