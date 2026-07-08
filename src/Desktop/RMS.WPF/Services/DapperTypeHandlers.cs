using System.Data;
using System.Globalization;
using Dapper;

namespace RMS.WPF.Services;

/// <summary>
/// SQLite has no native DECIMAL storage class — Microsoft.Data.Sqlite hands back
/// NUMERIC columns as <see cref="double"/> (or occasionally <see cref="long"/> for
/// whole numbers, or <see cref="string"/> if a value was ever inserted as text).
/// Dapper's default column mapping does a direct unboxing cast to the target
/// type, which throws for all of these when the target is <see cref="decimal"/>
/// (e.g. "Error parsing column 8 (CostPrice = 19.99 - Double)"). This handler
/// converts whatever SQLite actually returns into a proper decimal.
/// </summary>
public sealed class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
{
    public override void SetValue(IDbDataParameter parameter, decimal value)
    {
        parameter.DbType = DbType.Decimal;
        parameter.Value = value;
    }

    public override decimal Parse(object value) => ToDecimal(value);

    internal static decimal ToDecimal(object value) => value switch
    {
        decimal d => d,
        double d => (decimal)d,
        float f => (decimal)f,
        long l => l,
        int i => i,
        string s => decimal.Parse(s, CultureInfo.InvariantCulture),
        _ => Convert.ToDecimal(value, CultureInfo.InvariantCulture)
    };
}

/// <summary>Same fix as <see cref="DecimalTypeHandler"/>, for nullable decimal columns.</summary>
public sealed class NullableDecimalTypeHandler : SqlMapper.TypeHandler<decimal?>
{
    public override void SetValue(IDbDataParameter parameter, decimal? value)
    {
        parameter.DbType = DbType.Decimal;
        parameter.Value = (object?)value ?? DBNull.Value;
    }

    public override decimal? Parse(object value) => value is null or DBNull ? null : DecimalTypeHandler.ToDecimal(value);
}
