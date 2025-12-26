using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics;
using FFMpegCore;
using System.Net.Http;
using System.IO.Compression;
using System.Text;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

// Display beautiful header using Spectre.Console
AnsiConsole.Write(
    new FigletText("YOUTUBE")
        .Centered()
        .Color(Color.Cyan1));

var panel = new Panel("[bold cyan]Video Downloader with Auto-Merge[/]");
panel.Border = BoxBorder.Rounded;
panel.BorderColor(Color.Cyan1);
panel.Padding = new Padding(1, 1);
AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center));

AnsiConsole.WriteLine();

// Setup FFmpeg (embedded - no external installation needed)
await SetupFFmpegAsync();

var youtube = new YoutubeClient();

while (true)
{
    try
    {
        var url = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold cyan]📺[/] Enter YouTube Video URL (or 'q'/'Q' to quit, 'a'/'i' for information):")
                .PromptStyle("cyan")
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(url))
        {
            AnsiConsole.Write(new Align(new Markup("[red]✗ Please enter a valid URL.[/]"), HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            continue;
        }

        if (url.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            var goodbyePanel = new Panel("[bold green]Thank you for using our app![/]\n[dim]See you again soon![/]");
            goodbyePanel.Border = BoxBorder.Rounded;
            goodbyePanel.BorderColor(Color.Green);
            goodbyePanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(goodbyePanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            break;
        }

        if (url.Equals("a", StringComparison.OrdinalIgnoreCase) || url.Equals("i", StringComparison.OrdinalIgnoreCase))
        {
            ShowAboutPage();
            continue;
        }

        // Normalize URL - add https:// if missing
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        // Extract video ID
        var videoId = YoutubeExplode.Videos.VideoId.Parse(url);
        
        var video = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Fetching video information...", async ctx =>
            {
                return await youtube.Videos.GetAsync(videoId);
            });

        PrintVideoInfo(video);

        // Get available streams
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();
        var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
        var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();

        // Display available options using Spectre.Console Table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold cyan]Available Quality Options[/]");
        table.AddColumn(new TableColumn("[bold]Option[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Quality[/]"));
        table.AddColumn(new TableColumn("[bold]Format[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Size[/]").RightAligned());
        
        int optionIndex = 1;
        
        if (muxedStreams.Any())
        {
            table.AddRow("", "[bold yellow]Video + Audio (Ready to Download)[/]", "", "");
            foreach (var stream in muxedStreams)
            {
                table.AddRow(
                    $"[cyan]{optionIndex}[/]",
                    $"[green]✓[/] {stream.VideoQuality.Label}",
                    $"[dim]{stream.Container}[/]",
                    FormatFileSize((long)(stream.Size.MegaBytes * 1024 * 1024)));
                optionIndex++;
            }
        }
        
        if (videoStreams.Any() && audioStreams.Any())
        {
            table.AddRow("", "[bold yellow]Higher Quality (Auto-merged)[/]", "", "");
            foreach (var stream in videoStreams)
            {
                var estimatedSize = stream.Size.MegaBytes + (audioStreams.FirstOrDefault()?.Size.MegaBytes ?? 0);
                table.AddRow(
                    $"[cyan]{optionIndex}[/]",
                    $"[yellow]🔄[/] {stream.VideoQuality.Label}",
                    $"[dim]{stream.Container}[/]",
                    $"~{FormatFileSize((long)(estimatedSize * 1024 * 1024))}");
                optionIndex++;
            }
        }

        if (audioStreams.Any())
        {
            table.AddRow("", "[bold yellow]Audio Only[/]", "", "");
            for (int i = 0; i < audioStreams.Count; i++)
            {
                var stream = audioStreams[i];
                table.AddRow(
                    $"[cyan]A{i + 1}[/]",
                    $"[magenta]🎵[/] {stream.Bitrate} kbps",
                    $"[dim]{stream.Container}[/]",
                    FormatFileSize((long)(stream.Size.MegaBytes * 1024 * 1024)));
            }
        }
        
        if (!muxedStreams.Any() && !audioStreams.Any())
        {
            AnsiConsole.Write(new Align(new Markup("[red]✗ No streams available for this video.[/]"), HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            continue;
        }

        AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
        
        var choice = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold cyan]🎯[/] Select quality (number or Enter for highest):")
                .PromptStyle("cyan")
                .AllowEmpty());

        // Download location
        var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (!Directory.Exists(downloadsFolder))
        {
            downloadsFolder = Directory.GetCurrentDirectory();
        }

        string outputFilePath;

        if (string.IsNullOrWhiteSpace(choice))
        {
            // Default: highest quality (prefer video+audio merge over muxed)
            if (videoStreams.Any() && audioStreams.Any())
            {
                var bestVideo = videoStreams.First();
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.mp4");
                await DownloadAndMergeAsync(youtube, bestVideo, bestAudio, outputFilePath);
            }
            else if (muxedStreams.Any())
            {
                var bestMuxed = muxedStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.{bestMuxed.Container}");
                await DownloadWithProgressAsync(youtube, bestMuxed, outputFilePath, "Downloading Video+Audio");
            }
            else if (audioStreams.Any())
            {
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.{bestAudio.Container}");
                await DownloadWithProgressAsync(youtube, bestAudio, outputFilePath, "Downloading Audio");
            }
            else
            {
                Console.WriteLine("No streams available.\n");
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
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.{selectedAudio.Container}");
                await DownloadWithProgressAsync(youtube, selectedAudio, outputFilePath, "Downloading Audio");
            }
            else
            {
                AnsiConsole.Write(new Align(new Markup("[red]✗ Invalid selection.[/]"), HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
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
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.{selectedMuxed.Container}");
                await DownloadWithProgressAsync(youtube, selectedMuxed, outputFilePath, "Downloading Video+Audio");
            }
            else if (selectedIndex >= muxedCount && selectedIndex < muxedCount + videoStreams.Count && audioStreams.Any())
            {
                // Video stream selected - will merge with audio
                var videoIndex = selectedIndex - muxedCount;
                var selectedVideo = videoStreams[videoIndex];
                var bestAudio = audioStreams.First();
                outputFilePath = Path.Combine(downloadsFolder, $"{SanitizeFileName(video.Title)}.mp4");
                await DownloadAndMergeAsync(youtube, selectedVideo, bestAudio, outputFilePath);
            }
            else
            {
                AnsiConsole.Write(new Align(new Markup("[red]✗ Invalid selection.[/]"), HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
                continue;
            }
        }

        var successPanel = new Panel($"[bold green]✓ Successfully downloaded![/]\n\n[cyan]📁 File:[/] {Path.GetFileName(outputFilePath)}\n[cyan]📂 Location:[/] {outputFilePath}");
        successPanel.Border = BoxBorder.Rounded;
        successPanel.BorderColor(Color.Green);
        successPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
    catch (YoutubeExplode.Exceptions.VideoUnavailableException ex)
    {
        var errorPanel = new Panel($"[bold red]✗ Video is unavailable or private.[/]\n\n[dim]Details: {ex.Message}[/]");
        errorPanel.Border = BoxBorder.Rounded;
        errorPanel.BorderColor(Color.Red);
        errorPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
    catch (YoutubeExplode.Exceptions.VideoUnplayableException ex)
    {
        var errorPanel = new Panel($"[bold red]✗ Video cannot be played or downloaded.[/]\n\n[dim]Details: {ex.Message}[/]");
        errorPanel.Border = BoxBorder.Rounded;
        errorPanel.BorderColor(Color.Red);
        errorPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
    catch (YoutubeExplode.Exceptions.RequestLimitExceededException ex)
    {
        var errorPanel = new Panel($"[bold red]✗ YouTube request limit exceeded.[/]\n\n[dim]Please try again later.\nDetails: {ex.Message}[/]");
        errorPanel.Border = BoxBorder.Rounded;
        errorPanel.BorderColor(Color.Red);
        errorPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
    catch (System.Net.Http.HttpRequestException ex)
    {
        var errorPanel = new Panel($"[bold red]✗ Network error occurred.[/]\n\n[dim]Please check your internet connection.\nDetails: {ex.Message}[/]");
        errorPanel.Border = BoxBorder.Rounded;
        errorPanel.BorderColor(Color.Red);
        errorPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
    catch (Exception ex)
    {
        var innerMsg = ex.InnerException != null ? $"\n[dim]Inner: {ex.InnerException.Message}[/]" : "";
        var errorPanel = new Panel($"[bold red]✗ Error: {ex.Message}[/]\n\n[dim]Type: {ex.GetType().Name}{innerMsg}[/]");
        errorPanel.Border = BoxBorder.Rounded;
        errorPanel.BorderColor(Color.Red);
        errorPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
}

static string SanitizeFileName(string fileName)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
        .TrimEnd('.');
}

static string FormatFileSize(long bytes)
{
    string[] sizes = { "B", "KB", "MB", "GB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len = len / 1024;
    }
    return $"{len:0.##} {sizes[order]}";
}

static async Task DownloadWithProgressAsync(YoutubeClient youtube, IStreamInfo streamInfo, string filePath, string label)
{
    var totalBytes = streamInfo.Size.Bytes;
    var stopwatch = Stopwatch.StartNew();
    var lastBytes = 0L;
    var lastTime = DateTime.Now;
    
    var downloadPanel = new Panel($"[bold yellow]{label}[/]\n\n[cyan]📄 File:[/] {Path.GetFileName(filePath)}\n[magenta]📊 Size:[/] {FormatFileSize(totalBytes)}");
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
            
            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, new Progress<double>(progress =>
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
                    task.Description = $"[cyan]{label}[/] | [yellow]{FormatFileSize((long)speed)}/s[/] | [magenta]ETA: {FormatTime((long)etaSeconds)}[/]";
                    
                    lastBytes = currentBytes;
                    lastTime = DateTime.Now;
                }
            }));
            
            task.Value = 100;
        });
    
    stopwatch.Stop();
    AnsiConsole.Write(new Align(new Markup($"[bold green]✓ Download completed in {FormatTime((long)stopwatch.Elapsed.TotalSeconds)}[/]"), HorizontalAlignment.Center));
    AnsiConsole.WriteLine();
}

static string FormatTime(long seconds)
{
    if (seconds < 60)
        return $"{seconds}s";
    if (seconds < 3600)
        return $"{seconds / 60}m {seconds % 60}s";
    return $"{seconds / 3600}h {(seconds % 3600) / 60}m";
}

static async Task SetupFFmpegAsync()
{
    try
    {
        // Check if FFmpeg is already available
        var existingPath = GlobalFFOptions.GetFFMpegBinaryPath();
        if (!string.IsNullOrEmpty(existingPath) && File.Exists(existingPath))
        {
            AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg ready for video/audio merging.[/]"), HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            return;
        }

        // Try to find FFmpeg in common locations
        var appFolder = AppDomain.CurrentDomain.BaseDirectory;
        var ffmpegFolder = Path.Combine(appFolder, "ffmpeg");
        var ffmpegExe = Path.Combine(ffmpegFolder, "bin", "ffmpeg.exe");
        
        var commonPaths = new[]
        {
            ffmpegExe,
            Path.Combine(appFolder, "ffmpeg.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ffmpeg", "bin", "ffmpeg.exe"),
            "ffmpeg.exe"
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
            {
                var binaryFolder = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(binaryFolder))
                {
                    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = binaryFolder });
                    AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg found. Ready for video/audio merging.[/]"), HorizontalAlignment.Center));
                    AnsiConsole.WriteLine();
                    return;
                }
            }
        }

        // If not found, check PATH
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(2000);
            if (process.ExitCode == 0)
            {
                AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg found in PATH. Ready for video/audio merging.[/]"), HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
                return;
            }
        }
        catch { }

        // FFmpeg not found - download it automatically
        AnsiConsole.Write(new Align(new Markup("[yellow]⚠ FFmpeg not found. Downloading FFmpeg automatically...[/]"), HorizontalAlignment.Center));
        AnsiConsole.Write(new Align(new Markup("[dim]This is a one-time setup. Please wait...[/]"), HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
        
        var downloaded = await DownloadFFmpegAsync(ffmpegFolder);
        if (downloaded && File.Exists(ffmpegExe))
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(ffmpegFolder, "bin") });
            AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg downloaded and ready for video/audio merging![/]"), HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
        else
        {
            var warningPanel = new Panel("[bold yellow]⚠ FFmpeg download failed.[/]\n\n[dim]Higher quality downloads (1080p+) will not be available.\nLower quality muxed streams (360p-720p) will still work.[/]");
            warningPanel.Border = BoxBorder.Rounded;
            warningPanel.BorderColor(Color.Yellow);
            warningPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(warningPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }
    catch (Exception ex)
    {
        var warningPanel = new Panel($"[bold yellow]⚠ FFmpeg setup failed: {ex.Message}[/]\n\n[dim]Lower quality streams will still work.[/]");
        warningPanel.Border = BoxBorder.Rounded;
        warningPanel.BorderColor(Color.Yellow);
        warningPanel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(warningPanel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
}

static async Task<bool> DownloadFFmpegAsync(string targetFolder)
{
    try
    {
        // FFmpeg static build download URL (Windows)
        // Using gyan.dev builds - latest release
        var downloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
        var zipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_{Guid.NewGuid()}.zip");
        
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
                var downloadTask = ctx.AddTask("[cyan]Downloading FFmpeg[/]", maxValue: 100);
                
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(10);
                    
                    var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                    {
                        AnsiConsole.Write(new Align(new Markup($"[red]✗ Download failed: {response.StatusCode}[/]"), HorizontalAlignment.Center));
                        return;
                    }
                    
                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var httpStream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        var totalRead = 0L;
                        var readCount = 0;
                        
                        while ((readCount = await httpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, readCount);
                            totalRead += readCount;
                            
                            if (totalBytes > 0)
                            {
                                downloadTask.Value = (double)totalRead / totalBytes * 100;
                                downloadTask.Description = $"[cyan]Downloading FFmpeg[/] | [yellow]{FormatFileSize(totalRead)}/{FormatFileSize(totalBytes)}[/]";
                            }
                        }
                    }
                }
            });
        
        AnsiConsole.Write(new Align(new Markup("[cyan]Extracting FFmpeg...[/]"), HorizontalAlignment.Center));
        
        // Extract zip file
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        Directory.CreateDirectory(targetFolder);
        
        ZipFile.ExtractToDirectory(zipPath, targetFolder);
        
        // Find the actual ffmpeg folder inside (usually named like "ffmpeg-6.x-essentials_build")
        var extractedFolders = Directory.GetDirectories(targetFolder);
        if (extractedFolders.Length > 0)
        {
            var actualFFmpegFolder = extractedFolders[0];
            var binFolder = Path.Combine(actualFFmpegFolder, "bin");
            
            if (Directory.Exists(binFolder))
            {
                // Move contents to target folder structure
                var targetBinFolder = Path.Combine(targetFolder, "bin");
                if (Directory.Exists(targetBinFolder))
                {
                    Directory.Delete(targetBinFolder, true);
                }
                Directory.Move(binFolder, targetBinFolder);
                
                // Clean up extracted folder
                Directory.Delete(actualFFmpegFolder, true);
            }
        }
        
        // Clean up zip file
        try
        {
            File.Delete(zipPath);
        }
        catch { }
        
        AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg extraction completed![/]"), HorizontalAlignment.Center));
        return true;
    }
    catch (Exception ex)
    {
        AnsiConsole.Write(new Align(new Markup($"[bold red]✗ FFmpeg download failed: {ex.Message}[/]"), HorizontalAlignment.Center));
        return false;
    }
}

