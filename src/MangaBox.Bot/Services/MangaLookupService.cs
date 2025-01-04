namespace MangaBox.Bot.Services;

public interface IMangaLookupService
{
    string? DetermineUrl(string content);

    string? DetermineUrl(IMessage msg);

    Task<Lookup> Lookup(string url);
}

internal class MangaLookupService(
    IFileService _file,
    ISearchApiService _search,
    IEmbedService _embed,
    ILogger<MangaLookupService> _logger) : IMangaLookupService
{
    public async Task<Lookup> Lookup(string url)
    {
        try
        {
            var local = await _file.DownloadImage(url);
            if (local is null)
                return new(false, "I couldn't download the image!", []);

            var (io, filename) = local.Value;
            using var stream = io;
            var search = await _search.Search(io, filename);
            if (search is null)
                return new(false, "I couldn't find any results that matched that image :(", []);

            var embeds = _embed.GenerateEmbeds(search, url).ToArray();
            if (embeds.Length == 0)
                return new(false, "I couldn't find any results that matched that image :(", []);

            return new(true, "Here you go:", embeds, search);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while looking up image: {url}", url);
            return new(false, 
                "An error occurred while looking up the image :(\r\n" +
                "Error Message: " + ex.Message, []);
        }
    }

    public string? DetermineUrl(string content)
    {
        content = content.Trim();
        if (Uri.IsWellFormedUriString(content, UriKind.Absolute)) return content;

        var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        foreach (Match m in linkParser.Matches(content))
        {
            if (Uri.IsWellFormedUriString(m.Value, UriKind.Absolute))
                return m.Value;
        }

        return null;
    }

    public string? DetermineUrl(IMessage msg)
    {
        var img = msg.Attachments.FirstOrDefault(t => t.ContentType
            .StartsWith("image", StringComparison.InvariantCultureIgnoreCase));
        if (img != null) return img.Url;

        var content = msg.Content;
        return DetermineUrl(content);
    }
}

public record class Lookup(
    bool Worked,
    string Message,
    Embed[] Embeds,
    SearchResult? Result = null);
