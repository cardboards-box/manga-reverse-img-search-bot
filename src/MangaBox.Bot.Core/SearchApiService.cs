using CardboardBox.Extensions.Requesting;

namespace MangaBox.Bot.Core;

using Models.V1;
using Models.V2;

public interface ISearchApiService
{
    Task<SearchResult?> Search(string url);

    Task<SearchResult?> Search(Stream stream, string filename);

    Task<BoxedArray<ImageSearchResult>?> SearchV2(string url);

    Task<BoxedArray<ImageSearchResult>?> SearchV2(Stream stream, string filename);
}

public class SearchApiService(
    ILookupConfig _config,
    IApiService _api) : ISearchApiService
{
    public string MarryUrl(string url, bool v1, params (string, object?)[] parameters)
    {
        var pars = string.Join("&", parameters
            .Where(t => t.Item2 is not null)
            .Select(p => $"{p.Item1}={WebUtility.UrlEncode(p.Item2!.ToString())}"));

        if (url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            return $"{url.TrimEnd('?')}?{pars}";

        return $"{(v1 ? _config.Api : _config.ApiV2)}/{url.TrimStart('/').TrimEnd('?')}?{pars}";
    }

    public Task<SearchResult?> Search(string url)
    {
        var uri = WebUtility.UrlEncode(url);
        return _api.Get<SearchResult>(MarryUrl("manga/image-search", true, ("path", url)));
    }

    public async Task<SearchResult?> Search(Stream stream, string filename)
    {
        using var content = new StreamContent(stream);
        using var form = new MultipartFormDataContent
        {
            { content, "file", filename }
        };
        var url = MarryUrl("manga/image-search", true);
        return await _api.Create(url, "POST")
            .BodyContent(form)
            .Result<SearchResult>();
    }

	public Task<BoxedArray<ImageSearchResult>?> SearchV2(string url)
	{
		var uri = WebUtility.UrlEncode(url);
        return _api.Get<BoxedArray<ImageSearchResult>>(MarryUrl("reverse-search", false, ("path", url)));
	}

	public async Task<BoxedArray<ImageSearchResult>?> SearchV2(Stream stream, string filename)
	{
		using var content = new StreamContent(stream);
		using var form = new MultipartFormDataContent
		{
			{ content, "file", filename }
		};
		var url = MarryUrl("reverse-search", false);
		return await _api.Create(url, "POST")
			.BodyContent(form)
			.Result<BoxedArray<ImageSearchResult>>();
	}
}
