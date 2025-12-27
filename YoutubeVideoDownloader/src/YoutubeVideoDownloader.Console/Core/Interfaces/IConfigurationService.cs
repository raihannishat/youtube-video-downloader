using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Core.Interfaces;

public interface IConfigurationService
{
    AppConfiguration GetConfiguration();
    void SaveConfiguration(AppConfiguration configuration);
    void LoadConfiguration();
    string GetConfigFilePath();
    void ResetToDefaults();
}

