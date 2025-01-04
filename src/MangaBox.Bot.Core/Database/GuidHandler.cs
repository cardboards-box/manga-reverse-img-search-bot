using System.Data;

namespace MangaBox.Bot.Core.Database;

public class GuidHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        var strValue = value?.ToString();
        if (string.IsNullOrEmpty(strValue))
            return Guid.Empty;
        return Guid.TryParse(strValue, out var result) ? result : Guid.Empty;
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}
