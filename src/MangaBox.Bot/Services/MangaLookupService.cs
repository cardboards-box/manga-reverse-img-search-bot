namespace MangaBox.Bot.Services;

using CacheMessage = Cacheable<IUserMessage, ulong>;

/// <summary>
/// A service for utilities related to looking up manga
/// </summary>
public interface IMangaLookupService
{
    /// <summary>
    /// Download the image from the url, search for it, and then generate the embeds
    /// </summary>
    /// <param name="url">The image URL</param>
    /// <param name="config">The configuration for the guild</param>
    /// <returns>The lookup results</returns>
    Task<Lookup> Lookup(string url, GuildConfig config);

    /// <summary>
    /// Get the configuration for the given guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <returns>The config or defaults if no config exists</returns>
    /// <remarks>Passing null as <paramref name="guildId"/> will always give defaults</remarks>
    Task<GuildConfig> GetConfig(string? guildId);

    /// <summary>
    /// Get the lookup context from a reaction
    /// </summary>
    /// <param name="message">The message that was reacted to</param>
    /// <param name="reaction">The reaction to the message</param>
    /// <returns>The context for the lookup</returns>
    Task<LookupContext?> GetContext(CacheMessage message, SocketReaction reaction);

    /// <summary>
    /// Get the lookup context from a message
    /// </summary>
    /// <param name="message">The message that triggered the interaction</param>
    /// <returns>The context for the lookup</returns>
    Task<LookupContext?> GetContext(SocketMessage message);

    /// <summary>
    /// Process the lookup context after it's been validated
    /// </summary>
    /// <param name="ctx">The lookup context</param>
    /// <param name="ping">Whether or not to ping the requester</param>
    Task ProcessRequest(LookupContext ctx, PingType ping);

    /// <summary>
    /// Send a message to the user calling them an idiot for repeating lookups
    /// </summary>
    /// <param name="ctx">The lookup context</param>
    /// <param name="request">The previous lookup request</param>
    Task HandleIdiots(LookupContext ctx, LookupRequest request);

    /// <summary>
    /// Checks to see if the user executing the command has admin perms
    /// </summary>
    /// <param name="cmd">The slash command that was executed</param>
    /// <returns>Whether or not the user is an admin</returns>
    bool IsAdmin(SocketSlashCommand cmd);
}

