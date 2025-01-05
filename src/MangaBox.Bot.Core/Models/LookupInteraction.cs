namespace MangaBox.Bot.Core.Models;

/// <summary>
/// Table indicating that a user has interacted with a lookup request before
/// </summary>
[Table("lookup_interactions")]
public class LookupInteraction : DbObject
{
    /// <summary>
    /// The ID of the message that has the image URL
    /// </summary>
    [Column("message_id", Unique = true)]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who interacted with the message
    /// </summary>
    [Column("user_id", Unique = true)]
    public string UserId { get; set; } = string.Empty;
}
