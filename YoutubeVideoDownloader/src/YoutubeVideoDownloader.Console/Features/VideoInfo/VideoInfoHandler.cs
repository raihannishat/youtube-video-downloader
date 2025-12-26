namespace YoutubeVideoDownloader.Console.Features.VideoInfo;

public static class VideoInfoHandler
{
    public static void DisplayVideoInfo(Video video)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold yellow]📹 Video Information[/]");
        
        var propertyColumn = new TableColumn("[bold]Property[/]");
        propertyColumn.Width(12);
        
        var valueColumn = new TableColumn("[bold]Value[/]");
        valueColumn.Width(60);
        
        table.AddColumn(propertyColumn);
        table.AddColumn(valueColumn);
        
        table.AddRow("[cyan]Title[/]", video.Title ?? "");
        table.AddRow("[green]Channel[/]", video.Author.ChannelTitle ?? "");
        table.AddRow("[magenta]Duration[/]", video.Duration.ToString() ?? "");
        
        AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
}

