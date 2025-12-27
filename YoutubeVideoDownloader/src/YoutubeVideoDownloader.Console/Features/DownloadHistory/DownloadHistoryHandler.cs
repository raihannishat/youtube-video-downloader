using YoutubeVideoDownloader.Console.Core.Interfaces;
using YoutubeVideoDownloader.Console.Core.Models;

namespace YoutubeVideoDownloader.Console.Features.DownloadHistory;

public static class DownloadHistoryHandler
{
    private const int PageSize = 25;

    public static void ShowHistory(IDownloadHistoryService historyService)
    {
        var allHistory = historyService.GetHistory();
        var totalCount = allHistory.Count;
        var historyPath = historyService.GetHistoryFilePath();

        if (totalCount == 0)
        {
            var emptyPanel = new Panel("[bold yellow]No download history found.[/]\n\n[dim]Start downloading videos to build your history![/]");
            emptyPanel.Border = BoxBorder.Rounded;
            emptyPanel.BorderColor(Color.Yellow);
            emptyPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(emptyPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            return;
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        var currentPage = 1;

        while (true)
        {
            AnsiConsole.Clear();
            
            var startIndex = (currentPage - 1) * PageSize;
            var pageHistory = allHistory.Skip(startIndex).Take(PageSize).ToList();

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Cyan1);
            table.Title = new TableTitle($"[bold yellow]📜 Download History[/] (Page {currentPage} of {totalPages} - Showing {pageHistory.Count} of {totalCount})");

            table.AddColumn(new TableColumn("[bold]#[/]").Width(3).RightAligned());
            table.AddColumn(new TableColumn("[bold]Title[/]").Width(40));
            table.AddColumn(new TableColumn("[bold]Quality[/]").Width(10));
            table.AddColumn(new TableColumn("[bold]Size[/]").Width(10).RightAligned());
            table.AddColumn(new TableColumn("[bold]Date[/]").Width(18));

            for (int i = 0; i < pageHistory.Count; i++)
            {
                var entry = pageHistory[i];
                var index = (startIndex + i + 1).ToString();
                var title = entry.VideoTitle.Length > 40 ? entry.VideoTitle.Substring(0, 37) + "..." : entry.VideoTitle;
                var quality = entry.Quality;
                var fileSize = FileUtils.FormatFileSize(entry.FileSizeBytes);
                var date = entry.DownloadDate.ToString("yyyy-MM-dd HH:mm");

                var playlistIndicator = entry.IsPlaylist ? "[dim]📋[/] " : "";
                var titleMarkup = $"{playlistIndicator}[cyan]{title}[/]";

                table.AddRow(
                    $"[dim]{index}[/]",
                    titleMarkup,
                    $"[green]{quality}[/]",
                    $"[yellow]{fileSize}[/]",
                    $"[dim]{date}[/]"
                );
            }

            AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]History file: {historyPath}[/]");
            AnsiConsole.WriteLine();

            // Navigation options
            var navigationOptions = new List<string>();
            if (currentPage > 1)
                navigationOptions.Add("Previous Page");
            if (currentPage < totalPages)
                navigationOptions.Add("Next Page");
            navigationOptions.Add("Back");

            if (navigationOptions.Count == 1)
            {
                // Only "Back" option available
                AnsiConsole.MarkupLine("[dim]Press any key to go back...[/]");
                System.Console.ReadKey(true);
                break;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[dim]Navigation:[/]")
                    .AddChoices(navigationOptions));

            if (choice == "Back")
                break;
            else if (choice == "Previous Page")
                currentPage--;
            else if (choice == "Next Page")
                currentPage++;
        }
    }

