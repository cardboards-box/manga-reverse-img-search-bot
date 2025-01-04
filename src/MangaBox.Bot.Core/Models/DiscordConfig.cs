namespace MangaBox.Bot.Core.Models;

public class DiscordConfig
{
    public bool Local { get; set; } = false;

    public string? JoinLink { get; set; }

    public string[] Intents { get; set; } = [];

    public GatewayIntents ParseIntents()
    {
        if (Intents.Length == 0)
            return GatewayIntents.AllUnprivileged;

        var result = GatewayIntents.None;

        foreach (var intent in Intents)
        {
            if (Enum.TryParse<GatewayIntents>(intent, true, out var parsed))
                result |= parsed;
        }

        return result;
    }
}
