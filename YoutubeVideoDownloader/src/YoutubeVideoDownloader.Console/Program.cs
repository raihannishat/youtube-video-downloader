Console.OutputEncoding = Encoding.UTF8;

// Configure services
var services = new ServiceCollection();
services.AddApplicationServices();
var serviceProvider = services.BuildServiceProvider();

// Get services
var ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();
var applicationService = serviceProvider.GetRequiredService<IApplicationService>();

// Setup FFmpeg
await FFmpegSetupHandler.SetupFFmpegAsync(ffmpegService);

// Run application
await applicationService.RunAsync();

// Cleanup
Log.CloseAndFlush();
