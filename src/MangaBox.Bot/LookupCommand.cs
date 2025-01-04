namespace MangaBox.Bot;

using Services;

internal class LookupCommand(
    IMangaLookupService _lookup,
    ILookupConfig _config)
{
    public const string COMMAND_SEARCH = "manga-search";
    public const string COMMAND_SEARCH_PRIVATE = "manga-search-private";

    [Command(COMMAND_SEARCH, "Search for a manga by using an image.", LongRunning = true)]
    public async Task Lookup(SocketSlashCommand cmd, [Option("Image URL", true)] string url)
    {
        var result = await _lookup.Lookup(url);
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
        var message = $@"# {_config.Title}
Is some heathen not posting the sauce of that interesting manga? Well I can help with that!
I can use my reverse image search database to lookup the manga page and try to find the source.
There are various ways you can trigger my functions:
* React to an image with one of these emotes: {string.Join(" ", _config.Emotes)}
* Mention me while replying to a message that has an image in it
* Mention me in a message with the image in it
* Use the `/{COMMAND_SEARCH}` slash command (if enabled)
* Use the `/{COMMAND_SEARCH_PRIVATE}` slash command (for more risky images)
You can also use [the website](<https://mangabox.app>) to do lookups!
Want me in your server? You can use [this link]({_config.JoinLink}).
Want to host your own instance of me? You can check out the [source code](<https://github.com/cardboards-box/manga-reverse-img-search-bot>).

Some known issues:
* Cropped images and long-strip content don't work very well
* Only English manga are indexed
* Sites other than MangaDex are iffy at best.
* MangaDex content before Dec 2022 has not been indexed.

If you have comments, concerns, or questions, you can reach out [via this forum post](<https://forums.mangadex.org/threads/manga-reverse-image-lookup-service.1146452/>)";
        await cmd.RespondAsync(message);
    }
}
