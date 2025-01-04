namespace MangaBox.Bot.Services;

public interface IEmbedService
{
    IEnumerable<Embed> GenerateEmbeds(SearchResult result, string? image);
}

internal class EmbedService(ILookupConfig _config) : IEmbedService
{
    public IEnumerable<Embed> GenerateEmbeds(SearchResult result, string? image)
    {
        var fallback = result.Match
            .OrderByDescending(t => t.Score)
            .FirstOrDefault(t => t.Manga is not null && (t.Score > 90 || t.ExactMatch));

        if (fallback is null)
        {
            var full = SearchResults(result, image);
            if (full is not null) yield return full;

            var bestGuess = BestGuess(result);
            if (bestGuess is not null) yield return bestGuess;

            yield break;
        }

        var fbe = MbMatch(fallback);
        if (fbe is not null)
            yield return fbe;
    }

    public Embed? BestGuess(SearchResult search)
    {
        if (search.BestGuess is null) return null;

        var embed = new EmbedBuilder()
            .WithTitle(search.BestGuess.Title)
            .WithUrl(search.BestGuess.Url)
            .WithDescription($"{search.BestGuess.Description}.")
            .WithThumbnailUrl(search.BestGuess.Cover)
            .AddField("Tags", string.Join(", ", search.BestGuess.Tags))
            .AddField("Source", $"[{search.BestGuess.Source}]({search.BestGuess.Url})", true)
            .WithFooter(_config.Title)
            .WithCurrentTimestamp();

        if (search.BestGuess.Nsfw)
            embed.AddField("NSFW", "yes", true);

        return embed.Build();
    }

    public Embed? MbMatch(SearchResult.MatchResult result)
    {
        if (result.Manga == null) return null;

        var embed = new EmbedBuilder()
            .WithTitle(result.Manga.Title)
            .WithUrl(result.Manga.Url)
            .WithThumbnailUrl(result.Manga.Cover)
            .WithDescription($"{result.Manga.Description}")
            .AddField("Tags", string.Join(", ", result.Manga.Tags))
            .AddField("Score", $"{result.Score:0.00}. (EM: {result.ExactMatch})", true)
            .WithFooter(_config.Title);

        if (result.Manga.Nsfw)
            embed.AddField("NSFW", "yes", true);

        if (result.MetaData != null)
        {
            embed.AddField("Source", $"[{result.MetaData.Source}]({result.MetaData.Url})", true);

            if (result.MetaData.Type == 0 && 
                result.MetaData.Source.Equals("mangadex", StringComparison.InvariantCultureIgnoreCase))
                embed.AddField("Type", $"[Page](https://mangadex.org/chapter/{result.MetaData.ChapterId}/{result.MetaData.Page})", true);

            if (result.MetaData.Type == 1)
                embed.AddField("Type", "Cover", true);
        }

        return embed.Build();
    }

    public Embed? SearchResults(SearchResult search, string? image)
    {
        IEnumerable<(string title, string desc, double score)> GetResults()
        {
            foreach (var res in search.Match)
            {
                if (res.Manga == null || res.MetaData == null || res.Score < 70) continue;

                yield return (
                    res.Manga.Title,
                    $"MB-Match: [Mangadex]({res.Manga.Url}) - (MS: {res.Score:0.00}, EM: {res.ExactMatch})",
                    res.Score);
            }

            foreach (var res in search.Vision)
            {
                yield return (
                    res.Title ?? res.Manga?.Title ?? "Google Result",
                    $"Google Result: [{res.FilteredTitle}]({res.Url}) - (MS: {res.Score:0.00}, EM: {res.ExactMatch})",
                    res.Score);
            }

            foreach (var res in search.Textual)
            {
                if (res.Source != SearchResult.SOURCE_SAUCENAO) continue;

                yield return (
                    res.Manga?.Title ?? "Textual Result",
                    $"Textual Result: [SauceNAO]({res.Manga?.Url}) - (MS: {res.Score:0.00}, EM: {res.ExactMatch})",
                    res.Score * 100);
            }
        }

        var header = new EmbedBuilder()
            .WithTitle("Manga Search Results")
            .WithDescription("Here is what I found: ")
            .WithFooter(_config.Title)
            .WithCurrentTimestamp();

        if (!string.IsNullOrWhiteSpace(image))
            header.WithThumbnailUrl(image);

        var results = GetResults()
            .OrderByDescending(t => t.score);

        int count = 0;
        foreach(var (title, desc, score) in results)
        {
            if (count >= 5) break;

            header.AddField(title, desc);
            count++;
        }

        return count == 0 ? null : header.Build();
    }
}