static async Task DownloadAndMergeAsync(YoutubeClient youtube, IVideoStreamInfo videoStream, IAudioStreamInfo audioStream, string outputPath)
{
    var tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{videoStream.Container}");
    var tempAudioPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{audioStream.Container}");
    
    try
    {
        // Download video and audio streams
        await DownloadWithProgressAsync(youtube, videoStream, tempVideoPath, "Downloading Video Stream");
        await DownloadWithProgressAsync(youtube, audioStream, tempAudioPath, "Downloading Audio Stream");
        
        // Check FFmpeg availability
        try
        {
            var ffmpegPath = GlobalFFOptions.GetFFMpegBinaryPath();
            if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
            {
                // Try to find in PATH
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                try
                {
                    process.Start();
                    process.WaitForExit(2000);
                    if (process.ExitCode != 0)
                        throw new FileNotFoundException("FFmpeg not found");
                }
                catch
                {
                    throw new FileNotFoundException("FFmpeg not found. Please install FFmpeg and add it to your PATH.");
                }
            }
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch
        {
            throw new FileNotFoundException("FFmpeg not found. Please install FFmpeg and add it to your PATH.");
        }
        
        // Merge video and audio
        AnsiConsole.Write(new Align(new Markup("[cyan]Merging video and audio streams...[/]"), HorizontalAlignment.Center));
        await FFMpegArguments
            .FromFileInput(tempVideoPath)
            .AddFileInput(tempAudioPath)
            .OutputToFile(outputPath, overwrite: true, options => options
                .WithVideoCodec("copy")
                .WithAudioCodec("copy")
                .ForceFormat("mp4"))
            .ProcessAsynchronously();
        
        AnsiConsole.Write(new Align(new Markup("[bold green]✓ Merge completed![/]"), HorizontalAlignment.Center));
    }
    finally
    {
        // Clean up temp files
        try
        {
            if (File.Exists(tempVideoPath)) File.Delete(tempVideoPath);
            if (File.Exists(tempAudioPath)) File.Delete(tempAudioPath);
        }
        catch { }
    }
}

static void PrintVideoInfo(YoutubeExplode.Videos.Video video)
{
    var table = new Table();
    table.Border(TableBorder.Rounded);
    table.BorderColor(Color.Cyan1);
    table.Title = new TableTitle("[bold yellow]📹 Video Information[/]");
    
    // Make Property column narrower and Value column wider
    var propertyColumn = new TableColumn("[bold]Property[/]");
    propertyColumn.Width(12);
    
    var valueColumn = new TableColumn("[bold]Value[/]");
    // Use a reasonable width that fits most console windows and allows wrapping
    valueColumn.Width(60);
    
    table.AddColumn(propertyColumn);
    table.AddColumn(valueColumn);
    
    // Use full text without truncation - table will handle display
    table.AddRow("[cyan]Title[/]", video.Title ?? "");
    table.AddRow("[green]Channel[/]", video.Author.ChannelTitle ?? "");
    table.AddRow("[magenta]Duration[/]", video.Duration.ToString() ?? "");
    
    // Center the table
    AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
    AnsiConsole.WriteLine();
}

static void ShowAboutPage()
{
    AnsiConsole.Clear();
    
    // Header
    AnsiConsole.Write(
        new FigletText("ABOUT")
            .Centered()
            .Color(Color.Cyan1));
    
    AnsiConsole.WriteLine();
    
    // About information panel
    var aboutContent = new Panel(
        "[bold cyan]YouTube Video Downloader with Auto-Merge[/]\n\n" +
        "[bold yellow]Developer Information:[/]\n" +
        "[white]Name:[/] [cyan]Raihan Nishat[/]\n" +
        "[white]GitHub:[/] [link]https://github.com/raihannishat[/]\n" +
        "[white]LinkedIn:[/] [link]https://bd.linkedin.com/in/raihan-nishat-679455163[/]\n\n" +
        "[bold yellow]Application Features:[/]\n" +
        "[green]✓[/] Download YouTube videos in various qualities\n" +
        "[green]✓[/] Automatic video/audio merging with FFmpeg\n" +
        "[green]✓[/] Beautiful console UI with Spectre.Console\n" +
        "[green]✓[/] Real-time download progress display\n" +
        "[green]✓[/] Support for muxed and separate streams\n\n" +
        "[bold yellow]Technologies Used:[/]\n" +
        "[dim]• .NET 10.0[/]\n" +
        "[dim]• YoutubeExplode[/]\n" +
        "[dim]• FFMpegCore[/]\n" +
        "[dim]• Spectre.Console[/]\n\n" +
        "[bold yellow]Version:[/] [cyan]1.0.0[/]\n" +
        "[dim]© 2025 All rights reserved[/]");
    
    aboutContent.Border = BoxBorder.Rounded;
    aboutContent.BorderColor(Color.Cyan1);
    aboutContent.Padding = new Padding(2, 2);
    aboutContent.Width = 80;
    
    AnsiConsole.Write(new Align(aboutContent, HorizontalAlignment.Center));
    AnsiConsole.WriteLine();
    
    // Press any key to continue
    AnsiConsole.Write(new Align(new Markup("[dim]Press any key to continue...[/]"), HorizontalAlignment.Center));
    AnsiConsole.WriteLine();
    Console.ReadKey(true);
    AnsiConsole.Clear();
    
    // Show header again
    AnsiConsole.Write(
        new FigletText("YOUTUBE")
            .Centered()
            .Color(Color.Cyan1));
    
    var panel = new Panel("[bold cyan]Video Downloader with Auto-Merge[/]");
    panel.Border = BoxBorder.Rounded;
    panel.BorderColor(Color.Cyan1);
    panel.Padding = new Padding(1, 1);
    AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center));
    AnsiConsole.WriteLine();
}

