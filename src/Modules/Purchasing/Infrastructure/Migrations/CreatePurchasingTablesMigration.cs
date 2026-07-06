using FluentMigrator;

namespace RMS.Modules.Purchasing.Infrastructure.Migrations;

[Migration(6, "Create Purchasing tables")]
public sealed class CreatePurchasingTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("PurchaseOrders").Exists())
        {
            Create.Table("PurchaseOrders")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("PurchaseNumber").AsString(32).NotNullable().Unique()
                .WithColumn("SupplierId").AsGuid().NotNullable()
                .WithColumn("SupplierName").AsString(200).NotNullable()
                .WithColumn("OrderDate").AsDateTime().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("SubTotal").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("CompletedAt").AsDateTime().Nullable()
                .WithColumn("CancelledAt").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("Notes").AsString(500).Nullable()
                .WithColumn("SupplierInvoiceNumber").AsString(100).Nullable();

            Create.Index("IX_PurchaseOrders_SupplierId")
                .OnTable("PurchaseOrders")
                .OnColumn("SupplierId").Ascending();

            Create.Index("IX_PurchaseOrders_Status")
                .OnTable("PurchaseOrders")
                .OnColumn("Status").Ascending();

            Create.Index("IX_PurchaseOrders_OrderDate")
                .OnTable("PurchaseOrders")
                .OnColumn("OrderDate").Ascending();
        }

        if (!Schema.Table("PurchaseOrderItems").Exists())
        {
            Create.Table("PurchaseOrderItems")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("PurchaseOrderId").AsGuid().NotNullable().ForeignKey("FK_PurchaseOrderItems_PurchaseOrders", "PurchaseOrders", "Id")
                .WithColumn("ProductId").AsGuid().NotNullable()
                .WithColumn("ProductName").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("UnitCost").AsDecimal(18, 2).NotNullable()
                .WithColumn("TotalCost").AsDecimal(18, 2).NotNullable()
                .WithColumn("ReceivedQuantity").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Index("IX_PurchaseOrderItems_PurchaseOrderId")
                .OnTable("PurchaseOrderItems")
                .OnColumn("PurchaseOrderId").Ascending();

            Create.Index("IX_PurchaseOrderItems_ProductId")
                .OnTable("PurchaseOrderItems")
                .OnColumn("ProductId").Ascending();
        }

        if (!Schema.Table("GoodsReceipts").Exists())
        {
            Create.Table("GoodsReceipts")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("PurchaseOrderId").AsGuid().NotNullable().ForeignKey("FK_GoodsReceipts_PurchaseOrders", "PurchaseOrders", "Id")
                .WithColumn("ProductId").AsGuid().NotNullable()
                .WithColumn("QuantityReceived").AsInt32().NotNullable()
                .WithColumn("ReceivedAt").AsDateTime().NotNullable()
                .WithColumn("BatchNumber").AsString(100).Nullable()
                .WithColumn("ExpiryDate").AsDateTime().Nullable();

            Create.Index("IX_GoodsReceipts_PurchaseOrderId")
                .OnTable("GoodsReceipts")
                .OnColumn("PurchaseOrderId").Ascending();

            Create.Index("IX_GoodsReceipts_ProductId")
                .OnTable("GoodsReceipts")
                .OnColumn("ProductId").Ascending();

            Create.Index("IX_GoodsReceipts_ReceivedAt")
                .OnTable("GoodsReceipts")
                .OnColumn("ReceivedAt").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("GoodsReceipts");
        Delete.Table("PurchaseOrderItems");
        Delete.Table("PurchaseOrders");
    }
}
