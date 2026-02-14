namespace MangaBox.Bot.Core.Models;

public interface ILookupConfig
{
    string Api { get; }

	string ApiV2 { get; }

	string UserAgent { get; }

    string Title { get; }

    string JoinLink { get; }

    string[] Emotes { get; }

    string[] AuthorizedUsers { get; }

    ILookupConfigMessages Messages { get; }
}

public interface ILookupConfigMessages
{
    string Idiots { get; }
    string Loading { get; }
    string DownloadFailed { get; }
    string NoResults { get; }
    string Succeed { get; }
    string Error { get; }
}

public class LookupConfig : ILookupConfig
{
    public const string DEFAULT_USERAGENT = "MangaBox-DiscordBot/1.0";
    public const string DEFAULT_TITLE = "MangaBox | Search";
    public const string DEFAULT_JOIN_LINK = "https://discord.com/oauth2/authorize?client_id=1324859527480475710&permissions=1126864127313984&integration_type=0&scope=applications.commands+bot";
    
    public const string DEFAULT_MESSAGE_IDIOTS = "Uh, <@{0}>, it's right here...";
    public const string DEFAULT_MESSAGE_LOADING = "<@{0}> <a:box_loading:1048471999065903244> Processing your request...";
    public const string DEFAULT_MESSAGE_DOWNLOAD_FAILED = "<@{0}> I couldn't download the image!";
    public const string DEFAULT_MESSAGE_NO_RESULTS = "<@{0}> I couldn't find any results that matched that image :(";
    public const string DEFAULT_MESSAGE_SUCCEED = "<@{0}> Here you go:";
    public const string DEFAULT_MESSAGE_ERROR = "An error occurred while looking up the image :(\r\nError Message: {0}";

    public static readonly string[] DEFAULT_EMOTES = ["🍝", "🔍", "🔎"];
    
    public required string Api { get; init; }

    public required string ApiV2 { get; init; }

    public required string UserAgent { get; init; }

    public required string Title { get; init; }

    public required string[] Emotes { get; init; }

    public required string[] AuthorizedUsers { get; init; }

    public required string JoinLink { get; init; }

    public required ILookupConfigMessages Messages { get; init; }

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
		var apiV2 = section[nameof(ApiV2)].ForceNull()
			?? throw new NullReferenceException("Config Missing for Search API V2 Url");
		var emotes = ArrayValues(section, nameof(Emotes), DEFAULT_EMOTES);
        var authorized = ArrayValues(section, nameof(AuthorizedUsers), []);
        var messages = LookupConfigMessage.GetConfig(section.GetSection(nameof(Messages)));

        return new LookupConfig
        {
            Api = api,
            ApiV2 = apiV2,
			UserAgent = ua,
            Title = title,
            Emotes = emotes,
            JoinLink = join,
            AuthorizedUsers = authorized,
            Messages = messages
        };
    }

    public record class LookupConfigMessage(
        string Idiots,
        string Loading,
        string DownloadFailed,
        string NoResults,
        string Succeed,
        string Error) : ILookupConfigMessages
    {
        public static ILookupConfigMessages GetConfig(IConfigurationSection section)
        {
            var idiots = section[nameof(Idiots)].ForceNull() ?? DEFAULT_MESSAGE_IDIOTS;
            var loading = section[nameof(Loading)].ForceNull() ?? DEFAULT_MESSAGE_LOADING;
            var downloadFail = section[nameof(DownloadFailed)].ForceNull() ?? DEFAULT_MESSAGE_DOWNLOAD_FAILED;
            var noResults = section[nameof(NoResults)].ForceNull() ?? DEFAULT_MESSAGE_NO_RESULTS;
            var succeed = section[nameof(Succeed)].ForceNull() ?? DEFAULT_MESSAGE_SUCCEED;
            var error = section[nameof(Error)].ForceNull() ?? DEFAULT_MESSAGE_ERROR;

            return new LookupConfigMessage(idiots, loading, downloadFail, noResults, succeed, error);
        }
    }
}
