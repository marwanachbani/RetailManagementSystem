using FluentMigrator;

namespace RMS.Modules.Notifications.Infrastructure.Migrations;

[Migration(23, "Create Notifications table")]
public sealed class CreateNotificationsTableMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Notifications").Exists())
        {
            Create.Table("Notifications")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Message").AsString(1000).NotNullable()
                .WithColumn("Category").AsInt32().NotNullable()
                .WithColumn("Severity").AsInt32().NotNullable()
                .WithColumn("CreatedOn").AsDateTime().NotNullable()
                .WithColumn("ReadOn").AsDateTime().Nullable()
                .WithColumn("IsRead").AsBoolean().NotNullable().WithDefaultValue(0)
                .WithColumn("UserId").AsString(50).Nullable()
                .WithColumn("RelatedModule").AsString(100).NotNullable()
                .WithColumn("RelatedEntityId").AsString(50).Nullable();

            Create.Index("IX_Notifications_CreatedOn")
                .OnTable("Notifications")
                .OnColumn("CreatedOn").Descending();

            Create.Index("IX_Notifications_IsRead")
                .OnTable("Notifications")
                .OnColumn("IsRead").Ascending();

            Create.Index("IX_Notifications_Severity")
                .OnTable("Notifications")
                .OnColumn("Severity").Ascending();

            Create.Index("IX_Notifications_UserId")
                .OnTable("Notifications")
                .OnColumn("UserId").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("Notifications");
    }
}
