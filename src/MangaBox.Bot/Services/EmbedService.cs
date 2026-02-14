namespace MangaBox.Bot.Services;

using Core.Models.V1;
using Core.Models.V2;
using Core.Models.V2.Mb;
using Core.Models.V2.SauceNao;

/// <summary>
/// A service for generating embeds from search results
/// </summary>
public interface IEmbedService
{
    /// <summary>
    /// Go through the search results to figure out the best thing to show on the embeds
    /// </summary>
    /// <param name="result">The image search results</param>
    /// <param name="image">The URL of the image that was searched</param>
    /// <returns>All of the embeds to send</returns>
    IEnumerable<Embed> GenerateEmbeds(SearchResult result, string? image);

	/// <summary>
	/// Go through the search results to figure out the best thing to show on the embeds
	/// </summary>
	/// <param name="results">The image search results</param>
	/// <param name="image">The URL of the image that was searched</param>
	/// <returns>All of the embeds to send</returns>
	IEnumerable<Embed> GenerateEmbeds(ImageSearchResult[] results, string? image);
}

internal class EmbedService(ILookupConfig _config) : IEmbedService
{
	#region V1
	/// <summary>
	/// Go through the search results to figure out the best thing to show on the embeds
	/// </summary>
	/// <param name="result">The image search results</param>
	/// <param name="image">The URL of the image that was searched</param>
	/// <returns>All of the embeds to send</returns>
	public IEnumerable<Embed> GenerateEmbeds(SearchResult result, string? image)
    {
        //Find the best match from MB-Match
        var fallback = result.Match
            .OrderByDescending(t => t.Score)
            .FirstOrDefault(t => t.Manga is not null && (t.Score > 90 || t.ExactMatch));

        //If MB-Match doesn't exist then find all of the results from the other sources
        if (fallback is null)
        {
            //Generate a list of all of the results from third party sources
            var full = SearchResults(result, image);
            if (full is not null) yield return full;
            //Generate an embed representing the best guess
            var bestGuess = BestGuess(result);
            if (bestGuess is not null) yield return bestGuess;
            //Skip generating the MB-Match embed
            yield break;
        }
        //Get the MB-Match embed
        var fbe = MbMatch(fallback);
        if (fbe is not null)
            yield return fbe;
    }

    /// <summary>
    /// The embed representing the best guess from the search results
    /// </summary>
    /// <param name="search">The search results</param>
    /// <returns>The embed if any</returns>
    /// <remarks>This is the last embed shown when using sources other than MB-Match</remarks>
    public Embed? BestGuess(SearchResult search)
    {
        if (search.BestGuess is null) return null;

        var hasMatch = search.Match.FirstOrDefault(t => t.Manga is not null && t.Manga.Url == search.BestGuess.Url);
        if (hasMatch is not null)
        {
            var fallback = MbMatch(hasMatch);
            if (fallback is not null) return fallback;
        }

        var embed = new EmbedBuilder()
            .WithTitle(search.BestGuess.Title.EnsureLength(EmbedBuilder.MaxTitleLength))
            .WithUrl(search.BestGuess.Url)
            .WithDescription(search.BestGuess.Description.EnsureLength(EmbedBuilder.MaxDescriptionLength))
            .WithThumbnailUrl(search.BestGuess.Cover)
            .AddField("Tags", string.Join(", ", search.BestGuess.Tags).EnsureLength(EmbedFieldBuilder.MaxFieldValueLength))
            .AddField("Source", $"[{search.BestGuess.Source}]({search.BestGuess.Url})", true)
            .WithFooter(_config.Title)
            .WithCurrentTimestamp();

        if (search.BestGuess.Nsfw)
            embed.AddField("NSFW", "yes", true);

        return embed.Build();
    }

    /// <summary>
    /// The embed for MB-Match search results
    /// </summary>
    /// <param name="result">The MB-Match result</param>
    /// <returns>The embed if any</returns>
    /// <remarks>This will always be sent alone</remarks>
    public Embed? MbMatch(SearchResult.MatchResult result)
    {
        if (result.Manga == null) return null;

        var embed = new EmbedBuilder()
            .WithTitle(result.Manga.Title.EnsureLength(EmbedBuilder.MaxTitleLength))
            .WithUrl(result.Manga.Url)
            .WithThumbnailUrl(result.Manga.Cover)
            .WithDescription(result.Manga.Description.EnsureLength(EmbedBuilder.MaxDescriptionLength))
            .AddField("Tags", string.Join(", ", result.Manga.Tags).EnsureLength(EmbedFieldBuilder.MaxFieldValueLength))
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

    /// <summary>
    /// The embed for the search results from all sources
    /// </summary>
    /// <param name="search">The search results</param>
    /// <param name="image">The image that was searched for</param>
    /// <returns>The embed if any</returns>
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

            header.AddField(
                title.EnsureLength(EmbedFieldBuilder.MaxFieldNameLength), 
                desc.EnsureLength(EmbedFieldBuilder.MaxFieldValueLength));
            count++;
        }

