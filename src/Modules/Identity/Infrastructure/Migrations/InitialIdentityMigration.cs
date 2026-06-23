using FluentMigrator;

namespace RMS.Modules.Identity.Infrastructure.Migrations;

/// <summary>
/// Initial migration for the Identity module.
/// Creates the shared EventStore table (used by all modules) and the Users table.
/// </summary>
[Migration(1, "Initial Identity and EventStore schema")]
public sealed class InitialIdentityMigration : Migration
{
    public override void Up()
    {
        // Shared EventStore table — append-only audit/event log.
        if (!Schema.Table("EventStore").Exists())
        {
            Create.Table("EventStore")
                .WithColumn("EventId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("AggregateId").AsGuid().NotNullable()
                .WithColumn("AggregateType").AsString(256).NotNullable()
                .WithColumn("EventType").AsString(256).NotNullable()
                .WithColumn("PayloadJson").AsString(int.MaxValue).NotNullable()
                .WithColumn("OccurredOn").AsDateTime().NotNullable()
                .WithColumn("Version").AsInt32().NotNullable();

            Create.Index("IX_EventStore_AggregateId")
                .OnTable("EventStore")
                .OnColumn("AggregateId").Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_EventStore_EventType")
                .OnTable("EventStore")
                .OnColumn("EventType").Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_EventStore_OccurredOn")
                .OnTable("EventStore")
                .OnColumn("OccurredOn").Ascending()
                .WithOptions().NonClustered();
        }

        // Identity module tables.
        Create.Table("Users")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("UserName").AsString(50).NotNullable().Unique()
            .WithColumn("Email").AsString(254).NotNullable().Unique()
            .WithColumn("PasswordHash").AsString(256).NotNullable()
            .WithColumn("FullName").AsString(100).NotNullable()
            .WithColumn("Role").AsString(50).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        Create.Index("IX_Users_Role")
            .OnTable("Users")
            .OnColumn("Role").Ascending()
            .WithOptions().NonClustered();
    }

    public override void Down()
    {
        Delete.Table("Users");
        // Intentionally do NOT drop EventStore in Down — it is shared.
    }
}
