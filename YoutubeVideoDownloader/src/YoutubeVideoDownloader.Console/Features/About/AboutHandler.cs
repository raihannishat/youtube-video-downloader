namespace YoutubeVideoDownloader.Console.Features.About;

public static class AboutHandler
{
    public static void ShowAboutPage()
    {
        AnsiConsole.Clear();
        
        // Header
        AnsiConsole.Write(
            new FigletText("ABOUT")
                .Centered()
                .Color(Color.Cyan1));
        
        AnsiConsole.WriteLine();
        
        // About information panel
        var aboutContent = new Panel(
            "[bold cyan]YouTube Video Downloader with Auto-Merge[/]\n\n" +
            "[bold yellow]Developer Information:[/]\n" +
            "[white]Name:[/] [cyan]Raihan Nishat[/]\n" +
            "[white]GitHub:[/] [link]https://github.com/raihannishat[/]\n" +
            "[white]LinkedIn:[/] [link]https://bd.linkedin.com/in/raihan-nishat-679455163[/]\n\n" +
            "[bold yellow]Application Features:[/]\n" +
            "[green]✓[/] Download YouTube videos in various qualities\n" +
            "[green]✓[/] Automatic video/audio merging with FFmpeg\n" +
            "[green]✓[/] Beautiful console UI with Spectre.Console\n" +
            "[green]✓[/] Real-time download progress display\n" +
            "[green]✓[/] Support for muxed and separate streams\n\n" +
            "[bold yellow]Technologies Used:[/]\n" +
            "[dim]• .NET 10.0[/]\n" +
            "[dim]• YoutubeExplode[/]\n" +
            "[dim]• FFMpegCore[/]\n" +
            "[dim]• Spectre.Console[/]\n\n" +
            "[bold yellow]Version:[/] [cyan]1.2.1[/]\n" +
            "[dim]© 2025 All rights reserved[/]");
        
        aboutContent.Border = BoxBorder.Rounded;
        aboutContent.BorderColor(Color.Cyan1);
        aboutContent.Padding = new Padding(2, 2);
        aboutContent.Width = 80;
        
        AnsiConsole.Write(new Align(aboutContent, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
        
        // Press any key to continue
        AnsiConsole.Write(new Align(new Markup("[dim]Press any key to continue...[/]"), HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
        System.Console.ReadKey(true);
        AnsiConsole.Clear();
        
        // Show header again
        ConsoleUI.DisplayHeader();
    }
}