        return count == 0 ? null : header.Build();
    }
	#endregion

	#region V2
    /// <inheritdoc />
	public IEnumerable<Embed> GenerateEmbeds(ImageSearchResult[] results, string? image)
	{
		if (results.Length == 0) yield break;

		var exactMatch = results.FirstOrDefault(t => t.Exact && t.Closest is not null);
        if (exactMatch is not null)
        {
            yield return ExactMatch(exactMatch.Closest!);
            yield break;
        }

        var header = new EmbedBuilder()
			.WithTitle("Manga Search Results")
			.WithDescription("Here is what I found: ")
			.WithFooter(_config.Title)
			.WithCurrentTimestamp();

		if (!string.IsNullOrWhiteSpace(image))
			header.WithThumbnailUrl(image);

        int count = 0;
        foreach(var res in results.OrderByDescending(t => t.Score))
        {
            if (count >= 10) break;
            if (res.Result is null) continue;

            if (res.Result is VisionPage google)
                header.AddField("Google-Vision", $"[{google.Title}]({google.Url})".EnsureLength(EmbedFieldBuilder.MaxFieldValueLength));
            else if (res.Result is SauceResult sauce)
			{
				var url = sauce.Data?.ExternalUrls?.FirstOrDefault();
				if (string.IsNullOrEmpty(url))
					continue;

				var title = "";
                if (!string.IsNullOrEmpty(sauce.Data?.Title))
                    title += sauce.Data.Title + " ";
                title += sauce.MetaData.IndexName;

                header.AddField("SauceNAO", $"[{title}]({url}) - (Score: {res.Score}, EM: {res.Exact})".EnsureLength(EmbedFieldBuilder.MaxFieldValueLength));
            }
            else continue;

			count++;
		}

        if (count > 0)
            yield return header.Build();

        var top = results.Where(t => t.Closest is not null)
            .OrderByDescending(t => t.Score)
            .Take(3);

        foreach(var res in top)
            yield return ExactMatch(res.Closest!, 
                e => e.AddField("Score", $"{res.Score} (EM: {res.Exact})", true),
                res.Source);
	}

    public Embed ExactMatch(MangaBoxType<MbManga> manga, Action<EmbedBuilder>? config = null, string? via = null)
    {
        var title = manga.GetItem<MbMangaExt>()?.DisplayTitle ?? manga.Entity.Title;

        var cover = manga.GetItems<MbImage>().MaxBy(t => t.Ordinal)?.Id;
        var coverUrl = cover is not null ? $"https://v2.mangabox.app/image/{cover}" : null;
        var tags = string.Join(", ", manga.GetItems<MbTag>().Select(t => t.Name)).EnsureLength(EmbedFieldBuilder.MaxFieldValueLength);
        var source = manga.GetItem<MbSource>();

		var bob = new EmbedBuilder()
            .WithTitle(title.EnsureLength(EmbedBuilder.MaxTitleLength))
            .WithUrl(manga.Entity.Url)
            .WithDescription(manga.Entity.Description.EnsureLength(EmbedBuilder.MaxDescriptionLength / 4))
            .WithFooter(_config.Title)
            .WithCurrentTimestamp();
           
        if (!string.IsNullOrEmpty(coverUrl)) bob.WithThumbnailUrl(coverUrl);
        if (!string.IsNullOrEmpty(tags)) bob.AddField("Tags", tags);
        if (source is not null)
        {
            var name = source.Name;
            if (!string.IsNullOrEmpty(via))
                name += " (via " + via + ")";
            bob.AddField("Source", $"[{name}]({manga.Entity.Url})", true);
        }
        bob.AddField("Rating", manga.Entity.ContentRating.ToString(), true);

        if (config is not null) config(bob);
        else bob.AddField("Score", "Exact Match", true);

		return bob.Build();
    }
	#endregion
}
