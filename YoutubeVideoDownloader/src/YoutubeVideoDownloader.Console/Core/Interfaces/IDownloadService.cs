namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IDownloadService
{
    Task DownloadWithProgressAsync(IStreamInfo streamInfo, string filePath, string label);
}

