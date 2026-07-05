using FluentMigrator;

namespace RMS.Modules.Suppliers.Infrastructure.Migrations;

[Migration(7, "Create Suppliers table")]
public sealed class CreateSuppliersTableMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Suppliers").Exists())
        {
            Create.Table("Suppliers")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("SupplierCode").AsString(32).NotNullable().Unique()
                .WithColumn("CompanyName").AsString(200).NotNullable()
                .WithColumn("ContactPerson").AsString(200).Nullable()
                .WithColumn("PhoneNumber").AsString(20).NotNullable().Unique()
                .WithColumn("Email").AsString(254).Nullable().Unique()
                .WithColumn("VatNumber").AsString(50).Nullable().Unique()
                .WithColumn("Street").AsString(200).Nullable()
                .WithColumn("City").AsString(100).Nullable()
                .WithColumn("PostalCode").AsString(20).Nullable()
                .WithColumn("Country").AsString(100).Nullable()
                .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable();

            Create.Index("IX_Suppliers_CompanyName")
                .OnTable("Suppliers")
                .OnColumn("CompanyName").Ascending();

            Create.Index("IX_Suppliers_Status")
                .OnTable("Suppliers")
                .OnColumn("Status").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("Suppliers");
    }
}
