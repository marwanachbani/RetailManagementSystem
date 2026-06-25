using FluentMigrator;

namespace RMS.Modules.Inventory.Infrastructure.Migrations;

[Migration(3, "Create Inventory tables")]
public sealed class CreateInventoryTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("InventoryItems").Exists())
        {
            Create.Table("InventoryItems")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("ProductId").AsGuid().NotNullable()
                .WithColumn("CurrentQuantity").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("LowStockThreshold").AsInt32().NotNullable().WithDefaultValue(10);

            Create.Index("IX_InventoryItems_ProductId")
                .OnTable("InventoryItems")
                .OnColumn("ProductId").Ascending();

            Create.Index("IX_InventoryItems_CurrentQuantity")
                .OnTable("InventoryItems")
                .OnColumn("CurrentQuantity").Ascending();

            Create.Index("IX_InventoryItems_IsActive")
                .OnTable("InventoryItems")
                .OnColumn("IsActive").Ascending();

            Create.Index("IX_InventoryItems_UpdatedAt")
                .OnTable("InventoryItems")
                .OnColumn("UpdatedAt").Ascending();
        }

        if (!Schema.Table("InventoryTransactions").Exists())
        {
            Create.Table("InventoryTransactions")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("InventoryItemId").AsGuid().NotNullable().ForeignKey("FK_InventoryTransactions_InventoryItems", "InventoryItems", "Id")
                .WithColumn("ProductId").AsGuid().NotNullable()
                .WithColumn("QuantityBefore").AsInt32().NotNullable()
                .WithColumn("QuantityAfter").AsInt32().NotNullable()
                .WithColumn("ChangeAmount").AsInt32().NotNullable()
                .WithColumn("Reason").AsString(500).NotNullable()
                .WithColumn("UserId").AsGuid().Nullable()
                .WithColumn("Timestamp").AsDateTime().NotNullable();

            Create.Index("IX_InventoryTransactions_InventoryItemId")
                .OnTable("InventoryTransactions")
                .OnColumn("InventoryItemId").Ascending();

            Create.Index("IX_InventoryTransactions_ProductId")
                .OnTable("InventoryTransactions")
                .OnColumn("ProductId").Ascending();

            Create.Index("IX_InventoryTransactions_Timestamp")
                .OnTable("InventoryTransactions")
                .OnColumn("Timestamp").Ascending();
        }
    }

    public override void Down()
    {
        Delete.Table("InventoryTransactions");
        Delete.Table("InventoryItems");
    }
}
