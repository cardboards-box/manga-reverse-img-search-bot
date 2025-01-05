namespace MangaBox.Bot.Core.Models;

/// <summary>
/// The configuration for each guild for the bot
/// </summary>
[Table("guild_configs")]
public class GuildConfig : DbObject
{
    /// <summary>
    /// The ID of the guild
    /// </summary>
    [Column("guild_id", Unique = true)]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// The message to send when someone has requested something that's already been looked up
    /// </summary>
    [Column("message_idiots")]
    public string? MessageIdiots { get; set; }

    /// <summary>
    /// The message to send when a lookup request is loading
    /// </summary>
    [Column("message_loading")]
    public string? MessageLoading { get; set; }

    /// <summary>
    /// The channels for the white or black list
    /// </summary>
    [Column("channels")]
    public string[] Channels { get; set; } = [];

    /// <summary>
    /// What to do with the <see cref="Channels"/> list
    /// </summary>
    [Column("channels_type")]
    public ChannelType ChannelsType { get; set; } = ChannelType.None;

    /// <summary>
    /// The emotes that can be reacted with to request a lookup
    /// </summary>
    [Column("emotes")]
    public string[] Emotes { get; set; } = [];

    /// <summary>
    /// Whether or not to allow lookups via reactions
    /// </summary>
    [Column("emotes_enabled")]
    public bool EmotesEnabled { get; set; }

    /// <summary>
    /// Whether or not to allow lookups via pinging the bot
    /// </summary>
    [Column("pings_enabled")]
    public bool PingsEnabled { get; set; }
}

public enum ChannelType
{
    None = 0,
    WhiteList = 1,
    BlackList = 2,
}