namespace MangaBox.Bot.Core.Models;

public interface ILookupConfig
{
    string Api { get; }

    string UserAgent { get; }

    string Title { get; }

    string JoinLink { get; }

    string[] Emotes { get; }
}

public class LookupConfig : ILookupConfig
{
    public const string DEFAULT_USERAGENT = "MangaBox-DiscordBot/1.0";
    public const string DEFAULT_TITLE = "MangaBox | Search";
    public const string DEFAULT_JOIN_LINK = "https://discord.com/oauth2/authorize?client_id=1324859527480475710&permissions=1126864127313984&integration_type=0&scope=applications.commands+bot";
    public static readonly string[] DEFAULT_EMOTES = ["🍝", "🔍", "🔎"];

    public required string Api { get; init; }

    public required string UserAgent { get; init; }

    public required string Title { get; init; }

    public required string[] Emotes { get; init; }

    public required string JoinLink { get; init; }

    public static string[] ArrayValues(IConfigurationSection section, string key, string[] defaults)
    {
        try
        {
            var strs = section.GetValue<string[]>(key);
            if (strs is not null && strs.Length > 0) return strs;
        }
        catch { }

        try
        {
            var value = section[key]?.ForceNull();
            if (string.IsNullOrEmpty(value)) return defaults;

            var split = value.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0) return split;
        }
        catch { }

        return defaults;
    }

    public static ILookupConfig GetConfig(IConfiguration config, string key = "Search")
    {
        var section = config.GetSection(key);
        var ua = section[nameof(UserAgent)].ForceNull() ?? DEFAULT_USERAGENT;
        var title = section[nameof(Title)].ForceNull() ?? DEFAULT_TITLE;
        var join = section[nameof(JoinLink)].ForceNull() ?? DEFAULT_JOIN_LINK;
        var api = section[nameof(Api)].ForceNull() 
            ?? throw new NullReferenceException("Config Missing for Search API Url");
        var emotes = ArrayValues(section, nameof(Emotes), DEFAULT_EMOTES);

        return new LookupConfig
        {
            Api = api,
            UserAgent = ua,
            Title = title,
            Emotes = emotes,
            JoinLink = join
        };
    }
}
