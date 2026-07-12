using FluentMigrator;

namespace RMS.Modules.Backup.Infrastructure.Migrations;

[Migration(22, "Create BackupHistory table")]
public sealed class CreateBackupHistoryTableMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("BackupHistory").Exists())
        {
            Create.Table("BackupHistory")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("FileName").AsString(260).NotNullable()
                .WithColumn("FilePath").AsString(520).NotNullable()
                .WithColumn("BackupDate").AsDateTime().NotNullable()
                .WithColumn("Size").AsInt64().NotNullable()
                .WithColumn("UserName").AsString(128).NotNullable()
                .WithColumn("Version").AsString(32).NotNullable()
                .WithColumn("Notes").AsString(1000).Nullable()
                .WithColumn("Checksum").AsString(128).NotNullable();

            Create.Index("IX_BackupHistory_BackupDate")
                .OnTable("BackupHistory")
                .OnColumn("BackupDate").Descending();
        }
    }

    public override void Down() => Delete.Table("BackupHistory");
}
