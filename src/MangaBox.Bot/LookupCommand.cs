namespace MangaBox.Bot;

using Services;

internal class MessageTypeOptionAttribute() : OptionAttribute(
    "Message To Change", true,
    ("Duplicates - Sent on duplicate lookup requests", IDIOTS),
    ("Loading - Sent to indicate the bot is loading", LOADING),
    ("Download Failed - Sent when the bot fails to download the image", DOWNLOAD_FAILED),
    ("No Results - Sent when the bot fails to find any results", NO_RESULTS),
    ("Succeeded - Sent when the bot successfully finds results", SUCCEEDED),
    ("Error - Sent when an error occurs during lookup", ERROR))
{
    public const string IDIOTS = nameof(GuildConfig.MessageIdiots);
    public const string LOADING = nameof(GuildConfig.MessageLoading);
    public const string DOWNLOAD_FAILED = nameof(GuildConfig.MessageDownloadFailed);
    public const string NO_RESULTS = nameof(GuildConfig.MessageNoResults);
    public const string SUCCEEDED = nameof(GuildConfig.MessageSucceeded);
    public const string ERROR = nameof(GuildConfig.MessageError);
}

internal class ChannelOptionAttribute : OptionAttribute
{
    public ChannelOptionAttribute() : base("Channel in question", true)
    {
        Type = ApplicationCommandOptionType.Channel;
    }
}

