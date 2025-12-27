using YoutubeVideoDownloader.Console.Core.Interfaces;
using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Features.Configuration;

public static class ConfigurationHandler
{
    public static void ShowConfiguration(IConfigurationService configService)
    {
        var config = configService.GetConfiguration();
        var configPath = configService.GetConfigFilePath();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold yellow]⚙️ Application Configuration[/]");

        var propertyColumn = new TableColumn("[bold]Property[/]");
        propertyColumn.Width(25);

        var valueColumn = new TableColumn("[bold]Value[/]");
        valueColumn.Width(50);

        table.AddColumn(propertyColumn);
        table.AddColumn(valueColumn);

        table.AddRow("[cyan]Default Download Directory[/]", config.DefaultDownloadDirectory);
        table.AddRow("[green]Default Quality[/]", config.DefaultQuality);
        table.AddRow("[magenta]Custom FFmpeg Path[/]", config.CustomFFmpegPath ?? "Auto-detect");
        table.AddRow("[yellow]Log Level[/]", config.LogLevel);
        table.AddRow("[blue]Auto Create Playlist Folder[/]", config.AutoCreatePlaylistFolder ? "Yes" : "No");
        table.AddRow("[cyan]Show Video Info[/]", config.ShowVideoInfoBeforeDownload ? "Yes" : "No");
        table.AddRow("[green]Config File Path[/]", configPath);

        AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

    public static void EditConfiguration(IConfigurationService configService)
    {
        var config = configService.GetConfiguration();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]⚙️ Edit Configuration[/]");
        AnsiConsole.WriteLine();

        // Default Download Directory
        var defaultDir = AnsiConsole.Prompt(
            new TextPrompt<string>($"[cyan]Default Download Directory[/] (current: [dim]{config.DefaultDownloadDirectory}[/]):")
                .PromptStyle("cyan")
                .AllowEmpty()
                .Validate(path =>
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return ValidationResult.Success();
                    }

                    try
                    {
                        var fullPath = Path.GetFullPath(path);
                        if (Path.GetInvalidPathChars().Any(c => path.Contains(c)))
                        {
                            return ValidationResult.Error("[red]Invalid characters in path[/]");
                        }
                        return ValidationResult.Success();
                    }
                    catch
                    {
                        return ValidationResult.Error("[red]Invalid path[/]");
                    }
                }));

        if (!string.IsNullOrWhiteSpace(defaultDir))
        {
            config.DefaultDownloadDirectory = Path.GetFullPath(defaultDir);
        }

        // Default Quality
        var qualityOptions = new[] { "highest", "720p", "1080p", "audio", "prompt" };
        var qualityChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[cyan]Default Quality[/] (current: [dim]{config.DefaultQuality}[/]):")
                .AddChoices(qualityOptions));

        config.DefaultQuality = qualityChoice == "prompt" ? string.Empty : qualityChoice;

        // Custom FFmpeg Path
        var ffmpegPath = AnsiConsole.Prompt(
            new TextPrompt<string>($"[cyan]Custom FFmpeg Path[/] (current: [dim]{config.CustomFFmpegPath ?? "Auto-detect"}[/], Enter to skip):")
                .PromptStyle("cyan")
                .AllowEmpty());

        if (!string.IsNullOrWhiteSpace(ffmpegPath))
        {
            config.CustomFFmpegPath = Path.GetFullPath(ffmpegPath);
        }
        else if (string.IsNullOrWhiteSpace(ffmpegPath) && config.CustomFFmpegPath != null)
        {
            // User wants to clear custom path
            config.CustomFFmpegPath = null;
        }

        // Log Level
        var logLevels = new[] { "Debug", "Information", "Warning", "Error" };
        var logLevel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[cyan]Log Level[/] (current: [dim]{config.LogLevel}[/]):")
                .AddChoices(logLevels));

        config.LogLevel = logLevel;

        // Auto Create Playlist Folder
        config.AutoCreatePlaylistFolder = AnsiConsole.Confirm(
            $"[cyan]Auto Create Playlist Folder[/] (current: [dim]{(config.AutoCreatePlaylistFolder ? "Yes" : "No")}[/]):",
            config.AutoCreatePlaylistFolder);

        // Show Video Info
        config.ShowVideoInfoBeforeDownload = AnsiConsole.Confirm(
            $"[cyan]Show Video Info Before Download[/] (current: [dim]{(config.ShowVideoInfoBeforeDownload ? "Yes" : "No")}[/]):",
            config.ShowVideoInfoBeforeDownload);

        // Save configuration
        try
        {
            configService.SaveConfiguration(config);
            AnsiConsole.WriteLine();
            var successPanel = new Panel("[bold green]✓ Configuration saved successfully![/]");
            successPanel.Border = BoxBorder.Rounded;
            successPanel.BorderColor(Color.Green);
            successPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            var errorPanel = new Panel($"[bold red]✗ Failed to save configuration:[/]\n[dim]{ex.Message}[/]");
            errorPanel.Border = BoxBorder.Rounded;
            errorPanel.BorderColor(Color.Red);
            errorPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(errorPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }

    public static void ResetConfiguration(IConfigurationService configService)
    {
        var confirm = AnsiConsole.Confirm(
            "[bold yellow]⚠ Are you sure you want to reset all configuration to defaults?[/]",
            defaultValue: false);

        if (confirm)
        {
            configService.ResetToDefaults();
            AnsiConsole.WriteLine();
            var successPanel = new Panel("[bold green]✓ Configuration reset to defaults![/]");
            successPanel.Border = BoxBorder.Rounded;
            successPanel.BorderColor(Color.Green);
            successPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }
}

