namespace YoutubeVideoDownloader.Console.Core.Services;

public class FFmpegService : IFFmpegService
{
    private readonly ILoggerService _logger;

    public FFmpegService(ILoggerService logger)
    {
        _logger = logger;
    }

    public bool IsAvailable()
    {
        try
        {
            var existingPath = GlobalFFOptions.GetFFMpegBinaryPath();
            if (!string.IsNullOrEmpty(existingPath) && File.Exists(existingPath))
            {
                return true;
            }

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
            process.Start();
            process.WaitForExit(2000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task SetupAsync()
    {
        try
        {
            _logger.LogInformation("Setting up FFmpeg...");
            
            if (IsAvailable())
            {
                _logger.LogInformation("FFmpeg is already available");
                AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg ready for video/audio merging.[/]"), HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
                return;
            }

            // Try to find FFmpeg in common locations
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YoutubeVideoDownloader");
            
            var ffmpegFolderApp = Path.Combine(appFolder, "ffmpeg");
            var ffmpegFolderUser = Path.Combine(userDataFolder, "ffmpeg");
            
            var ffmpegExeApp = Path.Combine(ffmpegFolderApp, "bin", "ffmpeg.exe");
            var ffmpegExeUser = Path.Combine(ffmpegFolderUser, "bin", "ffmpeg.exe");
            
            var commonPaths = new[]
            {
                ffmpegExeApp,
                ffmpegExeUser,
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
                        _logger.LogInformation($"FFmpeg found at: {binaryFolder}");
                        AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg found. Ready for video/audio merging.[/]"), HorizontalAlignment.Center));
                        AnsiConsole.WriteLine();
                        return;
                    }
                }
            }

            // FFmpeg not found - download it automatically
            _logger.LogWarning("FFmpeg not found, attempting to download...");
            AnsiConsole.Write(new Align(new Markup("[yellow]⚠ FFmpeg not found. Downloading FFmpeg automatically...[/]"), HorizontalAlignment.Center));
            AnsiConsole.Write(new Align(new Markup("[dim]This is a one-time setup. Please wait...[/]"), HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            
            // Always use user data folder for downloads (always writable, no admin needed)
            // Ensure the directory exists
            Directory.CreateDirectory(userDataFolder);
            var ffmpegFolder = ffmpegFolderUser;
            var ffmpegExe = Path.Combine(ffmpegFolder, "bin", "ffmpeg.exe");
            
            _logger.LogInformation($"Downloading FFmpeg to user data folder: {ffmpegFolder}");
            _logger.LogInformation($"App folder (NOT used for download): {appFolder}");
            AnsiConsole.WriteLine();
            var downloaded = await DownloadFFmpegAsync(ffmpegFolder);
            if (downloaded && File.Exists(ffmpegExe))
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(ffmpegFolder, "bin") });
                _logger.LogInformation("FFmpeg downloaded and configured successfully");
                AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg downloaded and ready for video/audio merging![/]"), HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }
            else
            {
                _logger.LogWarning("FFmpeg download failed");
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
            _logger.LogError("FFmpeg setup failed", ex);
            var warningPanel = new Panel($"[bold yellow]⚠ FFmpeg setup failed: {ex.Message}[/]\n\n[dim]Lower quality streams will still work.[/]");
            warningPanel.Border = BoxBorder.Rounded;
            warningPanel.BorderColor(Color.Yellow);
            warningPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(warningPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }


    private async Task<bool> DownloadFFmpegAsync(string targetFolder)
    {
        try
        {
            // Verify we're using the correct (writable) folder
            _logger.LogInformation($"DownloadFFmpegAsync called with targetFolder: {targetFolder}");
            
            // Double-check: if targetFolder is in Program Files, force user data folder
            if (targetFolder.Contains("Program Files", StringComparison.OrdinalIgnoreCase))
            {
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "YoutubeVideoDownloader", "ffmpeg");
                _logger.LogWarning($"Detected Program Files path, switching to user data folder: {userDataFolder}");
                targetFolder = userDataFolder;
            }
            
            var downloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_{Guid.NewGuid()}.zip");
            
            _logger.LogInformation($"Starting FFmpeg download to: {targetFolder}");
            
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
                            _logger.LogError($"FFmpeg download failed: {response.StatusCode}");
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
                                    downloadTask.Description = $"[cyan]Downloading FFmpeg[/] | [yellow]{FileUtils.FormatFileSize(totalRead)}/{FileUtils.FormatFileSize(totalBytes)}[/]";
                                }
                            }
                        }
                    }
                });
            
            _logger.LogInformation("Extracting FFmpeg...");
            AnsiConsole.Write(new Align(new Markup("[cyan]Extracting FFmpeg...[/]"), HorizontalAlignment.Center));
            
            // Ensure target folder exists and is writable
            try
            {
                _logger.LogInformation($"Preparing extraction directory: {targetFolder}");
                
                // Final safety check - if still in Program Files, use user data
                if (targetFolder.Contains("Program Files", StringComparison.OrdinalIgnoreCase))
                {
                    var userDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "YoutubeVideoDownloader", "ffmpeg");
                    _logger.LogWarning($"Program Files detected during extraction, switching to: {userDataFolder}");
                    targetFolder = userDataFolder;
                }
                
                if (Directory.Exists(targetFolder))
                {
                    _logger.LogInformation($"Deleting existing directory: {targetFolder}");
                    Directory.Delete(targetFolder, true);
                }
                
                _logger.LogInformation($"Creating directory: {targetFolder}");
                Directory.CreateDirectory(targetFolder);
                
                // Verify we can write
                var testFile = Path.Combine(targetFolder, "test_write.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                _logger.LogInformation($"Directory is writable: {targetFolder}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create FFmpeg directory: {targetFolder}", ex);
                AnsiConsole.Write(new Align(new Markup($"[bold red]✗ Cannot create directory: {targetFolder}[/]"), HorizontalAlignment.Center));
                AnsiConsole.Write(new Align(new Markup($"[red]Error: {ex.Message}[/]"), HorizontalAlignment.Center));
                throw new IOException($"Cannot create FFmpeg directory at {targetFolder}. Access denied or insufficient permissions.", ex);
            }
            
            _logger.LogInformation($"Extracting ZIP to: {targetFolder}");
            ZipFile.ExtractToDirectory(zipPath, targetFolder);
            
            var extractedFolders = Directory.GetDirectories(targetFolder);
            if (extractedFolders.Length > 0)
            {
                var actualFFmpegFolder = extractedFolders[0];
                var binFolder = Path.Combine(actualFFmpegFolder, "bin");
                
                if (Directory.Exists(binFolder))
                {
                    var targetBinFolder = Path.Combine(targetFolder, "bin");
                    if (Directory.Exists(targetBinFolder))
                    {
                        Directory.Delete(targetBinFolder, true);
                    }
                    Directory.Move(binFolder, targetBinFolder);
                    Directory.Delete(actualFFmpegFolder, true);
                }
            }
            
            try
            {
                File.Delete(zipPath);
            }
            catch { }
            
            _logger.LogInformation("FFmpeg extraction completed");
            AnsiConsole.Write(new Align(new Markup("[bold green]✓ FFmpeg extraction completed![/]"), HorizontalAlignment.Center));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("FFmpeg download failed", ex);
            AnsiConsole.Write(new Align(new Markup($"[bold red]✗ FFmpeg download failed: {ex.Message}[/]"), HorizontalAlignment.Center));
            return false;
        }
    }

    public async Task MergeVideoAndAudioAsync(string videoPath, string audioPath, string outputPath)
    {
        _logger.LogInformation($"Merging video and audio: {outputPath}");
        
        if (!IsAvailable())
        {
            throw new FileNotFoundException("FFmpeg not found. Please install FFmpeg and add it to your PATH.");
        }
        
        await FFMpegArguments
            .FromFileInput(videoPath)
            .AddFileInput(audioPath)
            .OutputToFile(outputPath, overwrite: true, options => options
                .WithVideoCodec("copy")
                .WithAudioCodec("copy")
                .ForceFormat("mp4"))
            .ProcessAsynchronously();
        
        _logger.LogInformation($"Merge completed: {outputPath}");
    }
}

