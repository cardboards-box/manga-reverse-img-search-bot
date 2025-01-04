namespace MangaBox.Bot.Core;

using Models;

public interface ILookupDbService
{
    Task<Guid> Upsert(LookupRequest request);

    Task<LookupRequest?> Fetch(string messageId);
}

public class LookupDbService(
    IQueryService _query,
    ISqlService _sql) : ILookupDbService
{
    private string? _fetch;
    private string? _upsert;

    public Task<Guid> Upsert(LookupRequest request)
    {
        _upsert ??= _query.Upsert<LookupRequest>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_upsert, request);
    }

    public Task<LookupRequest?> Fetch(string messageId)
    {
        _fetch ??= _query.Select<LookupRequest>(t => t.With(a => a.MessageId));
        return _sql.Fetch<LookupRequest?>(_fetch, new { MessageId = messageId });
    }
}
