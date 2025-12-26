namespace YoutubeVideoDownloader.Console.Features.Download;

public static class DownloadHandler
{
    public static async Task DownloadWithProgressAsync(
        IDownloadService downloadService,
        YoutubeExplode.Videos.Streams.IStreamInfo streamInfo, 
        string filePath, 
        string label)
    {
        await downloadService.DownloadWithProgressAsync(streamInfo, filePath, label);
    }
}
