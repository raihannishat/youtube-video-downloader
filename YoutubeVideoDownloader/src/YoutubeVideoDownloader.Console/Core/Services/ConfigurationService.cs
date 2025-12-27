using System.Text.Json;
using YoutubeVideoDownloader.Console.Core.Interfaces;
using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private AppConfiguration _configuration;
    private readonly string _configFilePath;
    private readonly ILoggerService? _logger;

    public ConfigurationService(ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        
        // Store config in user's AppData folder
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeVideoDownloader");
        
        Directory.CreateDirectory(appDataFolder);
        _configFilePath = Path.Combine(appDataFolder, "config.json");
        
        _configuration = new AppConfiguration();
        LoadConfiguration();
    }

    public AppConfiguration GetConfiguration()
    {
        return _configuration;
    }

    public void SaveConfiguration(AppConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(_configFilePath, json);
            
            _configuration = configuration;
            _logger?.LogInformation($"Configuration saved to: {_configFilePath}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to save configuration: {ex.Message}", ex);
            throw;
        }
    }

    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                
                if (config != null)
                {
                    _configuration = config;
                    _logger?.LogInformation($"Configuration loaded from: {_configFilePath}");
                }
                else
                {
                    _logger?.LogWarning("Configuration file is empty or invalid, using defaults");
                    InitializeDefaultConfiguration();
                }
            }
            else
            {
                _logger?.LogInformation("Configuration file not found, using defaults");
                InitializeDefaultConfiguration();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to load configuration: {ex.Message}", ex);
            InitializeDefaultConfiguration();
        }
    }

    public string GetConfigFilePath()
    {
        return _configFilePath;
    }

    public void ResetToDefaults()
    {
        _configuration = new AppConfiguration();
        SaveConfiguration(_configuration);
        _logger?.LogInformation("Configuration reset to defaults");
    }

    private void InitializeDefaultConfiguration()
    {
        // Set default download directory
        var defaultDownloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Downloads");
        
        if (!Directory.Exists(defaultDownloadsFolder))
        {
            defaultDownloadsFolder = Directory.GetCurrentDirectory();
        }
        
        _configuration.DefaultDownloadDirectory = defaultDownloadsFolder;
        
        // Save default configuration
        try
        {
            SaveConfiguration(_configuration);
        }
        catch
        {
            // Ignore if we can't save defaults
        }
    }
}

