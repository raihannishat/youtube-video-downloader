# YouTube Video Downloader

A powerful, feature-rich console-based YouTube video downloader built with C# and .NET 10. This application allows you to download YouTube videos in various qualities with automatic video/audio merging capabilities.

## 📸 Screenshots

#### 1. Application Interface

![Application Interface](Contents/Screenshot%202025-12-26%20205157.png)

*Main application interface showing the welcome screen and URL input prompt*

#### 2. Video Information Display

![Video Information Display](Contents/Screenshot%202025-12-26%20205212.png)

*Video information and available quality options*

#### 3. About Page

*Access the about page by typing 'a' or 'i' when prompted for a URL. The about page displays developer information, application features, technologies used, and version details.*

#### 4. Download Progress

![Download Progress](Contents/Screenshot%202025-12-26%20205240.png)

*IDM-style real-time download progress display showing progress bar, percentage, download speed (MB/s), and estimated time of arrival (ETA)*

## 📋 Table of Contents

- [Screenshots](#-screenshots)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Logging](#logging)
- [Developer Information](#developer-information)
- [License](#license)

## ✨ Features

- **🎥 Multiple Quality Options**: Download videos in various resolutions (360p, 480p, 720p, 1080p, etc.)
- **🔀 Automatic Merging**: Automatically merges separate video and audio streams for higher quality downloads
- **🎵 Audio-Only Downloads**: Download audio-only streams in various bitrates
- **📊 Real-Time Progress**: IDM-style progress display with download speed, percentage, and ETA
- **🎨 Beautiful UI**: Modern console interface using Spectre.Console with centered content and ASCII art
- **🔧 Auto FFmpeg Setup**: Automatically downloads and configures FFmpeg if not found
- **📝 File Logging**: Comprehensive logging using Serilog with daily rolling log files
- **🏗️ Clean Architecture**: Vertical Slice Architecture with Dependency Injection
- **✅ Unit Tests**: Comprehensive unit test coverage with xUnit and FluentAssertions
- **🌐 URL Normalization**: Automatically handles YouTube URLs with or without protocol


## 🛠️ Technologies Used

- **.NET 10.0** - Latest .NET framework
- **YoutubeExplode (6.5.6)** - YouTube video metadata and stream extraction
- **FFMpegCore (5.1.0)** - FFmpeg wrapper for video/audio merging
- **Spectre.Console (0.54.1-alpha.0.10)** - Beautiful console UI components
- **Serilog (4.1.0)** - Structured logging framework
- **Serilog.Sinks.File (6.0.0)** - File logging sink
- **Microsoft.Extensions.DependencyInjection (10.0.0)** - Dependency injection container
- **xUnit (2.9.2)** - Unit testing framework
- **Moq (4.20.72)** - Mocking framework for unit tests
- **FluentAssertions (7.0.0)** - Fluent assertion library

## 📦 Prerequisites

- **.NET 10.0 SDK** or later
- **Windows OS** (FFmpeg auto-download is configured for Windows)
- **Internet Connection** (for downloading videos and FFmpeg setup)
- **Git** (optional, for cloning the repository)

## 🚀 Installation Guide

This guide will walk you through installing everything needed to run the YouTube Video Downloader application.

### Step 1: Install .NET 10.0 SDK

The application requires .NET 10.0 SDK to build and run. Follow these steps:

#### Option A: Download from Microsoft (Recommended)

1. **Visit the .NET Download Page**
   - Go to: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)

2. **Download .NET 10.0 SDK**
   - Click on **".NET 10.0 SDK"** (not just Runtime)
   - Select your operating system (Windows x64)
   - Download the installer

3. **Run the Installer**
   - Double-click the downloaded installer
   - Follow the installation wizard
   - Accept the license agreement
   - Click **Install**
   - Wait for installation to complete

4. **Verify Installation**
   - Open Command Prompt or PowerShell
   - Run the following command:
   ```bash
   dotnet --version
   ```
   - You should see version `10.0.x` or higher
   - If you see an error, restart your terminal or computer

#### Option B: Install via Winget (Windows Package Manager)

If you have Winget installed:

```bash
winget install Microsoft.DotNet.SDK.10
```

#### Option C: Install via Chocolatey

If you have Chocolatey installed:

```bash
choco install dotnet-10.0-sdk
```

### Step 2: Install Git (Optional - Only if cloning from GitHub)

If you want to clone the repository, you'll need Git:

1. **Download Git**
   - Visit: [https://git-scm.com/download/win](https://git-scm.com/download/win)
   - Download the Windows installer

2. **Install Git**
   - Run the installer
   - Use default settings (recommended)
   - Complete the installation

3. **Verify Installation**
   ```bash
   git --version
   ```

### Step 3: Clone the Repository

#### Option A: Using Git (Recommended)

1. **Open Command Prompt or PowerShell**

2. **Navigate to your desired directory**
   ```bash
   cd C:\Users\YourName\Documents
   ```

3. **Clone the repository**
   ```bash
   git clone https://github.com/raihannishat/youtube-video-downloader.git
   ```

4. **Navigate to the project directory**
   ```bash
   cd youtube-video-downloader\YoutubeVideoDownloader
   ```

#### Option B: Download as ZIP

1. **Visit the GitHub Repository**
   - Go to: [https://github.com/raihannishat/youtube-video-downloader](https://github.com/raihannishat/youtube-video-downloader)

2. **Download ZIP**
   - Click the green **"Code"** button
   - Select **"Download ZIP"**
   - Extract the ZIP file to your desired location

3. **Navigate to the project directory**
   ```bash
   cd path\to\extracted\folder\youtube-video-downloader\YoutubeVideoDownloader
   ```

### Step 4: Restore NuGet Packages

The first time you open the project, restore all NuGet packages:

```bash
dotnet restore
```

This will download all required packages (YoutubeExplode, FFMpegCore, Spectre.Console, etc.)

### Step 5: Build the Project

Build the project to compile the application:

```bash
dotnet build
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

If you see errors, make sure:
- .NET 10.0 SDK is properly installed
- You're in the correct directory
- Internet connection is active (for NuGet packages)

### Step 6: Run the Application

#### Option A: Run from Source (Development)

```bash
dotnet run --project src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj
```

#### Option B: Build and Run Executable

1. **Build a Release version**
   ```bash
   dotnet build --configuration Release
   ```

2. **Run the executable**
   ```bash
   .\src\YoutubeVideoDownloader.Console\bin\Release\net10.0\YoutubeVideoDownloader.Console.exe
   ```

#### Option C: Publish Standalone Application

Create a standalone executable that doesn't require .NET runtime:

```bash
dotnet publish src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj -c Release -r win-x64 --self-contained true
```

The executable will be in:
```
src\YoutubeVideoDownloader.Console\bin\Release\net10.0\win-x64\publish\YoutubeVideoDownloader.Console.exe
```

### Step 7: First Run Setup

When you run the application for the first time:

1. **FFmpeg Auto-Setup**
   - The application will check for FFmpeg
   - If not found, it will automatically download and configure FFmpeg
   - This is a one-time process (approximately 50-100 MB download)
   - FFmpeg will be stored in the application directory

2. **Log Files**
   - Log files will be created in `logs/` directory
   - Logs are automatically rotated daily

### Step 8: Verify Installation

To verify everything is working:

1. **Run the application**
   ```bash
   dotnet run --project src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj
   ```

2. **Test with a YouTube URL**
   - Enter a YouTube video URL when prompted
   - The application should display video information
   - You should see available quality options

### Troubleshooting Installation

#### Problem: `dotnet` command not found

**Solution:**
- Restart your terminal/Command Prompt
- Restart your computer
- Verify .NET SDK installation: Check `C:\Program Files\dotnet\`
- Add to PATH manually if needed

#### Problem: Build errors related to NuGet packages

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages again
dotnet restore

# Rebuild
dotnet build --no-incremental
```

#### Problem: FFmpeg download fails

**Solution:**
- Check internet connection
- Ensure firewall/antivirus isn't blocking downloads
- Manually download FFmpeg from [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
- Place `ffmpeg.exe` in the application directory

#### Problem: Application crashes on startup

**Solution:**
- Check log files in `logs/` directory
- Verify .NET 10.0 runtime is installed
- Run with verbose logging:
  ```bash
  dotnet run --project src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj --verbosity detailed
  ```

### Optional: Run Tests

To verify the installation and run unit tests:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test test/YoutubeVideoDownloader.Console.Tests/YoutubeVideoDownloader.Console.Tests.csproj
```

**Expected Output:**
```
Test Run Successful.
Total tests: 51
     Passed: 51
     Failed: 0
```

### Quick Start Summary

For experienced users, here's the quick installation:

```bash
# 1. Install .NET 10.0 SDK (from Microsoft website)
# 2. Clone repository
git clone https://github.com/raihannishat/youtube-video-downloader.git
cd youtube-video-downloader\YoutubeVideoDownloader

# 3. Restore and build
dotnet restore
dotnet build

# 4. Run
dotnet run --project src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj
```

## 💻 Usage

### Starting the Application

1. Run the application using `dotnet run` or execute the compiled binary
2. The application will display a welcome header and check for FFmpeg
3. If FFmpeg is not found, it will automatically download and configure it (one-time setup)

### Downloading Videos

1. **Enter YouTube URL**: When prompted, paste a YouTube video URL
   - Supports full URLs: `https://www.youtube.com/watch?v=VIDEO_ID`
   - Supports short URLs: `https://youtu.be/VIDEO_ID`
   - Auto-adds `https://` if protocol is missing

2. **View Video Information**: The application displays:
   - Video title
   - Channel name
   - Duration

3. **Select Quality**: Choose from available options:
   - **Muxed Streams** (Video + Audio combined) - Ready to download
   - **Higher Quality** (Separate video/audio) - Auto-merged with FFmpeg
   - **Audio Only** - Various bitrate options

4. **Download**: The application will:
   - Show real-time progress with speed and ETA
   - Automatically merge video and audio if needed
   - Save to your Downloads folder

### Commands

- **`q` or `Q`** - Quit the application
- **`a` or `i`** - Show about/information page
- **Enter** (empty input) - Select highest available quality

### Example

```
📺 Enter YouTube Video URL (or 'q'/'Q' to quit, 'a'/'i' for about): https://youtu.be/VIDEO_ID

📹 Video Information
Title: Example Video
Channel: Example Channel
Duration: 00:10:30

Available Quality Options:
1. 720p (mp4) - 120.5 MB
2. 1080p (mp4) - 250.3 MB (Auto-merged)

🎯 Select quality (number or Enter for highest): 2

Downloading Video Stream...
[████████████████████] 100% | 5.2 MB/s | ETA: 0s

Downloading Audio Stream...
[████████████████████] 100% | 2.1 MB/s | ETA: 0s

Merging video and audio streams...
✓ Merge completed!

✓ Successfully downloaded!
📁 File: Example_Video.mp4
📂 Location: C:\Users\YourName\Downloads\Example_Video.mp4
```


## 🏗️ Architecture

### Vertical Slice Architecture

The project follows **Vertical Slice Architecture**, organizing code by features rather than technical layers:

```
src/YoutubeVideoDownloader.Console/
├── Core/                    # Core business logic
│   ├── Interfaces/          # Service interfaces
│   ├── Services/            # Service implementations
│   └── DependencyInjection/ # DI configuration
├── Features/                # Feature-specific handlers
│   ├── About/               # About page feature
│   ├── Download/            # Download feature
│   ├── FFmpegSetup/         # FFmpeg setup feature
│   ├── StreamSelection/     # Stream selection feature
│   └── VideoInfo/           # Video info display feature
├── Common/                  # Shared utilities
│   ├── UI/                  # UI helpers
│   └── Utils/               # Utility functions
└── Infrastructure/          # Infrastructure concerns
    └── Logging/             # Logging configuration
```

### Dependency Injection

All services are registered using Microsoft.Extensions.DependencyInjection:

- **IYouTubeService** → **YouTubeService**
- **IDownloadService** → **DownloadService**
- **IFFmpegService** → **FFmpegService**
- **IDownloadAndMergeService** → **DownloadAndMergeHandler**
- **IApplicationService** → **ApplicationService**
- **ILoggerService** → **SerilogLoggerService**

### Design Patterns

- **Dependency Injection**: All dependencies are injected through constructors
- **Interface Segregation**: Services are defined by focused interfaces
- **Single Responsibility**: Each class has a single, well-defined purpose
- **Separation of Concerns**: Business logic separated from infrastructure

## 📁 Project Structure

```
youtube-video-downloader/
└── YoutubeVideoDownloader/
    ├── src/
    │   └── YoutubeVideoDownloader.Console/
    │       ├── Core/
    │       │   ├── Interfaces/          # Service interfaces
    │       │   ├── Services/            # Service implementations
    │       │   └── DependencyInjection/ # DI setup
    │       ├── Features/                # Feature handlers
    │       │   ├── About/
    │       │   ├── Download/
    │       │   ├── FFmpegSetup/
    │       │   ├── StreamSelection/
    │       │   └── VideoInfo/
    │       ├── Common/                  # Shared code
    │       │   ├── UI/
    │       │   └── Utils/
    │       ├── Infrastructure/          # Infrastructure
    │       │   └── Logging/
    │       ├── GlobalUsings.cs          # Global using statements
    │       ├── Program.cs               # Application entry point
    │       └── YoutubeVideoDownloader.Console.csproj
    ├── test/
    │   └── YoutubeVideoDownloader.Console.Tests/
    │       ├── Core/
    │       ├── Common/
    │       ├── Features/
    │       ├── Infrastructure/
    │       ├── GlobalUsings.cs
    │       └── YoutubeVideoDownloader.Console.Tests.csproj
    └── YoutubeVideoDownloader.slnx
```

## 🧪 Testing

### Test Framework

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent assertion syntax

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~FileUtilsTests"
```

### Test Coverage

- **FileUtilsTests** - File utility methods (SanitizeFileName, FormatFileSize, FormatTime)
- **YouTubeServiceTests** - YouTube service functionality
- **DownloadServiceTests** - Download service with null checks
- **FFmpegServiceTests** - FFmpeg service availability checks
- **DownloadAndMergeHandlerTests** - Download and merge operations
- **SerilogLoggerServiceTests** - Logging service methods
- **ServiceCollectionExtensionsTests** - DI container registration

### Test Statistics

- **Total Tests**: 51
- **Test Framework**: xUnit
- **Assertion Library**: FluentAssertions

## 📝 Logging

### Log Configuration

- **Framework**: Serilog
- **Sink**: File (daily rolling)
- **Location**: `logs/youtube-downloader-YYYYMMDD.log`
- **Retention**: 7 days
- **Format**: `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}`

### Log Levels

- **Information** - General application flow
- **Warning** - Non-critical issues
- **Error** - Exceptions and errors
- **Debug** - Detailed debugging information

### Example Log Entry

```
2025-12-26 14:30:45.123 +06:00 [INF] Application started
2025-12-26 14:30:46.234 +06:00 [INF] Processing URL: https://youtu.be/VIDEO_ID
2025-12-26 14:30:47.345 +06:00 [INF] Download completed successfully: C:\Users\...\video.mp4
```

## 🎯 Key Features Explained

### Automatic FFmpeg Setup

- Checks for FFmpeg in common locations
- Automatically downloads FFmpeg if not found
- Configures FFmpeg for video/audio merging
- One-time setup, persists across sessions

### Stream Types

1. **Muxed Streams**: Video and audio combined (360p-720p typically)
   - Ready to download immediately
   - No merging required

2. **Separate Streams**: Higher quality video (1080p+) with separate audio
   - Downloads video and audio separately
   - Automatically merges using FFmpeg

3. **Audio-Only**: Audio streams in various bitrates
   - Perfect for music downloads
   - Multiple quality options

### Progress Display

- **Progress Bar**: Visual representation of download progress
- **Percentage**: Exact completion percentage
- **Speed**: Real-time download speed (MB/s, KB/s)
- **ETA**: Estimated time of arrival
- **File Info**: File name and size

## 👨‍💻 Developer Information

**Developer**: Raihan Nishat

- **GitHub**: [https://github.com/raihannishat](https://github.com/raihannishat)
- **LinkedIn**: [https://bd.linkedin.com/in/raihan-nishat-679455163](https://bd.linkedin.com/in/raihan-nishat-679455163)

## 📄 License

© 2025 All rights reserved

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 🐛 Known Issues

- FFmpeg auto-download is currently Windows-only
- Some videos may have restricted access (private, age-restricted, etc.)

## 🔮 Future Enhancements

- [ ] Playlist download support
- [ ] Subtitle download
- [ ] Custom output directory selection
- [ ] Batch download from file
- [ ] Cross-platform FFmpeg support
- [ ] Configuration file support

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub.

---

**Version**: 1.0.0  
**Last Updated**: December 2025
