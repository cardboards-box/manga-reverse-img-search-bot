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
    /// The message to send when the bot fails to download the image
    /// </summary>
    [Column("message_download_failed")]
    public string? MessageDownloadFailed { get; set; }

    /// <summary>
    /// The message to send when the bot fails to find any results
    /// </summary>
    [Column("message_no_results")]
    public string? MessageNoResults { get; set; }

    /// <summary>
    /// The message to send when the bot successfully finds results
    /// </summary>
    [Column("message_succeeded")]
    public string? MessageSucceeded { get; set; }

    /// <summary>
    /// The message to send when an error occurs during lookup
    /// </summary>
    [Column("message_error")]
    public string? MessageError { get; set; }

    /// <summary>
    /// All of the channels that are whitelisted for lookups
    /// </summary>
    /// <remarks>If empty, all channels will be whitelisted by default</remarks>
    [Column("channels_whitelist")]
    public string[] ChannelsWhitelist { get; set; } = [];

    /// <summary>
    /// All of the channels that are blacklisted for lookups
    /// </summary>
    /// <remarks>If empty, no channels will be blacklisted</remarks>
    [Column("channels_blacklist")]
    public string[] ChannelsBlacklist { get; set; } = [];

    /// <summary>
    /// The emotes that can be reacted with to request a lookup
    /// </summary>
    [Column("emotes")]
    public string[] Emotes { get; set; } = [];

    /// <summary>
    /// Whether or not to allow lookups via reactions
    /// </summary>
    [Column("emotes_enabled")]
    public bool EmotesEnabled { get; set; } = true;

    /// <summary>
    /// Whether or not to allow lookups via pinging the bot
    /// </summary>
    [Column("pings_enabled")]
    public bool PingsEnabled { get; set; } = true;
}