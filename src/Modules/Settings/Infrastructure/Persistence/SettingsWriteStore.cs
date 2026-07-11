using System.Data;
using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Settings.Infrastructure.Persistence;

public sealed class SettingsWriteStore : ISettingsWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SettingsWriteStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task UpsertAsync(string key, string? value, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Settings (Key, Category, Value, DataType, Description)
            VALUES (@Key,
                    COALESCE((SELECT Category FROM Settings WHERE Key = @Key), ''),
                    @Value,
                    COALESCE((SELECT DataType FROM Settings WHERE Key = @Key), 'String'),
                    (SELECT Description FROM Settings WHERE Key = @Key))
            ON CONFLICT(Key) DO UPDATE SET Value = @Value;";
        await connection.ExecuteAsync(sql, new { Key = key, Value = value });
    }

    public async Task UpsertManyAsync(IEnumerable<KeyValuePair<string, string?>> values, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        foreach (var pair in values)
            await UpsertAsync(pair.Key, pair.Value, cancellationToken);
    }

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Settings;");

        foreach (var definition in SettingCatalog.Defaults)
        {
            await connection.ExecuteAsync(
                "INSERT INTO Settings (Key, Category, Value, DataType, Description) VALUES (@Key, @Category, @Value, @DataType, @Description);",
                new
                {
                    definition.Key,
                    definition.Category,
                    Value = definition.DefaultValue,
                    DataType = definition.DataType.ToString(),
                    definition.Description
                });
        }
    }
}
