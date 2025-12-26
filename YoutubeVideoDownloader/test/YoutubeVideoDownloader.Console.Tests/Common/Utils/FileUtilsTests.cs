namespace YoutubeVideoDownloader.Console.Tests.Common.Utils;

public class FileUtilsTests
{
    [Theory]
    [InlineData("test file", "test_file")]
    [InlineData("test/file", "test_file")]
    [InlineData("test<file>", "test_file")]
    [InlineData("test|file", "test_file")]
    [InlineData("test:file", "test_file")]
    [InlineData("test\"file", "test_file")]
    [InlineData("test*file", "test_file")]
    [InlineData("test?file", "test_file")]
    [InlineData("test.file.", "test_file")]
    public void SanitizeFileName_ShouldRemoveInvalidCharacters(string input, string expected)
    {
        // Act
        var result = FileUtils.SanitizeFileName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1572864, "1.5 MB")]
    public void FormatFileSize_ShouldFormatCorrectly(long bytes, string expected)
    {
        // Act
        var result = FileUtils.FormatFileSize(bytes);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(30, "30s")]
    [InlineData(60, "1m 0s")]
    [InlineData(90, "1m 30s")]
    [InlineData(3600, "1h 0m")]
    [InlineData(3661, "1h 1m 1s")]
    public void FormatTime_ShouldFormatCorrectly(long seconds, string expected)
    {
        // Act
        var result = FileUtils.FormatTime(seconds);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeFileName_ShouldHandleEmptyString()
    {
        // Act
        var result = FileUtils.SanitizeFileName("");

        // Assert
        result.Should()
            .NotBeNull()
            .And.BeEmpty()
            .And.Be("");
    }

    [Fact]
    public void SanitizeFileName_ShouldHandleNull()
    {
        // Act
        var result = FileUtils.SanitizeFileName(null!);

        // Assert
        result.Should()
            .NotBeNull()
            .And.BeOfType<string>();
    }

    [Fact]
    public void SanitizeFileName_ShouldNotContainInvalidCharacters()
    {
        // Arrange
        var invalidChars = Path.GetInvalidFileNameChars();
        var fileName = $"test{string.Join("", invalidChars)}file";

        // Act
        var result = FileUtils.SanitizeFileName(fileName);

        // Assert
        result.Should()
            .NotBeNull()
            .And.NotContainAny(invalidChars.Select(c => c.ToString()));
    }

    [Fact]
    public void FormatFileSize_ShouldReturnStringWithUnit()
    {
        // Act
        var result = FileUtils.FormatFileSize(1024);

        // Assert
        result.Should()
            .NotBeNull()
            .And.NotBeEmpty()
            .And.Contain("KB")
            .And.MatchRegex(@"^\d+\.?\d*\s+(B|KB|MB|GB)$");
    }

    [Fact]
    public void FormatTime_ShouldReturnFormattedString()
    {
        // Act
        var result = FileUtils.FormatTime(3661);

        // Assert
        result.Should()
            .NotBeNull()
            .And.NotBeEmpty()
            .And.MatchRegex(@"^(\d+h\s)?(\d+m\s)?(\d+s)?$");
    }
}

