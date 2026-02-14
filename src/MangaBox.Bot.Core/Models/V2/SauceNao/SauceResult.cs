using CardboardBox.Json.InterfaceDeserializing;

namespace MangaBox.Bot.Core.Models.V2.SauceNao;

/// <summary>
/// The result returned from SauceNao
/// </summary>
[InterfaceOption("sauce-nao")]
public class SauceResult : IImageSearchResult
{
	/// <summary>
	/// The metadata for the result
	/// </summary>
	[JsonPropertyName("header")]
	public SauceMetaData MetaData { get; set; } = new();

	/// <summary>
	/// The data of the result
	/// </summary>
	[JsonPropertyName("data")]
	public SauceData Data { get; set; } = new();
}
