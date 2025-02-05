namespace MangaBox.Bot.Core;

using Database;
using Models;

public static class DiExtensions
{
    /// <summary>
    /// Attempt to find the file at the given path in various root directories
    /// </summary>
    /// <param name="path">The path to find</param>
    /// <returns>The localized file path (or null if not found)</returns>
    public static string? FindFile(this string path)
    {
        if (File.Exists(path)) return path;

        var roots = new[]
        {
            AppDomain.CurrentDomain.BaseDirectory,
            AppDomain.CurrentDomain.RelativeSearchPath,
            AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
            "./"
        };

        foreach (var root in roots)
        {
            if (string.IsNullOrEmpty(root)) continue;

            var fullPath = Path.Combine(root, path);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    /// <summary>
    /// Ensures the given string is at most the given length
    /// </summary>
    /// <param name="str">The string to check</param>
    /// <param name="length">The max length of the string</param>
    /// <param name="append">The string to append if the length is trimmed</param>
    /// <returns>The ensured string</returns>
    public static string? EnsureLength(this string? str, int length, string append = "...")
    {
        if (string.IsNullOrEmpty(str) || 
            str.Length <= length) return str;

        return str[..(length - append.Length)] + append;
    }

    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration config)
    {
        var conString = config["Database:ConnectionString"] ?? "Data Source=database.db;";
        return services
            .AddSqlService(c => c
                .ConfigureTypes(t => t
                    .TypeHandler<Guid, GuidHandler>()
                    .PolyfillBooleanHandler()
                    .PolyfillDateTimeHandler()
                    .PolyfillGuidArrays()
                    .DefaultJsonHandler<string[]>(() => [])
                    .CamelCase()
                    .Entity<LookupRequest>()
                    .Entity<LookupInteraction>()
                    .Entity<GuildConfig>())
                .ConfigureGeneration(g => g
                    .WithCamelCaseChange())
                .AddSQLite(conString, init: t => t.DeployManifest()))
            .AddTransient<IQueryCacheService, QueryCacheService>()
            .AddTransient<ILookupDbService, LookupDbService>()
            .AddTransient<ISearchApiService, SearchApiService>()
            .AddTransient<IFileService, FileService>()
            .AddTransient<IEmoteService, EmoteService>()
            .AddSingleton(c =>
            {
                var config = c.GetRequiredService<IConfiguration>();
                return LookupConfig.GetConfig(config);
            })
            .AddSingleton(c =>
            {
                var config = c.GetRequiredService<IConfiguration>();
                return config.GetValue<DiscordConfig>("Discord") ?? new();
            });
    }
}
