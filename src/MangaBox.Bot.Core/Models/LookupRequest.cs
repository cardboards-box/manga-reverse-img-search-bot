namespace MangaBox.Bot.Core.Models;

[Table("lookup_requests")]
public class LookupRequest : DbObject
{
    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("message_id", Unique = true)]
    public string MessageId { get; set; } = string.Empty;

    [Column("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [Column("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    [Column("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [Column("response_id")]
    public string ResponseId { get; set; } = string.Empty;

    [Column("results")]
    public string? Results { get; set; }
}
