namespace YoutubeVideoDownloader.Console.Features.Playlist;

public static class PlaylistHandler
{
    public static void DisplayPlaylistInfo(YoutubeExplode.Playlists.Playlist playlist)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold yellow]📋 Playlist Information[/]");
        
        var propertyColumn = new TableColumn("[bold]Property[/]");
        propertyColumn.Width(12);
        
        var valueColumn = new TableColumn("[bold]Value[/]");
        valueColumn.Width(60);
        
        table.AddColumn(propertyColumn);
        table.AddColumn(valueColumn);
        
        table.AddRow("[cyan]Title[/]", playlist.Title ?? "");
        table.AddRow("[green]Author[/]", playlist.Author?.ChannelTitle ?? "Unknown");
        table.AddRow("[yellow]Description[/]", string.IsNullOrWhiteSpace(playlist.Description) ? "No description" : playlist.Description.Length > 100 ? playlist.Description.Substring(0, 100) + "..." : playlist.Description);
        
        AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

    public static bool ConfirmPlaylistDownload(int videoCount)
    {
        var message = $"[bold yellow]This playlist contains {videoCount} videos.[/]\n[dim]Do you want to download all videos?[/]";
        return AnsiConsole.Confirm(message, defaultValue: true);
    }

    public static string GetQualityChoice()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[bold cyan]🎯[/] Select quality for all videos (number or Enter for highest):")
                .PromptStyle("cyan")
                .AllowEmpty());
    }
}

