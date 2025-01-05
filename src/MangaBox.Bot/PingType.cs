namespace MangaBox.Bot;

/// <summary>
/// How we want to ping a user within a message
/// </summary>
public enum PingType
{
    /// <summary>
    /// None of the users should be pinged
    /// </summary>
    None = 0,
    /// <summary>
    /// We just want to ping the user who is being replied to and not any mentioned users
    /// </summary>
    JustReply = 1,
    /// <summary>
    /// We just want to ping the users who are mentioned in the message and not the user we're replying to
    /// </summary>
    JustMention = 2,
    /// <summary>
    /// We want to ping both the user who is being replied to and the mentioned users
    /// </summary>
    All = 3,
}
