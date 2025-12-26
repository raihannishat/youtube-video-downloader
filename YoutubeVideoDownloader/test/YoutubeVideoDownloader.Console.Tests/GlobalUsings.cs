global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.Extensions.DependencyInjection;

global using Xunit;
global using FluentAssertions;
global using Moq;

global using Serilog;

global using YoutubeExplode;
global using YoutubeExplode.Videos;
global using YoutubeExplode.Videos.Streams;

global using YoutubeVideoDownloader.Console.Core.Interfaces;
global using YoutubeVideoDownloader.Console.Core.Services;
global using YoutubeVideoDownloader.Console.Core.DependencyInjection;
global using YoutubeVideoDownloader.Console.Common.Utils;
global using YoutubeVideoDownloader.Console.Features.Download;
global using YoutubeVideoDownloader.Console.Infrastructure.Logging;

