namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IYouTubeService
{
    Task<Video> GetVideoAsync(VideoId videoId);
    Task<StreamManifest> GetStreamManifestAsync(VideoId videoId);
    Task DownloadStreamAsync(IStreamInfo streamInfo, string filePath, IProgress<double>? progress = null);
    Task<YoutubeExplode.Playlists.Playlist> GetPlaylistAsync(YoutubeExplode.Playlists.PlaylistId playlistId);
    IAsyncEnumerable<YoutubeExplode.Playlists.PlaylistVideo> GetPlaylistVideosAsync(YoutubeExplode.Playlists.PlaylistId playlistId);
}

