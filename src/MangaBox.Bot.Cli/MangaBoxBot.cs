namespace MangaBox.Bot.Cli;

using DiscordConfig = Core.Models.DiscordConfig;
using IDiscordClient = CardboardBox.Discord.IDiscordClient;

public static class MangaBoxBot
{
    public static (IServiceCollection, IConfiguration) DiStuff()
    {
        var services = new ServiceCollection()
            .AddConfig(c => c
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables(),
                out var config)
            .AddCore(config)
            .AddMangaBox();

        return (services, config);
    }

    public static DiscordSocketConfig GetDiscordConfig(IConfiguration config)
    {
        var settings = config.GetValue<DiscordConfig>("Discord") ?? new();
        return new DiscordSocketConfig
        {
            UseInteractionSnowflakeDate = !settings.Local,
            GatewayIntents = settings.ParseIntents()
        };
    }

    public static IDiscordClient CreateBot()
    {
        var (services, config) = DiStuff();
        var socket = GetDiscordConfig(config);
        var client = new DiscordSocketClient(socket);
        return DiscordBotBuilder.Start(services, client)
            .WithSlashCommands(c => c
                .AddMangaBox())
            .Build();
    }
}
