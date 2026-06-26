using FluentMigrator;

namespace RMS.Modules.Sales.Infrastructure.Migrations;

[Migration(4, "Create Sales schema")]
public sealed class CreateSalesTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Sales").Exists())
        {
            Create.Table("Sales")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("SaleNumber").AsString(32).NotNullable().Unique()
                .WithColumn("CashierId").AsGuid().NotNullable()
                .WithColumn("SaleDate").AsDateTime().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("SubTotal").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("DiscountAmount").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("DiscountPercentage").AsDecimal(5, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("CompletedAt").AsDateTime().Nullable()
                .WithColumn("RefundedAt").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("Notes").AsString(500).Nullable();

            Create.Index("IX_Sales_SaleDate")
                .OnTable("Sales")
                .OnColumn("SaleDate").Ascending();

            Create.Index("IX_Sales_CashierId")
                .OnTable("Sales")
                .OnColumn("CashierId").Ascending();

            Create.Index("IX_Sales_Status")
                .OnTable("Sales")
                .OnColumn("Status").Ascending();
        }

        if (!Schema.Table("SaleItems").Exists())
        {
            Create.Table("SaleItems")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("SaleId").AsGuid().NotNullable().ForeignKey("FK_SaleItems_Sales", "Sales", "Id")
                .WithColumn("ProductId").AsGuid().NotNullable()
                .WithColumn("ProductName").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("UnitPrice").AsDecimal(18, 2).NotNullable()
                .WithColumn("TotalPrice").AsDecimal(18, 2).NotNullable();

            Create.Index("IX_SaleItems_SaleId")
                .OnTable("SaleItems")
                .OnColumn("SaleId").Ascending();

            Create.Index("IX_SaleItems_ProductId")
                .OnTable("SaleItems")
                .OnColumn("ProductId").Ascending();
        }

        if (!Schema.Table("Receipts").Exists())
        {
            Create.Table("Receipts")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("SaleId").AsGuid().NotNullable().ForeignKey("FK_Receipts_Sales", "Sales", "Id")
                .WithColumn("ReceiptNumber").AsString(32).NotNullable().Unique()
                .WithColumn("PdfPath").AsString(500).Nullable()
                .WithColumn("GeneratedAt").AsDateTime().NotNullable()
                .WithColumn("StoreName").AsString(100).Nullable()
                .WithColumn("CashierName").AsString(100).Nullable()
                .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable();
        }
    }

    public override void Down()
    {
        Delete.Table("Receipts");
        Delete.Table("SaleItems");
        Delete.Table("Sales");
    }
}
