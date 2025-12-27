using System.Text.Json;
using YoutubeVideoDownloader.Console.Core.Interfaces;
using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Core.Services;

public class DownloadHistoryService : IDownloadHistoryService
{
    private readonly List<DownloadHistoryEntry> _history;
    private readonly string _historyFilePath;
    private readonly ILoggerService? _logger;
    private readonly object _lockObject = new object();

    public DownloadHistoryService(ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        
        // Store history in user's AppData folder
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeVideoDownloader");
        
        Directory.CreateDirectory(appDataFolder);
        _historyFilePath = Path.Combine(appDataFolder, "download-history.json");
        
        _history = new List<DownloadHistoryEntry>();
        LoadHistory();
    }

    public void AddHistoryEntry(DownloadHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        
        lock (_lockObject)
        {
            // Remove existing entry with same video ID to avoid duplicates
            _history.RemoveAll(h => h.VideoId == entry.VideoId && !entry.IsPlaylist);
            
            // Add new entry at the beginning (most recent first)
            _history.Insert(0, entry);
            
            // Keep only last 1000 entries to prevent file from growing too large
            if (_history.Count > 1000)
            {
                _history.RemoveRange(1000, _history.Count - 1000);
            }
            
            SaveHistory();
            _logger?.LogInformation($"Added download history entry: {entry.VideoTitle}");
        }
    }

    public List<DownloadHistoryEntry> GetHistory(int? limit = null)
    {
        lock (_lockObject)
        {
            if (limit.HasValue && limit.Value > 0)
            {
                return _history.Take(limit.Value).ToList();
            }
            return _history.ToList();
        }
    }

    public List<DownloadHistoryEntry> GetHistoryByDateRange(DateTime startDate, DateTime endDate)
    {
        lock (_lockObject)
        {
            return _history
                .Where(h => h.DownloadDate >= startDate && h.DownloadDate <= endDate)
                .ToList();
        }
    }

    public DownloadHistoryEntry? GetHistoryByVideoId(string videoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        
        lock (_lockObject)
        {
            return _history.FirstOrDefault(h => h.VideoId == videoId);
        }
    }

    public void ClearHistory()
    {
        lock (_lockObject)
        {
            _history.Clear();
            SaveHistory();
            _logger?.LogInformation("Download history cleared");
        }
    }

    public void RemoveHistoryEntry(string videoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        
        lock (_lockObject)
        {
            var removed = _history.RemoveAll(h => h.VideoId == videoId);
            if (removed > 0)
            {
                SaveHistory();
                _logger?.LogInformation($"Removed history entry for video ID: {videoId}");
            }
        }
    }

    public string GetHistoryFilePath()
    {
        return _historyFilePath;
    }

    public int GetHistoryCount()
    {
        lock (_lockObject)
        {
            return _history.Count;
        }
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                var json = File.ReadAllText(_historyFilePath);
                var entries = JsonSerializer.Deserialize<List<DownloadHistoryEntry>>(json);
                
                if (entries != null)
                {
                    _history.AddRange(entries);
                    _logger?.LogInformation($"Loaded {entries.Count} download history entries");
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to load download history: {ex.Message}", ex);
        }
    }

    private void SaveHistory()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(_history, options);
            File.WriteAllText(_historyFilePath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to save download history: {ex.Message}", ex);
        }
    }
}

