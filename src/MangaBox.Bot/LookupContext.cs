namespace MangaBox.Bot;

/// <summary>
/// The context in which a lookup was requested
/// </summary>
/// <param name="Channel">The channel the request was sent in</param>
/// <param name="GuildId">The ID of the guild the request was sent in</param>
/// <param name="ChannelId">The ID of the channel the request was sent in</param>
/// <param name="AuthorId">The ID of the author who sent the request</param>
/// <param name="MessageId">The ID of the message that has the image URL</param>
/// <param name="ImageUrl">The URL of the image that was found (null if no image was found)</param>
/// <param name="SourceMessage">The message the image was found in</param>
/// <param name="InteractedMessage">The message that was interacted with</param>
/// <remarks>
/// If the image was found in the same message that triggered the bot then <paramref name="InteractedMessage"/> will be the same as <paramref name="SourceMessage"/>
/// </remarks>
public record class LookupContext(
    SocketGuildChannel Channel,
    string GuildId,
    string ChannelId,
    string AuthorId,
    string MessageId,
    string? ImageUrl,
    IMessage SourceMessage,
    IMessage InteractedMessage)
{
    /// <summary>
    /// The reference to the message that triggered the request
    /// </summary>
    public MessageReference ReplyingTo => new(InteractedMessage.Id, Channel.Id, Channel.Guild.Id);
}
