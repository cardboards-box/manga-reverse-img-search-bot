using CardboardBox.Json.InterfaceDeserializing;

namespace MangaBox.Bot.Core.Models.V2.Mb;

/// <summary>
/// All of the source providers that MangaBox supports
/// </summary>
[Table("mb_sources")]
[InterfaceOption(nameof(MbSource))]
public class MbSource : MbDbObject
{
	/// <summary>
	/// The unique slug of the source
	/// </summary>
	[Column("slug", Unique = true)]
	[JsonPropertyName("slug")]
	public string Slug { get; set; } = string.Empty;

	/// <summary>
	/// The name of the source
	/// </summary>
	[Column("name")]
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The base URL to use for the source
	/// </summary>
	[Column("base_url")]
	[JsonPropertyName("baseUrl")]
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>
	/// Whether or not the source is hidden from the public
	/// </summary>
	[Column("is_hidden")]
	[JsonPropertyName("isHidden")]
	public bool IsHidden { get; set; } = false;

	/// <summary>
	/// Whether or not the source is enabled
	/// </summary>
	[Column("enabled")]
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; } = true;
}