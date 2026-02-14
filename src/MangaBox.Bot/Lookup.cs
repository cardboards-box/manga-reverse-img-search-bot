namespace MangaBox.Bot;

using Core.Models.V1;
using Core.Models.V2;

/// <summary>
/// Represents the result of a lookup
/// </summary>
/// <param name="Worked">Whether or not we found a result</param>
/// <param name="Message">The message to send to the user</param>
/// <param name="Embeds">The embeds representing the results</param>
/// <param name="Result">The search results that were found</param>
/// <param name="ResultV2">The V2 search results that were found</param>
public record class Lookup(
    bool Worked,
    string Message,
    Embed[] Embeds,
    SearchResult? Result = null,
    ImageSearchResult[]? ResultV2 = null);
