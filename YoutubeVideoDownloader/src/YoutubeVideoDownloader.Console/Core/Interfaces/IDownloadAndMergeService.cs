namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IDownloadAndMergeService
{
    Task DownloadAndMergeAsync(IVideoStreamInfo videoStream, IAudioStreamInfo audioStream, string outputPath);
}

