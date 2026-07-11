using System.Data;
using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Settings.Infrastructure.Persistence;

public sealed class SettingsReadStore : ISettingsReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SettingsReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyDictionary<string, string?>> GetAllValuesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT Key, Value FROM Settings;";
        var rows = await connection.QueryAsync<SettingRow>(sql);
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT Value FROM Settings WHERE Key = @Key;";
        return await connection.ExecuteScalarAsync<string?>(sql, new { Key = key });
    }

    private sealed class SettingRow
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}
