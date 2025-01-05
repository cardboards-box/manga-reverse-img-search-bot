namespace MangaBox.Bot;

/// <summary>
/// Represents the result of a lookup
/// </summary>
/// <param name="Worked">Whether or not we found a result</param>
/// <param name="Message">The message to send to the user</param>
/// <param name="Embeds">The embeds representing the results</param>
/// <param name="Result">The search results that were found</param>
public record class Lookup(
    bool Worked,
    string Message,
    Embed[] Embeds,
    SearchResult? Result = null);
