using FluentMigrator;

namespace RMS.Modules.Printing.Infrastructure.Migrations;

[Migration(24, "Create PrintJobs table")]
public sealed class CreatePrintingTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("PrintJobs").Exists())
        {
            Create.Table("PrintJobs")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("DocumentType").AsString(40).NotNullable()
                .WithColumn("DocumentNumber").AsString(64).Nullable()
                .WithColumn("PrinterName").AsString(128).NotNullable()
                .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("CompletedAt").AsDateTime().Nullable()
                .WithColumn("OutputPath").AsString(500).Nullable()
                .WithColumn("ErrorMessage").AsString(500).Nullable()
                .WithColumn("Copies").AsInt32().NotNullable().WithDefaultValue(1);

            Create.Index("IX_PrintJobs_CreatedAt").OnTable("PrintJobs").OnColumn("CreatedAt").Descending();
        }
    }

    public override void Down() => Delete.Table("PrintJobs");
}
