using ActivityTracker.src;
using Serilog;
using Config.Net;
using Serilog.Formatting.Json;
using ActivityTracker.src.util;
using DSharpPlus;


class Program {
    private static string TOKEN;

    private static int LOG_FILE_SIZE_LIMIT = 20_000_000; // 20MB
    
    static void Main(string[] args) {
        CreateLogger();
        
        IBotConfig config = new ConfigurationBuilder<IBotConfig>()
            .UseJsonFile("conf")
            .Build();

        // Set default
        // (Writing a value will generate the file) 
        config.Id = 1;

        // TODO: Move this into the bot startup logic
        TOKEN = Environment.GetEnvironmentVariable("TOKEN") ?? string.Empty;
        if (TOKEN == string.Empty) {
            Log.Error("No discord token found");
            Close(401);
            // throw new ArgumentNullException("TOKEN", "Invalid Argument");
        }

        ActivityTrackerBot activityTrackerBot = new ActivityTrackerBot(TOKEN, config);
        activityTrackerBot.RunBotAsync().GetAwaiter().GetResult();
        
        Close();
    }

    static void CreateLogger() {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: "latest.log",
                fileSizeLimitBytes: LOG_FILE_SIZE_LIMIT,
                retainedFileCountLimit: 100,
                shared: true,
                formatter: new JsonFormatter()
            )
            .MinimumLevel.Debug()
            .CreateLogger();
        
        Log.Debug("logger created");
    }
    
    static void Close() {
        Log.Debug("closing logger");
        Log.CloseAndFlush();
        Environment.Exit(0);
    }

    static void Close(int exitCode) {
        Log.Debug("closing logger and terminating process");
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }
}