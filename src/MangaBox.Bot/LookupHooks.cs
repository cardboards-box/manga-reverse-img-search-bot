namespace MangaBox.Bot;

using Services;

using CacheMessage = Cacheable<IUserMessage, ulong>;
using CacheChannel = Cacheable<IMessageChannel, ulong>;

/// <summary>
/// A service for hooking into the discord client and watching for events
/// </summary>
public interface ILookupHooks
{
    /// <summary>
    /// Setups the hooks for the discord client
    /// </summary>
    Task Setup();
}

internal class LookupHooks(
    DiscordSocketClient _client,
    IMangaLookupService _lookup,
    ILogger<LookupHooks> _logger) : ILookupHooks
{
    public Task Setup()
    {
        //Forward the hooks to a different thread to not block the bot's connection thread
        //This also boxes errors for better error handling and logging.
        _client.MessageReceived += (m) => Background(() => HandleMessage(m), "message");
        _client.ReactionAdded += (m, c, r) => Background(() => HandleReaction(m, c, r), "reaction");
        return Task.CompletedTask;
    }

    public Task Background(Func<Task> task, string message)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LOOKUP HOOK] Error occurred while handling {message}", message);
            }
        });
        return Task.CompletedTask;
    }

    public async Task HandleReaction(CacheMessage message, CacheChannel _, SocketReaction reaction)
    {
        //Get the context from the reaction
        var ctx = await _lookup.GetContext(message, reaction);
        if (ctx is null || string.IsNullOrEmpty(ctx.ImageUrl)) return;

        //Process the request but don't ping the person who sent the original message
        await _lookup.ProcessRequest(ctx, PingType.JustMention);
    }

    public async Task HandleMessage(SocketMessage message)
    {
        //Get the context from the message
        var ctx = await _lookup.GetContext(message);
        if (ctx is null || string.IsNullOrEmpty(ctx.ImageUrl)) return;
        
        //Process the request but don't ping the person who sent the original message
        await _lookup.ProcessRequest(ctx, PingType.JustMention);
    }
}
