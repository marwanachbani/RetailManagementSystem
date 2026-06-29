using FluentMigrator;

namespace RMS.Modules.Customers.Infrastructure.Migrations;

[Migration(5, "Create Customers table")]
public sealed class CreateCustomersTableMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Customers").Exists())
        {
            Create.Table("Customers")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("CustomerCode").AsString(32).NotNullable().Unique()
                .WithColumn("FirstName").AsString(100).NotNullable()
                .WithColumn("LastName").AsString(100).NotNullable()
                .WithColumn("PhoneNumber").AsString(20).NotNullable().Unique()
                .WithColumn("Email").AsString(254).Nullable().Unique()
                .WithColumn("Street").AsString(200).Nullable()
                .WithColumn("City").AsString(100).Nullable()
                .WithColumn("PostalCode").AsString(20).Nullable()
                .WithColumn("Country").AsString(100).Nullable()
                .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable();

            Create.Index("IX_Customers_LastName")
                .OnTable("Customers")
                .OnColumn("LastName").Ascending();

            Create.Index("IX_Customers_Status")
                .OnTable("Customers")
                .OnColumn("Status").Ascending();
        }

        if (Schema.Table("Sales").Exists() && !Schema.Table("Sales").Column("CustomerId").Exists())
        {
            Alter.Table("Sales")
                .AddColumn("CustomerId").AsGuid().Nullable();

            Create.Index("IX_Sales_CustomerId")
                .OnTable("Sales")
                .OnColumn("CustomerId").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("Customers");
    }
}
