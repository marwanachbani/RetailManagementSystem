using FluentMigrator;

namespace RMS.Modules.Audit.Infrastructure.Migrations;

[Migration(21, "Create AuditLogs table")]
public sealed class CreateAuditLogsMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("AuditLogs").Exists())
        {
            Create.Table("AuditLogs")
                .WithColumn("AuditId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Timestamp").AsDateTime().NotNullable()
                .WithColumn("UserId").AsGuid().Nullable()
                .WithColumn("UserName").AsString(128).NotNullable()
                .WithColumn("Module").AsString(64).NotNullable()
                .WithColumn("Action").AsString(64).NotNullable()
                .WithColumn("Entity").AsString(64).NotNullable()
                .WithColumn("EntityId").AsString(128).Nullable()
                .WithColumn("OldValue").AsString(int.MaxValue).Nullable()
                .WithColumn("NewValue").AsString(int.MaxValue).Nullable()
                .WithColumn("MachineName").AsString(128).NotNullable()
                .WithColumn("ApplicationVersion").AsString(32).NotNullable();

            Create.Index("IX_AuditLogs_Timestamp")
                .OnTable("AuditLogs")
                .OnColumn("Timestamp").Descending();

            Create.Index("IX_AuditLogs_UserId")
                .OnTable("AuditLogs")
                .OnColumn("UserId").Ascending();

            Create.Index("IX_AuditLogs_Module")
                .OnTable("AuditLogs")
                .OnColumn("Module").Ascending();

            Create.Index("IX_AuditLogs_Action")
                .OnTable("AuditLogs")
                .OnColumn("Action").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("AuditLogs");
    }
}
