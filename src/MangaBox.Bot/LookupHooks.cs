namespace MangaBox.Bot;

using Services;

using CacheMessage = Cacheable<IUserMessage, ulong>;
using CacheChannel = Cacheable<IMessageChannel, ulong>;

public interface ILookupHooks
{
    Task Setup();
}

internal class LookupHooks(
    DiscordSocketClient _client,
    IMangaLookupService _lookup,
    ILookupDbService _db,
    IJsonService _json,
    ILookupConfig _config,
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

    public bool IsTrigger(IEmote emote)
    {
        var comp = StringComparison.InvariantCultureIgnoreCase;
        if (_config.Emotes.Any(t => t.Equals(emote.Name, comp))) 
            return true;

        if (emote is Emote e)
        {
            var marked = $"<{(e.Animated ? "a" : "")}:{e.Name}:{e.Id}>";
            return _config.Emotes.Any(t => t.Equals(marked, comp));
        }

        return false;
    }

    public async Task HandleReaction(CacheMessage message, CacheChannel channel, SocketReaction reaction)
    {
        //Sanity checks
        if (//Ensure the user is specified on the reaction
            !reaction.User.IsSpecified ||
            //Ensure the reaction is not from a bot
            reaction.User.Value.IsBot ||
            //Ensure the reaction is in a server
            reaction.Channel is not SocketGuildChannel guild ||
            //Ensure the emote is a trigger
            !IsTrigger(reaction.Emote)) 
            return;

        //Get or download the cached message and channel
        var msg = await message.GetOrDownloadAsync();
        var chn = await channel.GetOrDownloadAsync();

        //Determine the image URL from the reacted message
        var image = _lookup.DetermineUrl(msg);
        //If there are no images, skip processing
        if (string.IsNullOrEmpty(image)) return;

        //Pull out the IDs for various things
        string messageId = msg.Id.ToString(),
            guildId = guild.Guild.Id.ToString(),
            channelId = guild.Id.ToString(),
            authorId = reaction.UserId.ToString();
        //Check to see if someone has already looked up this message
        var existing = await _db.Fetch(messageId);
        //If there is a result, call the person an idiot.
        if (existing is not null && !string.IsNullOrEmpty(existing.Results))
        {
            await HandleIdiots(guild, msg, authorId, existing);
            return;
        }

        //Create a reference to the reacted message
        var rpl = new MessageReference(msg.Id, channel.Id, guild.Guild.Id);
        //Create a loading message to tell the user we're doing something
        //This references the reacted message, so we want to avoid pinging the message author
        //We also need to keep a reference to the message so we can updated it after the results come in
        var mod = await CreateLoadingMessage(msg, rpl, authorId);
        //Make the request to the lookup service and track the results
        var lookup = await WrapRequest(new LookupRequest
        {
            MessageId = messageId,
            GuildId = guildId,
            ChannelId = channelId,
            AuthorId = authorId,
            ImageUrl = image,
            Results = null,
            ResponseId = mod.Id.ToString(),
        }, image);
        //Modify the loading message to show the results
        await mod.ModifyAsync(t =>
        {
            t.Embeds = lookup.Embeds;
            t.Content = lookup.Message;
        });
    }

    public bool IsTrigger(SocketMessage message)
    {
        //Ensure the author isn't a bot
        if (message.Author.IsBot) 
            return false;
        //Ensure the message mentions the bot
        if (!message.MentionedUsers.Any(t => t.Id == _client.CurrentUser.Id)) 
            return false;

        return true;
    }

    public async Task HandleMessage(SocketMessage message)
    {
        //Sanity checks
        if (!IsTrigger(message) ||
            //Ensure the message was sent in a server
            message.Channel is not SocketGuildChannel channel) return;

        //Check if the message contains an image url or attachment
        var image = _lookup.DetermineUrl(message);
        //If there is an image, assume a direct lookup
        if (!string.IsNullOrWhiteSpace(image))
        {
            await HandleDirectRequest(message, channel, image);
            return;
        }

        //Create a reference to the message requesting a lookup
        var rpl = new MessageReference(message.Id, channel.Id, channel.Guild.Id);
        //Check to see if the trigger message references another message
        if (message.Reference is null || !message.Reference.MessageId.IsSpecified)
        {
            //Not a reference message and there's no image, so we can't do anything
            await message.Channel.SendMessageAsync("Could not find any images to search for.",
                messageReference: rpl);
            return;
        }

        //Get the referenced message by it's ID
        var msg = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
        //Couldn't load the referenced message (this shouldn't happen)
        if (msg is null) return;

        //Determine the image URL from the referenced message
        image = _lookup.DetermineUrl(msg);
        if (string.IsNullOrEmpty(image))
        {
            //No image url found, so we can't do anything
            await message.Channel.SendMessageAsync("Could not find any images to search for on referenced message.",
                messageReference: rpl);
            return;
        }

        //Pull out the IDs for various things
        string messageId = msg.Id.ToString(),
               guildId = channel.Guild.Id.ToString(),
               channelId = channel.Id.ToString(),
               authorId = message.Author.Id.ToString();

        //Check to see if someone has already looked up this message
        var existing = await _db.Fetch(messageId);
        //If there is a result, call the person an idiot.
        if (existing is not null && !string.IsNullOrEmpty(existing.Results))
        {
            await HandleIdiots(channel, msg, authorId, existing);
            return;
        }

        //Create a loading message to tell the user we're doing something
        //We need to keep a reference to the message so we can updated it after the results come in
        var mod = await CreateLoadingMessage(msg, rpl, authorId);
        //Make the request to the lookup service and track the results
        var lookup = await WrapRequest(new LookupRequest
        {
            MessageId = messageId,
            GuildId = guildId,
            ChannelId = channelId,
            AuthorId = authorId,
            ImageUrl = image,
            Results = null,
            ResponseId = mod.Id.ToString(),
        }, image);
        //Modify the loading message to show the results
        await mod.ModifyAsync(t =>
        {
            t.Embeds = lookup.Embeds;
            t.Content = lookup.Message;
        });
    }

    public async Task HandleDirectRequest(SocketMessage message, SocketGuildChannel channel, string image)
    {
        //Create a reference to the message requesting a lookup
        var rpl = new MessageReference(message.Id, channel.Id, channel.Guild.Id);
        //Pull out the IDs for various things
        string messageId = message.Id.ToString(),
            guildId = channel.Guild.Id.ToString(),
            channelId = channel.Id.ToString(),
            authorId = message.Author.Id.ToString();
        //Create a loading message to tell the user we're doing something
        //We need to keep a reference to the message so we can updated it after the results come in
        //No need to ping the author since they're the one who sent the lookup request
        var mod = await CreateLoadingMessage(message, rpl, null);
        //Make the request to the lookup service and track the results
        var lookup = await WrapRequest(new LookupRequest
        {
            MessageId = messageId,
            GuildId = guildId,
            ChannelId = channelId,
            AuthorId = authorId,
            ImageUrl = image,
            Results = null,
            ResponseId = mod.Id.ToString(),
        }, image);
        //Modify the loading message to show the results
        await mod.ModifyAsync(t =>
        {
            t.Embeds = lookup.Embeds;
            t.Content = lookup.Message;
        });
    }

    public async Task<IUserMessage> CreateLoadingMessage(IMessage message, MessageReference? reference, string? authorId)
    {
        var men = AllowedMentions.None;
        men.MentionRepliedUser = false;
        var author = string.IsNullOrEmpty(authorId) ? "" : $"<@{authorId}> ";
        var output = $"{author}<a:loading:1048471999065903244> Processing your request...";

        return await message.Channel.SendMessageAsync(output, 
            messageReference: reference,
            allowedMentions: men);
    }

    public async Task HandleIdiots(SocketGuildChannel channel, IMessage msg, string authorId, LookupRequest request)
    {
        var rpl = new MessageReference(ulong.Parse(request.ResponseId), channel.Id, channel.Guild.Id);
        var men = AllowedMentions.All;
        men.MentionRepliedUser = false;
        await msg.Channel.SendMessageAsync($"Uh, <@{authorId}>, it's right here...",
            messageReference: rpl,
            allowedMentions: men);
    }

    public async Task<Lookup> WrapRequest(LookupRequest request, string image)
    {
        //Insert the request into the database
        request.Id = await _db.Upsert(request);
        //Make the request to lookup the image source
        var result = await _lookup.Lookup(image);
        //Update the request with the results of the lookup
        request.Results = _json.Serialize(result);
        //Update the database with the results
        await _db.Upsert(request);
        //Return the lookup results
        return result;
    }
}
