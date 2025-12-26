namespace YoutubeVideoDownloader.Console.Common.UI;

public static class ConsoleUI
{
    public static void DisplayHeader()
    {
        AnsiConsole.Write(
            new FigletText("YOUTUBE")
                .Centered()
                .Color(Color.Cyan1));

        var panel = new Panel("[bold cyan]Video Downloader with Auto-Merge[/]");
        panel.Border = BoxBorder.Rounded;
        panel.BorderColor(Color.Cyan1);
        panel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

    public static void DisplayCenteredMessage(string message)
    {
        DisplayCenteredMessage(message, Color.Red);
    }

    public static void DisplayCenteredMessage(string message, Color color)
    {
        AnsiConsole.Write(new Align(new Markup($"[{color}]{message}[/]"), HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }

    public static void DisplayCenteredPanel(string content)
    {
        DisplayCenteredPanel(content, Color.Green);
    }

    public static void DisplayCenteredPanel(string content, Color borderColor)
    {
        var panel = new Panel(content);
        panel.Border = BoxBorder.Rounded;
        panel.BorderColor(borderColor);
        panel.Padding = new Padding(1, 1);
        AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center));
        AnsiConsole.WriteLine();
    }
}

