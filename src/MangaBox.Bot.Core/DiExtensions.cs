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
