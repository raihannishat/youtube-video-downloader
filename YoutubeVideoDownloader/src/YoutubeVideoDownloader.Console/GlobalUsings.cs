// System namespaces - Core .NET framework types and utilities
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Net.Http;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

// Microsoft.Extensions - Dependency Injection framework
global using Microsoft.Extensions.DependencyInjection;

// Spectre.Console - Rich console UI library for terminal applications
global using Spectre.Console;

// Serilog - Structured logging framework
global using Serilog;

// YoutubeExplode - YouTube video and playlist extraction library
global using YoutubeExplode;
global using YoutubeExplode.Videos;
global using YoutubeExplode.Videos.Streams;

// FFMpegCore - FFmpeg wrapper for video/audio processing
global using FFMpegCore;

// Project-specific namespaces - Application modules and features
// Common - Shared utilities and UI helpers
global using YoutubeVideoDownloader.Console.Common.UI;
global using YoutubeVideoDownloader.Console.Common.Utils;

// Core - Business logic, services, and dependency injection
global using YoutubeVideoDownloader.Console.Core.DependencyInjection;
global using YoutubeVideoDownloader.Console.Core.Interfaces;
global using YoutubeVideoDownloader.Console.Core.Services;

// Features - Feature-specific handlers organized by vertical slice architecture
global using YoutubeVideoDownloader.Console.Features.About;
global using YoutubeVideoDownloader.Console.Features.DirectorySelection;
global using YoutubeVideoDownloader.Console.Features.Download;
global using YoutubeVideoDownloader.Console.Features.FFmpegSetup;
global using YoutubeVideoDownloader.Console.Features.Playlist;
global using YoutubeVideoDownloader.Console.Features.StreamSelection;
global using YoutubeVideoDownloader.Console.Features.VideoInfo;

// Infrastructure - Cross-cutting concerns like logging
global using YoutubeVideoDownloader.Console.Infrastructure.Logging;

