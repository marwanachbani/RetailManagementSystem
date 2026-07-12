using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Audit.Infrastructure;
using RMS.Modules.Audit.Infrastructure.Migrations;
using RMS.Modules.Backup.Application;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;
using RMS.Modules.Backup.Domain.Entities;
using RMS.Modules.Backup.Infrastructure;
using RMS.Modules.Backup.Infrastructure.Migrations;
using RMS.Modules.Settings.Application;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;
using RMS.Modules.Settings.Infrastructure;
using RMS.Modules.Settings.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Backup;

public partial class BackupTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly string _backupDir;
    private readonly ServiceProvider _serviceProvider;

    public BackupTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new DecimalTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDecimalTypeHandler());

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_backup_test_{Guid.NewGuid():N}.db");
        _backupDir = Path.Combine(Path.GetTempPath(), $"rms_backup_dir_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_backupDir);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        var services = new ServiceCollection();
        services.AddSingleton<IDbConnectionFactory>(new TestConnectionFactory(connectionString));
        services.AddSingleton<IEventBus, TestEventBus>();
        services.AddSingleton<IEventStore, SqliteEventStore>();
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();
        services.AddSingleton<ICurrentUserContext, TestCurrentUserContext>();
        services.AddSingleton<IFolderResolver>(_ => new FolderResolver(_backupDir));

        services.AddSettingsInfrastructure();
        services.AddBackupModule();
        services.AddBackupInfrastructure();
        services.AddSingleton<IBackupSettingsProvider>(sp =>
        {
            var readStore = sp.GetRequiredService<ISettingsReadStore>();
            var resolver = sp.GetRequiredService<IFolderResolver>();
            return new TestBackupSettingsProvider(_dbFilePath, readStore, resolver);
        });

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(CreateAuditLogsMigration).Assembly,
                    typeof(CreateSettingsTablesMigration).Assembly,
                    typeof(CreateBackupHistoryTableMigration).Assembly).For.Migrations()
            );

        _serviceProvider = services.BuildServiceProvider();

        var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        SeedSettings();
    }

    public IServiceProvider Services => _serviceProvider;

    public void Dispose()
    {
        _serviceProvider.Dispose();
        try { File.Delete(_dbFilePath); } catch { }
        try { Directory.Delete(_backupDir, true); } catch { }
    }

    private sealed class TestConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public TestConnectionFactory(string connectionString) => _connectionString = connectionString;
        public IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    private sealed class TestEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
            => Task.CompletedTask;
    }

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new DateTime(2026, 7, 12, 14, 0, 0, DateTimeKind.Utc);
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public Guid? UserId => null;
        public string? UserName => "System";
        public bool IsAuthenticated => false;
    }

    private sealed class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
    {
        public override void SetValue(IDbDataParameter parameter, decimal value) => parameter.Value = value;
        public override decimal Parse(object value) => Convert.ToDecimal(value);
    }

    private sealed class NullableDecimalTypeHandler : SqlMapper.TypeHandler<decimal?>
    {
        public override void SetValue(IDbDataParameter parameter, decimal? value) => parameter.Value = value ?? (object)DBNull.Value;
        public override decimal? Parse(object value) => value == DBNull.Value ? null : Convert.ToDecimal(value);
    }

    private async void SeedSettings()
    {
        var writeStore = _serviceProvider.GetRequiredService<ISettingsWriteStore>();
        var resolver = _serviceProvider.GetRequiredService<IFolderResolver>();
        resolver.EnsureExists(_backupDir);

        await writeStore.UpsertAsync(SettingCatalog.Keys.StorageBackupFolder, _backupDir);
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupAutomaticBackup, "false");
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupFrequency, "Daily");
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupTime, "23:00");
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupMaximumCount, "10");
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupCompress, "true");
        await writeStore.UpsertAsync(SettingCatalog.Keys.BackupVerifyIntegrity, "true");
    }

    private sealed class TestBackupSettingsProvider : IBackupSettingsProvider
    {
        private readonly string _databasePath;
        private readonly ISettingsReadStore _readStore;
        private readonly IFolderResolver _resolver;

        public TestBackupSettingsProvider(string databasePath, ISettingsReadStore readStore, IFolderResolver resolver)
        {
            _databasePath = databasePath;
            _readStore = readStore;
            _resolver = resolver;
        }

        public async Task<BackupConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
        {
            var values = await _readStore.GetAllValuesAsync(cancellationToken);
            bool Bool(string key) => string.Equals(values.TryGetValue(key, out var v) ? v : null, "true", StringComparison.OrdinalIgnoreCase);
            int Int(string key) => int.TryParse(values.TryGetValue(key, out var v) ? v : null, out var n) ? n : 0;
            string Str(string key) => values.TryGetValue(key, out var v) && v is not null ? v : string.Empty;
            var backupFolder = _resolver.Resolve(Str(SettingCatalog.Keys.StorageBackupFolder), "Backups");
        var contentFolders = new List<string>();
        foreach (var definition in SettingCatalog.FolderDefinitions)
        {
            if (definition.Key == SettingCatalog.Keys.StorageBackupFolder)
                continue;

            var path = _resolver.Resolve(values.TryGetValue(definition.Key, out var v) ? v : definition.DefaultValue, definition.FolderSubPath);
            if (Directory.Exists(path)) contentFolders.Add(path);
        }

            return new BackupConfiguration
            {
                DatabasePath = _databasePath,
                BackupFolder = backupFolder,
                ContentFolders = contentFolders,
                AutomaticBackup = Bool(SettingCatalog.Keys.BackupAutomaticBackup),
                Frequency = Str(SettingCatalog.Keys.BackupFrequency),
                Time = Str(SettingCatalog.Keys.BackupTime),
                MaximumCount = Int(SettingCatalog.Keys.BackupMaximumCount),
                Compress = Bool(SettingCatalog.Keys.BackupCompress),
                VerifyIntegrity = Bool(SettingCatalog.Keys.BackupVerifyIntegrity),
                ApplicationVersion = "1.0.0.0"
            };
        }
    }
}

public partial class BackupTestDatabaseFixture
{
    public void ResetState()
    {
        using var connection = _serviceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM BackupHistory;");
        if (Directory.Exists(_backupDir))
        {
            foreach (var dir in Directory.GetDirectories(_backupDir))
                Directory.Delete(dir, true);
            foreach (var file in Directory.GetFiles(_backupDir, "*.zip"))
                File.Delete(file);
        }
    }
}
