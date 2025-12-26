namespace YoutubeVideoDownloader.Console.Features.FFmpegSetup;

public static class FFmpegSetupHandler
{
    public static async Task SetupFFmpegAsync(IFFmpegService ffmpegService)
    {
        await ffmpegService.SetupAsync();
    }
}
