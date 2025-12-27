namespace YoutubeVideoDownloader.Console.Features.BatchDownload;

public static class BatchDownloadHandler
{
    public static List<string> ReadUrlsFromFile(string filePath)
    {
        var urls = new List<string>();
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines and comments (lines starting with #)
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            // Normalize URL
            if (!trimmedLine.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !trimmedLine.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmedLine = "https://" + trimmedLine;
            }

            urls.Add(trimmedLine);
        }

        return urls;
    }

    public static void DisplayBatchInfo(int totalUrls, string filePath)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold yellow]📋 Batch Download Information[/]");
        
        var propertyColumn = new TableColumn("[bold]Property[/]");
        propertyColumn.Width(15);
        
        var valueColumn = new TableColumn("[bold]Value[/]");
        valueColumn.Width(60);
        
        table.AddColumn(propertyColumn);
        table.AddColumn(valueColumn);
        
        table.AddRow("[cyan]File[/]", filePath);
        table.AddRow("[green]Total URLs[/]", totalUrls.ToString());
        
        AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

    public static bool ConfirmBatchDownload(int totalUrls)
    {
        var message = $"[bold yellow]This file contains {totalUrls} URLs.[/]\n[dim]Do you want to download all videos/playlists?[/]";
        return AnsiConsole.Confirm(message, defaultValue: true);
    }

    public static string GetBatchFileOrUrl()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[bold cyan]📄[/] Enter path to file containing URLs (one per line)\n[dim]or enter a YouTube Video/Playlist URL directly:[/]")
                .PromptStyle("cyan")
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        return ValidationResult.Error("[red]Input cannot be empty[/]");
                    }

                    // Check if it's a file path
                    if (File.Exists(input))
                    {
                        return ValidationResult.Success();
                    }

                    // Check if it's a YouTube URL
                    if (input.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || 
                        input.Contains("youtu.be", StringComparison.OrdinalIgnoreCase) ||
                        input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        return ValidationResult.Success();
                    }

                    // Check if it might be a YouTube URL without protocol
                    if (input.Contains("youtube") || input.Contains("youtu.be"))
                    {
                        return ValidationResult.Success();
                    }

                    return ValidationResult.Error("[red]File does not exist or invalid URL[/]");
                }));
    }
}

