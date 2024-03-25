using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ActivityTracker.src;

public class SlashCommands : ApplicationCommandModule {
    [SlashCommand("ping", "Replies to command")]
    public async Task PingSlashCommand(InteractionContext ctx) {
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("Pong!"));
    }
}