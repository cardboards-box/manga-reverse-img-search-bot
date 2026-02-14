using CardboardBox.Json.InterfaceDeserializing;
using System.ComponentModel.DataAnnotations;

namespace MangaBox.Bot.Core.Models.V2.Mb;

/// <summary>
/// Represents all of the tags MangaBox supports
/// </summary>
[Table("mb_tags")]
[InterfaceOption(nameof(MbTag))]
public partial class MbTag : MbDbObject
{
	/// <summary>
	/// The slug of the chapter
	/// </summary>
	[Column("slug", Unique = true)]
	[JsonPropertyName("slug"), MinLength(1), Required]
	public string Slug { get; set; } = string.Empty;

	/// <summary>
	/// The name of the tag
	/// </summary>
	[Column("name")]
	[JsonPropertyName("name"), MinLength(1), Required]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The description of the tag
	/// </summary>
	[Column("description")]
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The source that originally loaded this tag
	/// </summary>
	[Column("source_id")]
	[JsonPropertyName("sourceId")]
	public Guid SourceId { get; set; }

}
