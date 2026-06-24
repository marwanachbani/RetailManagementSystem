using FluentMigrator;

namespace RMS.Modules.Products.Infrastructure.Migrations;

[Migration(2, "Create Products and Categories schema")]
public sealed class CreateProductsTablesMigration : Migration
{
    public static readonly Guid ElectronicsCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ClothingCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid GroceriesCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public override void Up()
    {
        if (!Schema.Table("Categories").Exists())
        {
            Create.Table("Categories")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(100).NotNullable().Unique()
                .WithColumn("Description").AsString(500).Nullable();
        }

        if (!Schema.Table("Products").Exists())
        {
            Create.Table("Products")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("ProductCode").AsString(32).NotNullable().Unique()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Description").AsString(1000).Nullable()
                .WithColumn("Barcode").AsString(64).NotNullable().Unique()
                .WithColumn("CategoryId").AsGuid().NotNullable().ForeignKey("FK_Products_Categories", "Categories", "Id")
                .WithColumn("SalePrice").AsDecimal(18, 2).NotNullable()
                .WithColumn("CostPrice").AsDecimal(18, 2).NotNullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable();

            Create.Index("IX_Products_Name")
                .OnTable("Products")
                .OnColumn("Name").Ascending();

            Create.Index("IX_Products_CategoryId")
                .OnTable("Products")
                .OnColumn("CategoryId").Ascending();

            Create.Index("IX_Products_IsActive")
                .OnTable("Products")
                .OnColumn("IsActive").Ascending();
        }

        Insert.IntoTable("Categories").Row(new { Id = ElectronicsCategoryId, Name = "Electronics", Description = "Devices, accessories, and electronic goods" });
        Insert.IntoTable("Categories").Row(new { Id = ClothingCategoryId, Name = "Clothing", Description = "Apparel, footwear, and accessories" });
        Insert.IntoTable("Categories").Row(new { Id = GroceriesCategoryId, Name = "Groceries", Description = "Food, beverages, and household essentials" });
    }

    public override void Down()
    {
        Delete.Table("Products");
        Delete.Table("Categories");
    }
}
