namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IYouTubeService
{
    Task<Video> GetVideoAsync(VideoId videoId);
    Task<StreamManifest> GetStreamManifestAsync(VideoId videoId);
    Task DownloadStreamAsync(IStreamInfo streamInfo, string filePath, IProgress<double>? progress = null);
}

