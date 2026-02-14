using System.ComponentModel.DataAnnotations;

namespace MangaBox.Bot.Core.Models.V2.Mb;

/// <summary>
/// Represents an attribute for a manga or chapter
/// </summary>
public class MbAttribute
{
	/// <summary>
	/// The name of the attribute
	/// </summary>
	[Required]
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The value of the attribute
	/// </summary>
	[Required]
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;
}
