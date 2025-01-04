namespace MangaBox.Bot.Core;

public interface ISearchApiService
{
    Task<SearchResult?> Search(string url);

    Task<SearchResult?> Search(Stream stream, string filename);
}

public class SearchApiService(
    ILookupConfig _config,
    IApiService _api) : ISearchApiService
{
    private string? _apiUrl;

    public string MarryUrl(string url, params (string, object?)[] parameters)
    {
        var pars = string.Join("&", parameters
            .Where(t => t.Item2 is not null)
            .Select(p => $"{p.Item1}={WebUtility.UrlEncode(p.Item2!.ToString())}"));

        if (url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            return $"{url.TrimEnd('?')}?{pars}";

        return $"{_config.Api}/{url.TrimStart('/').TrimEnd('?')}?{pars}";
    }

    public Task<SearchResult?> Search(string url)
    {
        var uri = WebUtility.UrlEncode(url);
        return _api.Get<SearchResult>(MarryUrl("manga/image-search", ("path", url)));
    }

    public async Task<SearchResult?> Search(Stream stream, string filename)
    {
        using var content = new StreamContent(stream);
        using var form = new MultipartFormDataContent
        {
            { content, "file", filename }
        };
        var url = MarryUrl("manga/image-search");
        return await _api.Create(url, "POST")
            .BodyContent(form)
            .Result<SearchResult>();
    }
}
