using CardboardBox.Json.InterfaceDeserializing;

namespace MangaBox.Bot.Core.Models.V2;

/// <summary>
/// A image url and it's score
/// </summary>
/// <param name="Score">The match score</param>
/// <param name="Url">The image url</param>
public record class VisionItem(
	[property: JsonPropertyName("score")] float Score,
	[property: JsonPropertyName("url")] string Url);

/// <summary>
/// Represents a matching web page
/// </summary>
/// <param name="FullMatches">Any fully matching images</param>
/// <param name="PartialMatches">Any partially matching images</param>
/// <param name="Url">The page URL</param>
/// <param name="Score">The score</param>
/// <param name="Title">The page title</param>
/// <param name="PurgeTitle">The purged page title</param>
[InterfaceOption("google-vision")]
public record class VisionPage(
	[property: JsonPropertyName("fullMatches")] VisionItem[] FullMatches,
	[property: JsonPropertyName("partialMatches")] VisionItem[] PartialMatches,
	[property: JsonPropertyName("url")] string Url,
	[property: JsonPropertyName("score")] float Score,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("purgeTitle")] string PurgeTitle) : IImageSearchResult;