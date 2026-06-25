using System.Data;
using Dapper;

namespace RMS.BuildingBlocks.Persistence;

/// <summary>
/// Dapper type handler for SQLite GUID columns. SQLite stores GUIDs as TEXT,
/// so the default Dapper mapper fails with <see cref="InvalidCastException"/>
/// when trying to materialize <see cref="Guid"/> properties. This handler
/// bridges the string-to-Guid conversion explicitly.
/// </summary>
public sealed class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString("D");
        parameter.DbType = DbType.String;
    }

    public override Guid Parse(object value)
    {
        return value switch
        {
            Guid guid => guid,
            string str => new Guid(str),
            byte[] bytes => new Guid(bytes),
            _ => throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to Guid.")
        };
    }
}

/// <summary>
/// Companion handler for nullable GUIDs so that Dapper correctly maps
/// <c>NULL</c> SQLite values to <see cref="Guid?"/>.
/// </summary>
public sealed class SqliteNullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    public override void SetValue(IDbDataParameter parameter, Guid? value)
    {
        parameter.Value = value?.ToString("D");
        parameter.DbType = value.HasValue ? DbType.String : DbType.Object;
    }

    public override Guid? Parse(object value)
    {
        return value switch
        {
            null or DBNull => null,
            Guid guid => guid,
            string str => new Guid(str),
            byte[] bytes => new Guid(bytes),
            _ => throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to Guid?.")
        };
    }
}