    public static void ShowDetailedHistory(IDownloadHistoryService historyService)
    {
        var allHistory = historyService.GetHistory();
        var totalCount = allHistory.Count;

        if (totalCount == 0)
        {
            var emptyPanel = new Panel("[bold yellow]No download history found.[/]");
            emptyPanel.Border = BoxBorder.Rounded;
            emptyPanel.BorderColor(Color.Yellow);
            emptyPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(emptyPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
            return;
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        var currentPage = 1;

        while (true)
        {
            AnsiConsole.Clear();
            
            var startIndex = (currentPage - 1) * PageSize;
            var pageHistory = allHistory.Skip(startIndex).Take(PageSize).ToList();

            AnsiConsole.MarkupLine($"[bold yellow]📜 Download History (Detailed)[/] - [dim]Page {currentPage} of {totalPages} - Showing {pageHistory.Count} of {totalCount}[/]");
            AnsiConsole.WriteLine();

            foreach (var entry in pageHistory)
            {
                var panel = new Panel(
                    $"[bold cyan]📹 {entry.VideoTitle}[/]\n\n" +
                    $"[cyan]Channel:[/] {entry.ChannelName}\n" +
                    $"[green]Quality:[/] {entry.Quality}\n" +
                    $"[yellow]Size:[/] {FileUtils.FormatFileSize(entry.FileSizeBytes)}\n" +
                    $"[magenta]Duration:[/] {FileUtils.FormatTime((long)entry.Duration.TotalSeconds)}\n" +
                    $"[blue]Date:[/] {entry.DownloadDate:yyyy-MM-dd HH:mm:ss}\n" +
                    $"[cyan]File:[/] {entry.FilePath}\n" +
                    $"[dim]URL:[/] {entry.VideoUrl}" +
                    (entry.IsPlaylist ? $"\n[green]Playlist:[/] {entry.PlaylistTitle}" : "")
                );
                panel.Border = BoxBorder.Rounded;
                panel.BorderColor(Color.Cyan);
                panel.Padding = new Padding(1, 1);
                AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center));
                AnsiConsole.WriteLine();
            }

            // Navigation options
            var navigationOptions = new List<string>();
            if (currentPage > 1)
                navigationOptions.Add("Previous Page");
            if (currentPage < totalPages)
                navigationOptions.Add("Next Page");
            navigationOptions.Add("Back");

            if (navigationOptions.Count == 1)
            {
                // Only "Back" option available
                AnsiConsole.MarkupLine("[dim]Press any key to go back...[/]");
                System.Console.ReadKey(true);
                break;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[dim]Navigation:[/]")
                    .AddChoices(navigationOptions));

            if (choice == "Back")
                break;
            else if (choice == "Previous Page")
                currentPage--;
            else if (choice == "Next Page")
                currentPage++;
        }
    }

    public static void ClearHistory(IDownloadHistoryService historyService)
    {
        var confirm = AnsiConsole.Confirm(
            "[bold yellow]⚠ Are you sure you want to clear all download history?[/]",
            defaultValue: false);

        if (confirm)
        {
            historyService.ClearHistory();
            AnsiConsole.WriteLine();
            var successPanel = new Panel("[bold green]✓ Download history cleared![/]");
            successPanel.Border = BoxBorder.Rounded;
            successPanel.BorderColor(Color.Green);
            successPanel.Padding = new Padding(1, 1);
            AnsiConsole.Write(new Align(successPanel, HorizontalAlignment.Center));
            AnsiConsole.WriteLine();
        }
    }

    public static void ShowHistoryStats(IDownloadHistoryService historyService)
    {
        var history = historyService.GetHistory();
        var totalCount = history.Count;
        
        if (totalCount == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No download history available.[/]");
            return;
        }

        var totalSize = history.Sum(h => h.FileSizeBytes);
        var videoCount = history.Count(h => !h.IsPlaylist);
        var playlistCount = history.Count(h => h.IsPlaylist);
        var totalDuration = TimeSpan.FromSeconds(history.Sum(h => h.Duration.TotalSeconds));

        var statsTable = new Table();
        statsTable.Border(TableBorder.Rounded);
        statsTable.BorderColor(Color.Cyan1);
        statsTable.Title = new TableTitle("[bold yellow]📊 Download Statistics[/]");

        var propertyColumn = new TableColumn("[bold]Property[/]");
        propertyColumn.Width(25);

        var valueColumn = new TableColumn("[bold]Value[/]");
        valueColumn.Width(30);

        statsTable.AddColumn(propertyColumn);
        statsTable.AddColumn(valueColumn);

        statsTable.AddRow("[cyan]Total Downloads[/]", totalCount.ToString());
        statsTable.AddRow("[green]Videos[/]", videoCount.ToString());
        statsTable.AddRow("[magenta]Playlists[/]", playlistCount.ToString());
        statsTable.AddRow("[yellow]Total Size[/]", FileUtils.FormatFileSize(totalSize));
        statsTable.AddRow("[blue]Total Duration[/]", FileUtils.FormatTime((long)totalDuration.TotalSeconds));
        statsTable.AddRow("[cyan]History File[/]", historyService.GetHistoryFilePath());

        AnsiConsole.Write(new Align(statsTable, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
}

