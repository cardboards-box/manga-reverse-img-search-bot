namespace MangaBox.Bot.Core;

public interface IFileService
{
    string Encode(string url);

    string GetUserAgent(string url);

    string GetFileName(string? file, string imageUrl);

    Task<(MemoryStream stream, string filename)?> DownloadImage(string url, bool throwErrors = false);
}

internal class FileService(
    IApiService _api,
    ILookupConfig _config,
    ILogger<FileService> _logger) : IFileService
{
    public string Encode(string url) => WebUtility.UrlEncode(url);

    public string GetUserAgent(string url)
    {
        if (url.Contains("mangadex", StringComparison.InvariantCultureIgnoreCase))
            return _config.UserAgent;

        return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 OPR/106.0.0.0";
    }

    public string GetFileName(string? file, string imageUrl)
    {
        if (!string.IsNullOrEmpty(file)) return file;

        var uri = new Uri(imageUrl);
        return Path.GetFileName(uri.LocalPath);
    }

    public async Task<(MemoryStream stream, string filename)?> DownloadImage(string url, bool throwErrors = false)
    {
        var io = new MemoryStream();
        string? filename = null;

        try
        {
            var (stream, _, file, type) = await _api.GetData(url, 
                userAgent: GetUserAgent(url));
            await stream.CopyToAsync(io);
            io.Position = 0;
            filename = GetFileName(file, url);
            return (io, filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during image download: {imgUrl}", url);
            if (throwErrors) throw;
            return null;
        }
    }
}
