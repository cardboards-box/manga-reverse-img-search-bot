namespace MangaBox.Bot.Core;

using Models;
using Models.V1;

public interface ILookupDbService
{
    Task<Guid> Upsert(LookupRequest request);

    Task<Guid> Upsert(LookupInteraction interaction);

    Task<Guid> Upsert(GuildConfig config);

    Task<LookupInteraction?> Interaction(string messageId, string userId);

    Task<LookupRequest?> Request(string messageId);

    Task<GuildConfig?> GuildConfig(ulong guildId) => GuildConfig(guildId.ToString());

    Task<GuildConfig?> GuildConfig(string guildId);
}

public class LookupDbService(
    IQueryService _query,
    ISqlService _sql) : ILookupDbService
{
    private static string? _guildFetch;
    private static string? _guildUpsert;
    private static string? _requestFetch;
    private static string? _requestUpsert;
    private static string? _interactionFetch;
    private static string? _interactionUpsert;

    public Task<Guid> Upsert(LookupRequest request)
    {
        _requestUpsert ??= _query.Upsert<LookupRequest>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_requestUpsert, request);
    }

    public Task<Guid> Upsert(LookupInteraction interaction)
    {
        _interactionUpsert ??= _query.Upsert<LookupInteraction>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_interactionUpsert, interaction);
    }

    public Task<LookupRequest?> Request(string messageId)
    {
        _requestFetch ??= _query.Select<LookupRequest>(t => t.With(a => a.MessageId));
        return _sql.Fetch<LookupRequest?>(_requestFetch, new { MessageId = messageId });
    }

    public Task<LookupInteraction?> Interaction(string messageId, string userId)
    {
        _interactionFetch ??= _query.Select<LookupInteraction>(t => t.With(a => a.MessageId).With(a => a.UserId));
        return _sql.Fetch<LookupInteraction?>(_interactionFetch, new { MessageId = messageId, UserId = userId });
    }

    public Task<Guid> Upsert(GuildConfig config)
    {
        _guildFetch ??= _query.Upsert<GuildConfig>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_guildFetch, config);
    }

    public Task<GuildConfig?> GuildConfig(string guildId)
    {
        _guildUpsert ??= _query.Select<GuildConfig>(t => t.With(a => a.GuildId));
        return _sql.Fetch<GuildConfig?>(_guildUpsert, new { GuildId = guildId });
    }
}
