namespace YoutubeVideoDownloader.Console.Core.Services;

public class YouTubeService : IYouTubeService
{
    private readonly YoutubeClient _client;
    private readonly ILoggerService _logger;

    public YouTubeService(ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _client = new YoutubeClient();
        _logger = logger;
    }

    public async Task<Video> GetVideoAsync(VideoId videoId)
    {
        _logger.LogInformation($"Fetching video information for: {videoId}");
        var video = await _client.Videos.GetAsync(videoId);
        _logger.LogInformation($"Video fetched: {video.Title}");
        return video;
    }

    public async Task<StreamManifest> GetStreamManifestAsync(VideoId videoId)
    {
        _logger.LogInformation($"Fetching stream manifest for: {videoId}");
        var manifest = await _client.Videos.Streams.GetManifestAsync(videoId);
        _logger.LogInformation($"Stream manifest fetched. Muxed: {manifest.GetMuxedStreams().Count()}, Video: {manifest.GetVideoOnlyStreams().Count()}, Audio: {manifest.GetAudioOnlyStreams().Count()}");
        return manifest;
    }

    public async Task DownloadStreamAsync(IStreamInfo streamInfo, string filePath, IProgress<double>? progress = null)
    {
        _logger.LogInformation($"Starting download: {filePath}");
        await _client.Videos.Streams.DownloadAsync(streamInfo, filePath, progress);
        _logger.LogInformation($"Download completed: {filePath}");
    }

    public async Task<YoutubeExplode.Playlists.Playlist> GetPlaylistAsync(YoutubeExplode.Playlists.PlaylistId playlistId)
    {
        _logger.LogInformation($"Fetching playlist information for: {playlistId}");
        var playlist = await _client.Playlists.GetAsync(playlistId);
        _logger.LogInformation($"Playlist fetched: {playlist.Title}");
        return playlist;
    }

    public async IAsyncEnumerable<YoutubeExplode.Playlists.PlaylistVideo> GetPlaylistVideosAsync(YoutubeExplode.Playlists.PlaylistId playlistId)
    {
        _logger.LogInformation($"Fetching playlist videos for: {playlistId}");
        await foreach (var video in _client.Playlists.GetVideosAsync(playlistId))
        {
            _logger.LogInformation($"Playlist video fetched: {video.Title}");
            yield return video;
        }
    }
}