internal class MangaLookupService(
    IFileService _file,
    ISearchApiService _search,
    IEmbedService _embed,
    ILookupConfig _config,
    IJsonService _json,
    ILookupDbService _db,
    DiscordSocketClient _client,
    ILogger<MangaLookupService> _logger) : IMangaLookupService
{
    private string[]? _botMentions;

    public bool IsAdmin(SocketSlashCommand cmd)
    {
        if (_config.AuthorizedUsers.Contains(cmd.User.Id.ToString()))
            return true;

        if (cmd.User is not SocketGuildUser user)
            return false;

        return user.GuildPermissions.ManageGuild;
    }

    public async Task<Lookup> Lookup(string url, GuildConfig config)
    {
        try
        {
            var local = await _file.DownloadImage(url);
            if (local is null)
                return new(false, config.MessageDownloadFailed!, []);

            var (io, filename) = local.Value;
            using var stream = io;
            var search = await _search.SearchV2(io, filename);
            if (search is null || !search.Success || search.Data is null || search.Data.Length == 0)
                return new(false, config.MessageNoResults!, []);

            var embeds = _embed.GenerateEmbeds(search.Data, url).ToArray();
            if (embeds.Length == 0)
                return new(false, config.MessageNoResults!, []);

            return new(true, config.MessageSucceeded!, embeds, null, search.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while looking up image: {url}", url);
            return new(false, 
                string.Format(config.MessageError!, ex.Message), []);
        }
    }

    public static string? DetermineUrl(IMessage msg)
    {
        var img = msg.Attachments.FirstOrDefault(t => 
            t.ContentType.StartsWithIc("image"));
        if (img != null) return img.Url;

        var content = msg.Content;
        content = content.Trim();
        if (Uri.IsWellFormedUriString(content, UriKind.Absolute)) return content;

        var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        foreach (Match m in linkParser.Matches(content))
            if (Uri.IsWellFormedUriString(m.Value, UriKind.Absolute))
                return m.Value;

        return null;
    }

    public static AllowedMentions Mentions(PingType type)
    {
        AllowedMentions mentions;
        switch(type)
        {
            case PingType.All:
                mentions = AllowedMentions.All;
                mentions.MentionRepliedUser = true;
                return mentions;
            case PingType.None:
                mentions = AllowedMentions.None;
                mentions.MentionRepliedUser = false;
                return mentions;
            case PingType.JustReply:
                mentions = AllowedMentions.None;
                mentions.MentionRepliedUser = true;
                return mentions;
            default:
                mentions = AllowedMentions.All;
                mentions.MentionRepliedUser = false;
                return mentions;
        }
    }

    public async Task<bool> HasInteracted(string messageId, string userId)
    {
        if (await _db.Interaction(messageId, userId) is not null)
            return true;

        await _db.Upsert(new LookupInteraction
        {
            MessageId = messageId,
            UserId = userId
        });
        return false;
    }

    public async Task HandleIdiots(LookupContext ctx, LookupRequest request)
    {
        //Create a reference to the message the bot sent previously
        var rpl = new MessageReference(
            ulong.Parse(request.ResponseId), ctx.Channel.Id, ctx.Channel.Guild.Id);
        //Generate the message calling the author an idiot
        var output = string.Format(ctx.Config.MessageIdiots!, ctx.AuthorId);
        //Send message showing the author is an idiot
        await ctx.SourceMessage.Channel.SendMessageAsync(output,
            messageReference: rpl,
            allowedMentions: Mentions(PingType.JustMention));
    }

    public static bool CheckLists(LookupContext ctx)
    {
        if (ctx.Config.ChannelsWhitelist.Length > 0)
            return ctx.Config.ChannelsWhitelist.Contains(ctx.ChannelId);

        if (ctx.Config.ChannelsBlacklist.Length > 0)
            return !ctx.Config.ChannelsBlacklist.Contains(ctx.ChannelId);

        return true;
    }

    public async Task ProcessRequest(LookupContext ctx, PingType ping)
    {
        //Ensure the channel isn't white/black listed.
        if (!CheckLists(ctx)) return;
        //If teh user has already reacted to this message, ignore them
        if (await HasInteracted(ctx.MessageId, ctx.AuthorId)) return;
        //Check to see if someone has already looked up this message
        var existing = await _db.Request(ctx.MessageId);
        //If there is a result, call the person an idiot.
        if (existing is not null && !string.IsNullOrEmpty(existing.Results))
        {
            await HandleIdiots(ctx, existing);
            return;
        }
        //Get the loading message
        var output = string.Format(ctx.Config.MessageLoading!, ctx.AuthorId);
        //Create a message to tell the user we're doing something
        var mod = await ctx.SourceMessage.Channel.SendMessageAsync(output,
            messageReference: ctx.ReplyingTo,
            allowedMentions: Mentions(ping));
        //Create a request tracking object
        var tracker = new LookupRequest
        {
            MessageId = ctx.MessageId,
            GuildId = ctx.GuildId,
            ChannelId = ctx.ChannelId,
            AuthorId = ctx.AuthorId,
            ImageUrl = ctx.ImageUrl!,
            Results = null,
            ResponseId = mod.Id.ToString()
        };
        //Insert the request into the database
        tracker.Id = await _db.Upsert(tracker);
        //Make the request to lookup the image source
        var result = await Lookup(ctx.ImageUrl!, ctx.Config);
        //Update the request with the results of the lookup
        tracker.Results = _json.Serialize(result);
        //Update the database with the results
        await _db.Upsert(tracker);
        //Modify the loading message to show the results
        await mod.ModifyAsync(t =>
        {
            t.Embeds = result.Embeds;
            t.Content = string.Format(result.Message, ctx.AuthorId);
        });
    }

    public async Task<GuildConfig> GetConfig(string? guildId)
    {
        var config = (string.IsNullOrEmpty(guildId) 
            ? null 
            : await _db.GuildConfig(guildId)) ?? new();
        var messages = _config.Messages;

        config.ChannelsWhitelist ??= [];
        config.ChannelsBlacklist ??= [];
        config.Emotes ??= [];
        config.MessageIdiots = config.MessageIdiots?.ForceNull() ?? messages.Idiots;
        config.MessageLoading = config.MessageLoading?.ForceNull() ?? messages.Loading;
        config.MessageDownloadFailed = config.MessageDownloadFailed?.ForceNull() ?? messages.DownloadFailed;
        config.MessageNoResults = config.MessageNoResults?.ForceNull() ?? messages.NoResults;
        config.MessageSucceeded = config.MessageSucceeded?.ForceNull() ?? messages.Succeed;
        config.MessageError = config.MessageError?.ForceNull() ?? messages.Error;

        if (config.Emotes.Length == 0) config.Emotes = _config.Emotes;

        return config;
    }

    public async Task<LookupContext?> GetContext(CacheMessage message, SocketReaction reaction)
    {
        bool IsTrigger(string[] emotes)
        {
            var comp = StringComparison.InvariantCultureIgnoreCase;
            //Check to see if any of the emotes match the config triggers
            if (emotes.Any(t => t.Equals(reaction.Emote.Name, comp)))
                return true;
            //Check to see if the server emote matches the config triggers
            if (reaction.Emote is Emote e)
            {
                var marked = $"<{(e.Animated ? "a" : "")}:{e.Name}:{e.Id}>";
                if (_config.Emotes.Any(t => t.Equals(marked, comp)))
                    return true;
            }
            //Not a valid emote
            return false;
        }

        //Ensure it was triggered in a guild
        if (reaction.Channel is not SocketGuildChannel channel)
            return null;
        //Ensure the user is specified on the reaction
        if (!reaction.User.IsSpecified)
            return null;
        //Ensure the reaction is not from a bot
        if (reaction.User.Value.IsBot)
            return null;
        //Get the guild ID from the channel
        string guildId = channel.Guild.Id.ToString();
        //Get the guild configuration 
        var config = await GetConfig(guildId);
        if (!config.EmotesEnabled || !IsTrigger(config.Emotes))
            return null;
        //Get or download the cached message
        var msg = await message.GetOrDownloadAsync();
        if (msg is null) return null;
        //Get the image URL
        var image = DetermineUrl(msg);
        //Pull out the IDs for various things
        string messageId = msg.Id.ToString(),
            channelId = channel.Id.ToString(),
            authorId = reaction.UserId.ToString();
        //Return the context
        return new(channel, guildId, channelId, authorId, messageId, image, msg, msg, config);
    }

    public async Task<LookupContext?> GetContext(SocketMessage message)
    {
        _botMentions ??=
        [
            $"<@{_client.CurrentUser.Id}>",
            $"<@!{_client.CurrentUser.Id}>"
        ];

        //Ensure it was triggered in a guild
        if (message.Channel is not SocketGuildChannel channel)
            return null;
        //Ensure the author isn't a bot
        if (message.Author.IsBot)
            return null;
        //Ensure the message mentions the bot
        if (!_botMentions.Any(message.Content.ContainsIc))
            return null;
        //Get the guild ID from the channel
        string guildId = channel.Guild.Id.ToString();
        //Get the guild configuration 
        var config = await GetConfig(guildId);
        if (!config.PingsEnabled)
            return null;
        //Pull out the IDs for various things
        string messageId = message.Id.ToString(),
            channelId = channel.Id.ToString(),
            authorId = message.Author.Id.ToString();
        //Check if the message contains an image url or attachment
        var image = DetermineUrl(message);
        //If there is an image, it's a direct request, so we can return the context
        if (!string.IsNullOrEmpty(image))
            return new(channel, guildId, channelId, authorId, messageId, image, message, message, config);
        //Check to see if the trigger message references another message
        if (message.Reference is null || !message.Reference.MessageId.IsSpecified)
            return null;
        //Get the referenced message by it's ID
        var msg = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
        if (msg is null) 
            return null;
        //Determine the image URL from the referenced message
        if (string.IsNullOrEmpty(image = DetermineUrl(msg))) 
            return null;
        //Set the message ID to the referenced message
        messageId = msg.Id.ToString();
        //Return the context
        return new(channel, guildId, channelId, authorId, messageId, image, msg, message, config);
    }
}