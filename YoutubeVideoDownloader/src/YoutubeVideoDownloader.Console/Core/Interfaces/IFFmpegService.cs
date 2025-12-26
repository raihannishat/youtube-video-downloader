namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IFFmpegService
{
    Task SetupAsync();
    bool IsAvailable();
    Task MergeVideoAndAudioAsync(string videoPath, string audioPath, string outputPath);
}

