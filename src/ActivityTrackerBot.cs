using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ActivityTracker.src.util;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ActivityTracker.src;

public class ActivityTrackerBot {
    private string TOKEN;
    private DiscordClient DiscordBot { get; set; }
    private SlashCommandsExtension SlashCommands { get; set; }
    private SqliteConnection LoggingDb { get; set; }
    private IBotConfig BotConfig { get; set; }
    private readonly Metadat metadat = new();

    public ActivityTrackerBot(string token, IBotConfig config) {
        TOKEN = token;
        BotConfig = config;
        LoggingDb = new SqliteConnection("Data Source=scmd.log");
        LoggingDb.Open();
        
        var command = LoggingDb.CreateCommand();
        command.CommandText =
        $"""
             CREATE TABLE IF NOT EXISTS commands_log (
             id INTEGER PRIMARY KEY AUTOINCREMENT,
             timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
             user INTEGER,
             command TEXT
             );
         """;
        command.ExecuteNonQuery();
    }

    public async Task RunBotAsync() {
        DiscordConfiguration defaultConfiguration = new DiscordConfiguration {
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt",
            MinimumLogLevel = LogLevel.Debug,
            Token = TOKEN,
            TokenType = TokenType.Bot,
            LogUnknownEvents = true
        };
        DiscordBot = new DiscordClient(defaultConfiguration);
        DiscordBot.SessionCreated += OnReadyEvent;

        var slashCommandConfig = new SlashCommandsConfiguration {
            Services = new ServiceCollection().AddSingleton(BotConfig).BuildServiceProvider()
        };
        SlashCommands = DiscordBot.UseSlashCommands(slashCommandConfig);
        SlashCommands.RegisterCommands<SlashCommands>();
        SlashCommands.SlashCommandErrored += OnSlashCommandError;
        SlashCommands.SlashCommandInvoked += OnSlashCommandInvoked;

        await DiscordBot.ConnectAsync();
        await Task.Delay(-1);
    }

    private async Task OnReadyEvent(DiscordClient discordClient, SessionReadyEventArgs eventArgs) {
        discordClient.Logger.LogInformation($"{metadat.BotName} - {metadat.VersionName} has established a session");
        await SetDefaultStatus(DiscordBot);
    }

    private async Task OnSlashCommandError(SlashCommandsExtension sender, SlashCommandErrorEventArgs eventArgs) {
        if (eventArgs.Exception is SlashExecutionChecksFailedException) {
            await eventArgs.Context.CreateResponseAsync(new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF0000"))
                .WithDescription(":x: You don't have permission for this command! :x:")
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter($"{metadat.BotName} | {metadat.VersionName}")
                .Build()
            );
            return;
        }

        DiscordBot.Logger.LogError(eventArgs.Exception.ToString());
        DiscordBot.Logger.LogError($"{eventArgs.Exception.Message} | {eventArgs.Exception.StackTrace}");
    }

    private async Task OnSlashCommandInvoked(SlashCommandsExtension sender, SlashCommandInvokedEventArgs eventArgs) {
        string insertQuery = "INSERT INTO commands_log (USER, COMMAND) VALUES (@user, @command)";
        using (SqliteCommand commandInsert = new SqliteCommand(insertQuery, LoggingDb)) {
            commandInsert.Parameters.AddWithValue("@user", eventArgs.Context.User.Id);
            commandInsert.Parameters.AddWithValue("@command", eventArgs.Context.CommandName);
            commandInsert.ExecuteNonQuery();
        }
    }

    private async Task SetDefaultStatus(DiscordClient discordClient) {
        DiscordActivity activity = new DiscordActivity("guild players!", ActivityType.Watching);
        await discordClient.UpdateStatusAsync(activity);
    }
}