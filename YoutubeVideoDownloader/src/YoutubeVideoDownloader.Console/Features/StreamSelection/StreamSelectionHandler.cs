namespace YoutubeVideoDownloader.Console.Features.StreamSelection;

public static class StreamSelectionHandler
{
    public static Table DisplayStreams(
        List<MuxedStreamInfo> muxedStreams,
        List<VideoOnlyStreamInfo> videoStreams,
        List<AudioOnlyStreamInfo> audioStreams)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Cyan1);
        table.Title = new TableTitle("[bold cyan]Available Quality Options[/]");
        table.AddColumn(new TableColumn("[bold]Option[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Quality[/]"));
        table.AddColumn(new TableColumn("[bold]Format[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Size[/]").RightAligned());
        
        int optionIndex = 1;
        
        if (muxedStreams.Any())
        {
            table.AddRow("", "[bold yellow]Video + Audio (Ready to Download)[/]", "", "");
            foreach (var stream in muxedStreams)
            {
                table.AddRow(
                    $"[cyan]{optionIndex}[/]",
                    $"[green]✓[/] {stream.VideoQuality.Label}",
                    $"[dim]{stream.Container}[/]",
                    FileUtils.FormatFileSize((long)(stream.Size.MegaBytes * 1024 * 1024)));
                optionIndex++;
            }
        }
        
        if (videoStreams.Any() && audioStreams.Any())
        {
            table.AddRow("", "[bold yellow]Higher Quality (Auto-merged)[/]", "", "");
            foreach (var stream in videoStreams)
            {
                var estimatedSize = stream.Size.MegaBytes + (audioStreams.FirstOrDefault()?.Size.MegaBytes ?? 0);
                table.AddRow(
                    $"[cyan]{optionIndex}[/]",
                    $"[yellow]🔄[/] {stream.VideoQuality.Label}",
                    $"[dim]{stream.Container}[/]",
                    $"~{FileUtils.FormatFileSize((long)(estimatedSize * 1024 * 1024))}");
                optionIndex++;
            }
        }

        if (audioStreams.Any())
        {
            table.AddRow("", "[bold yellow]Audio Only[/]", "", "");
            for (int i = 0; i < audioStreams.Count; i++)
            {
                var stream = audioStreams[i];
                table.AddRow(
                    $"[cyan]A{i + 1}[/]",
                    $"[magenta]🎵[/] {stream.Bitrate} kbps",
                    $"[dim]{stream.Container}[/]",
                    FileUtils.FormatFileSize((long)(stream.Size.MegaBytes * 1024 * 1024)));
            }
        }
        
        return table;
    }
}

