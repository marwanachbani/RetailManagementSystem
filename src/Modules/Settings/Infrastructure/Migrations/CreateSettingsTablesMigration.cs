using FluentMigrator;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Settings.Infrastructure.Migrations;

/// <summary>
/// Creates the Settings and SettingCategories tables and seeds them from the
/// single source of truth (<see cref="SettingCatalog"/>). Runs once (FluentMigrator
/// versioning guarantees a migration's Up() executes a single time).
/// </summary>
[Migration(20, "Create Settings and SettingCategories tables")]
public sealed class CreateSettingsTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("SettingCategories").Exists())
        {
            Create.Table("SettingCategories")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(50).NotNullable().Unique()
                .WithColumn("DisplayName").AsString(100).NotNullable()
                .WithColumn("SortOrder").AsInt32().NotNullable();

            Create.Index("IX_SettingCategories_SortOrder")
                .OnTable("SettingCategories")
                .OnColumn("SortOrder").Ascending();
        }

        if (!Schema.Table("Settings").Exists())
        {
            Create.Table("Settings")
                .WithColumn("Key").AsString(128).NotNullable().PrimaryKey()
                .WithColumn("Category").AsString(50).NotNullable()
                .WithColumn("Value").AsString(int.MaxValue).Nullable()
                .WithColumn("DataType").AsString(20).NotNullable()
                .WithColumn("Description").AsString(256).Nullable();

            Create.Index("IX_Settings_Category")
                .OnTable("Settings")
                .OnColumn("Category").Ascending();
        }

        foreach (var category in SettingCatalog.Categories)
        {
            Insert.IntoTable("SettingCategories")
                .Row(new { category.Id, category.Name, category.DisplayName, category.SortOrder });
        }

        foreach (var definition in SettingCatalog.Defaults)
        {
            Insert.IntoTable("Settings")
                .Row(new
                {
                    definition.Key,
                    definition.Category,
                    Value = definition.DefaultValue,
                    DataType = definition.DataType.ToString(),
                    definition.Description
                });
        }
    }

    public override void Down()
    {
        Delete.Table("Settings");
        Delete.Table("SettingCategories");
    }
}
