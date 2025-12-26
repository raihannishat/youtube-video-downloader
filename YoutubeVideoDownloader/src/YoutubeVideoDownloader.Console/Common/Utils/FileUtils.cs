namespace YoutubeVideoDownloader.Console.Common.Utils;

public static class FileUtils
{
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return string.Empty;
            
        var invalidChars = Path.GetInvalidFileNameChars();
        var charsToReplace = new HashSet<char>(invalidChars) { '<', '>', '.', ' ' };
        
        var result = new System.Text.StringBuilder();
        var lastWasUnderscore = false;
        
        foreach (var c in fileName)
        {
            if (charsToReplace.Contains(c))
            {
                // Replace invalid character with underscore, but avoid consecutive underscores
                if (!lastWasUnderscore)
                {
                    result.Append('_');
                    lastWasUnderscore = true;
                }
            }
            else
            {
                result.Append(c);
                lastWasUnderscore = false;
            }
        }
        
        // Trim trailing dots and underscores
        var sanitized = result.ToString().TrimEnd('.', '_');
        
        return sanitized;
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static string FormatTime(long seconds)
    {
        if (seconds < 60)
            return $"{seconds}s";
        if (seconds < 3600)
        {
            var minutes = seconds / 60;
            var secs = seconds % 60;
            return secs > 0 ? $"{minutes}m {secs}s" : $"{minutes}m 0s";
        }
        var hours = seconds / 3600;
        var remainingSeconds = seconds % 3600;
        var mins = remainingSeconds / 60;
        var secsRemaining = remainingSeconds % 60;
        
        if (secsRemaining > 0)
            return $"{hours}h {mins}m {secsRemaining}s";
        if (mins > 0)
            return $"{hours}h {mins}m";
        return $"{hours}h 0m";
    }
}

