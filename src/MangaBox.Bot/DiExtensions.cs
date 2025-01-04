namespace MangaBox.Bot;

using Services;

public static class DiExtensions
{
    public static IServiceCollection AddMangaBox(this IServiceCollection services)
    {
        return services
            .AddTransient<IEmbedService, EmbedService>()
            .AddTransient<IMangaLookupService, MangaLookupService>()
            .AddTransient<ILookupHooks, LookupHooks>();
    }

    public static IDiscordSlashCommandBuilder AddMangaBox(this IDiscordSlashCommandBuilder bob)
    {
        return bob
            .With<LookupCommand>();
    }
}
