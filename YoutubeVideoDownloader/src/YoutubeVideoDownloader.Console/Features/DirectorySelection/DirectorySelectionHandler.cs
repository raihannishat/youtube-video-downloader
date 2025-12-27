namespace YoutubeVideoDownloader.Console.Features.DirectorySelection;

public static class DirectorySelectionHandler
{
    public static string GetOutputDirectory(string defaultDirectory, ILoggerService? logger = null)
    {
        AnsiConsole.WriteLine();
        var defaultPath = defaultDirectory;
        var message = $"[bold cyan]📂[/] Enter output directory (or Enter for default):\n[dim]Default: {defaultPath}[/]";
        
        var userInput = AnsiConsole.Prompt(
            new TextPrompt<string>(message)
                .PromptStyle("cyan")
                .AllowEmpty()
                .Validate(path =>
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return ValidationResult.Success();
                    }

                    // Validate path
                    try
                    {
                        var fullPath = Path.GetFullPath(path);
                        
                        // Check if path is valid
                        if (Path.GetInvalidPathChars().Any(c => path.Contains(c)))
                        {
                            return ValidationResult.Error("[red]Invalid characters in path[/]");
                        }

                        // Check if parent directory exists
                        var parentDir = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                        {
                            return ValidationResult.Error("[red]Parent directory does not exist[/]");
                        }

                        return ValidationResult.Success();
                    }
                    catch (Exception ex)
                    {
                        return ValidationResult.Error($"[red]Invalid path: {ex.Message}[/]");
                    }
                }));

        if (string.IsNullOrWhiteSpace(userInput))
        {
            return defaultPath;
        }

        var selectedPath = Path.GetFullPath(userInput);
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(selectedPath))
        {
            try
            {
                Directory.CreateDirectory(selectedPath);
                logger?.LogInformation($"Created output directory: {selectedPath}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Failed to create directory: {ex.Message}[/]");
                AnsiConsole.MarkupLine($"[yellow]Using default directory instead: {defaultPath}[/]");
                return defaultPath;
            }
        }

        return selectedPath;
    }
}

