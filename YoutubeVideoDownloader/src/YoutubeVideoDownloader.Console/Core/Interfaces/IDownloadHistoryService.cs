using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IDownloadHistoryService
{
    void AddHistoryEntry(DownloadHistoryEntry entry);
    List<DownloadHistoryEntry> GetHistory(int? limit = null);
    List<DownloadHistoryEntry> GetHistoryByDateRange(DateTime startDate, DateTime endDate);
    DownloadHistoryEntry? GetHistoryByVideoId(string videoId);
    void ClearHistory();
    void RemoveHistoryEntry(string videoId);
    string GetHistoryFilePath();
    int GetHistoryCount();
}

