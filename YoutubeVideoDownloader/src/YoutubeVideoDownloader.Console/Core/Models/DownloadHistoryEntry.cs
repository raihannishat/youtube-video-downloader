namespace YoutubeVideoDownloader.Console.Core.Models;

public class DownloadHistoryEntry
{
    public string VideoId { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime DownloadDate { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPlaylist { get; set; }
    public string? PlaylistTitle { get; set; }
}