internal class LookupCommand(
    IMangaLookupService _lookup,
    ILookupConfig _config,
    ILookupDbService _db,
    IEmoteService _emotes)
{
    public const string COMMAND_SEARCH = "manga-search";
    public const string COMMAND_SEARCH_PRIVATE = "manga-search-private";

    public const string ACTION_STATE = "State";
    public const string ACTION_CLEAR = "Clear";
    public const string ACTION_BLACK = "Blacklist";
    public const string ACTION_WHITE = "Whitelist";

    [Command(COMMAND_SEARCH, "Search for a manga by using an image.", LongRunning = true)]
    public async Task Lookup(SocketSlashCommand cmd, [Option("Image URL", true)] string url)
    {
        var config = await _lookup.GetConfig(cmd.GuildId?.ToString());
        var result = await _lookup.Lookup(url, config);
        await cmd.ModifyOriginalResponseAsync(t =>
        {
            t.Content = result.Message;
            if (result.Embeds.Length > 0)
                t.Embeds = result.Embeds;
        });
    }

    [Command(COMMAND_SEARCH_PRIVATE, "Search for a manga by using an image (but it's only for you).", 
        LongRunning = true, Ephemeral = true)]
    public Task LookupPrivate(SocketSlashCommand cmd, [Option("Image URL", true)] string url)
    {
        return Lookup(cmd, url);
    }

    [Command("about", "Displays some information about the bot")]
    public async Task About(SocketSlashCommand cmd)
    {
        var config = await _lookup.GetConfig(cmd.GuildId?.ToString());
        var bob = new StringBuilder();
        bob.AppendLine($@"# {_config.Title}");
        bob.AppendLine("Is some heathen not posting the sauce of that interesting manga? Well I can help with that!");
        bob.AppendLine("I can use my reverse image search database to lookup the manga page and try to find the source.");
        bob.AppendLine("There are various ways you can trigger page lookups:");

        if (config.EmotesEnabled && cmd.GuildId is not null)
            bob.AppendLine($@"* React to an image with one of these emotes: {string.Join(" ", config.Emotes)}");

        if (config.PingsEnabled && cmd.GuildId is not null)
        {
            bob.AppendLine("* Mention me while replying to a message that has an image in it");
            bob.AppendLine("* Mention me in a message with the image in it");
        }

        bob.AppendLine($"* Use the `/{COMMAND_SEARCH}` slash command");
        bob.AppendLine($"* Use the `/{COMMAND_SEARCH_PRIVATE}` slash command (for more risky images)");
        bob.AppendLine($@"You can also use [the website](<https://mangabox.app>) to do lookups!
Want me in your server? You can use [this link]({_config.JoinLink}).
Want to host your own instance of me? You can check out the [source code](<https://github.com/cardboards-box/manga-reverse-img-search-bot>).

Some known issues:
* Cropped images and long-strip content don't work very well
* Only English manga are indexed
* Sites other than MangaDex are iffy at best.
* MangaDex content before Dec 2022 has not been indexed.

If you have comments, concerns, or questions, you can reach out [via this forum post](<https://forums.mangadex.org/threads/manga-reverse-image-lookup-service.1146452/>)");
        await cmd.RespondAsync(bob.ToString());
    }

    [Command("manga-config-responses", "Configures the response messages for the bot", 
        Ephemeral = true, LongRunning = true)]
    public async Task Config(SocketSlashCommand cmd,
        [MessageTypeOption] string message,
        [Option("What to say (blank will reset)", false)] string? value)
    {
        if (cmd.GuildId is null)
        {
            await cmd.Modify("This is only valid in discord servers!");
            return;
        }

        if (!_lookup.IsAdmin(cmd))
        {
            await cmd.Modify("You don't have permission to do that!");
            return;
        }

        var prop = typeof(GuildConfig).GetProperty(message);
        if (prop is null)
        {
            await cmd.Modify("Invalid message type");
            return;
        }

        var config = await _db.GuildConfig(cmd.GuildId.Value) 
            ?? new GuildConfig { GuildId = cmd.GuildId.Value.ToString() };
        prop.SetValue(config, value.ForceNull());
        await _db.Upsert(config);
        await cmd.Modify("Config updated!");
    }

    [Command("manga-config-channels", "Configures the black/white list for channels that can be used for looking up manga", 
        Ephemeral = true, LongRunning = true)]
    public async Task Channels(SocketSlashCommand cmd,
        [Option("What to do", true, ACTION_WHITE, ACTION_BLACK, ACTION_CLEAR, ACTION_STATE)] string action,
        [ChannelOption] IChannel channel)
    {
        var channelId = channel.Id.ToString();
        if (cmd.GuildId is null)
        {
            await cmd.Modify("This is only valid in discord servers!");
            return;
        }

        if (channel is not SocketTextChannel txt)
        {
            await cmd.Modify("Selected channel needs to be a text channel");
            return;
        }

        if (!_lookup.IsAdmin(cmd))
        {
            await cmd.Modify("You don't have permission to do that!");
            return;
        }

        var config = await _db.GuildConfig(cmd.GuildId.Value)
            ?? new GuildConfig { GuildId = cmd.GuildId.Value.ToString() };
        if (action == ACTION_STATE)
        {
            var isWhite = config.ChannelsWhitelist.Contains(channelId);
            var isBlack = config.ChannelsBlacklist.Contains(channelId);
            string state;
            if (isWhite && isBlack) state = "Somehow on both the black and whitelists? What? How'd that happen?";
            else if (isWhite) state = "Whitelisted.";
            else if (isBlack) state = "Blacklisted.";
            else state = "Not listed.";
            await cmd.Modify($"{txt.Mention} is {state}");
            return;
        }

        if (action == ACTION_CLEAR)
        {
            config.ChannelsBlacklist = config.ChannelsBlacklist.Where(t => t != channelId).ToArray();
            config.ChannelsWhitelist = config.ChannelsWhitelist.Where(t => t != channelId).ToArray();
            await _db.Upsert(config);
            await cmd.Modify($"{txt.Mention} has been cleared from the black/white list.");
            return;
        }

        if (action == ACTION_WHITE)
        {
            config.ChannelsBlacklist = config.ChannelsBlacklist
                .Where(t => t != channelId)
                .ToArray();
            config.ChannelsWhitelist = config.ChannelsWhitelist
                .Where(t => t != channelId)
                .Append(channelId)
                .ToArray();
            await _db.Upsert(config);
            await cmd.Modify($"{txt.Mention} has been whitelisted.");
            return;
        }

        if (action == ACTION_BLACK)
        {
            config.ChannelsBlacklist = config.ChannelsBlacklist
                .Where(t => t != channelId)
                .Append(channelId)
                .ToArray();
            config.ChannelsWhitelist = config.ChannelsWhitelist
                .Where(t => t != channelId)
                .ToArray();
            await _db.Upsert(config);
            await cmd.Modify($"{txt.Mention} has been blacklisted.");
            return;
        }

        await cmd.Modify($"Unknown action {action}");
    }

    [Command("manga-config", "Configure general settings for manga lookups", Ephemeral = true, LongRunning = true)]
    public async Task Config(SocketSlashCommand cmd,
        [Option("Reaction Lookups", false, "Enabled", "Disable")] string? reactions,
        [Option("Ping Lookups", false, "Enabled", "Disable")] string? pings,
        [Option("Reaction Emotes (space separated - `reset` to reset)", false)] string? emotes)
    {
        static string Channels(string[] listing)
        {
            if (listing.Length == 0) return "None";

            return string.Join(" ", listing.Select(t => $"<#{t}>"));
        }

        if (cmd.GuildId is null)
        {
            await cmd.Modify("This is only valid in discord servers!");
            return;
        }

        if (!_lookup.IsAdmin(cmd))
        {
            await cmd.Modify("You don't have permission to do that!");
            return;
        }

        var config = await _db.GuildConfig(cmd.GuildId.Value)
            ?? new GuildConfig { GuildId = cmd.GuildId.Value.ToString() };

        if (!string.IsNullOrWhiteSpace(reactions)) config.EmotesEnabled = reactions == "Enabled";
        if (!string.IsNullOrWhiteSpace(pings)) config.PingsEnabled = pings == "Enabled";

        if (!string.IsNullOrWhiteSpace(emotes) && emotes.Trim().EqualsIc("reset"))
        {
            config.Emotes = [];
            emotes = null;
        }

        if (!string.IsNullOrWhiteSpace(emotes))
        {
            var parts = emotes.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (_emotes.IsEmote(part) ||
                    _emotes.IsEmoji(part)) continue;
                
                await cmd.Modify($"Invalid emote `{part}`");
                return;
            }
            config.Emotes = parts;
        }

        await _db.Upsert(config);

        config = await _lookup.GetConfig(cmd.GuildId.ToString());

        string channels = "all channels the bot can see";
        if (config.ChannelsWhitelist.Length > 0)
            channels = "only these channels: " + Channels(config.ChannelsWhitelist);
        else if (config.ChannelsBlacklist.Length > 0)
            channels = "all channels except these: " + Channels(config.ChannelsBlacklist);

        await cmd.Modify(@$"{_config.Title} Configuration:
Reaction lookups are {(config.EmotesEnabled ? "enabled" : "disabled")}.
Bot ping lookups are {(config.PingsEnabled ? "enabled" : "disabled")}.
The bot will react to these emotes: {string.Join(" ", config.Emotes)}.
Lookups can be done in {channels} (does not effect slash commands).

Here are the response messages:
__**Duplicate lookups**__: ```
{config.MessageIdiots}
```
__**Loading message**__: ```
{config.MessageLoading}
```
__**Image download failed**__: ```
{config.MessageDownloadFailed}
```
__**No results found**__: ```
{config.MessageNoResults}
```
__**Results found**__: ```
{config.MessageSucceeded}
```
__**Error Occurred**__: ```
{config.MessageError}
```");
    }
}
