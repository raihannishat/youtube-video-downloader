namespace YoutubeVideoDownloader.Console.Core.Models;

public class AppConfiguration
{
    public string DefaultDownloadDirectory { get; set; } = string.Empty;
    public string DefaultQuality { get; set; } = "highest"; // "highest", "720p", "1080p", "audio", or empty for prompt
    public string? CustomFFmpegPath { get; set; }
    public string LogLevel { get; set; } = "Information"; // "Debug", "Information", "Warning", "Error"
    public bool AutoCreatePlaylistFolder { get; set; } = true;
    public bool ShowVideoInfoBeforeDownload { get; set; } = true;
    public int MaxConcurrentDownloads { get; set; } = 1; // For future batch download optimization
}

