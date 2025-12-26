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

global using Microsoft.Extensions.DependencyInjection;

global using Spectre.Console;

global using Serilog;

global using YoutubeExplode;
global using YoutubeExplode.Videos;
global using YoutubeExplode.Videos.Streams;

global using FFMpegCore;

global using YoutubeVideoDownloader.Console.Common.UI;
global using YoutubeVideoDownloader.Console.Common.Utils;
global using YoutubeVideoDownloader.Console.Core.DependencyInjection;
global using YoutubeVideoDownloader.Console.Core.Interfaces;
global using YoutubeVideoDownloader.Console.Core.Services;
global using YoutubeVideoDownloader.Console.Features.About;
global using YoutubeVideoDownloader.Console.Features.Download;
global using YoutubeVideoDownloader.Console.Features.FFmpegSetup;
global using YoutubeVideoDownloader.Console.Features.StreamSelection;
global using YoutubeVideoDownloader.Console.Features.VideoInfo;
global using YoutubeVideoDownloader.Console.Infrastructure.Logging;

