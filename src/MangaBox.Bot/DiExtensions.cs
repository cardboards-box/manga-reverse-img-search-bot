namespace MangaBox.Bot;

using Services;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class DiExtensions
{
    /// <summary>
    /// Adds the services for the MangaBox bot
    /// </summary>
    /// <param name="services">The service collection to inject into</param>
    /// <returns></returns>
    public static IServiceCollection AddMangaBox(this IServiceCollection services)
    {
        return services
            .AddTransient<IEmbedService, EmbedService>()
            .AddTransient<IMangaLookupService, MangaLookupService>()
            .AddTransient<ILookupHooks, LookupHooks>();
    }

    /// <summary>
    /// Adds the commands for the MangaBox bot
    /// </summary>
    /// <param name="bob">The bot builder</param>
    /// <returns></returns>
    public static IDiscordSlashCommandBuilder AddMangaBox(this IDiscordSlashCommandBuilder bob)
    {
        return bob
            .With<LookupCommand>();
    }
}
