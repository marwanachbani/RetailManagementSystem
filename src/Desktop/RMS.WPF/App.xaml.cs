using System.IO;
using System.Windows;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Logging;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Identity.Application;
using RMS.Modules.Identity.Application.Contracts;
using RMS.Modules.Identity.Domain.Entities;
using RMS.Modules.Identity.Domain.Services;
using RMS.Modules.Identity.Domain.ValueObjects;
using RMS.Modules.Identity.Infrastructure;

using RMS.Modules.Inventory.Application;

using RMS.Modules.Inventory.Infrastructure;
using RMS.Modules.Products.Application;
using RMS.Modules.Sales.Application;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Sales.Infrastructure;
using RMS.Modules.Sales.Infrastructure.ReceiptGeneration;
using RMS.WPF.ViewModels;
using RMS.WPF.ReceiptGeneration;
using RMS.WPF.Views;
using Serilog;

namespace RMS.WPF;

/// <summary>
/// Composition root for the whole application. This is the ONLY place that
/// knows about every module's registration extension method. No module
/// references another module's types here — they are wired up independently
/// and talk to each other only through IEventBus.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    public static readonly string ProgramDataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RetailManagementSystem");

    public static readonly string DatabasePath = Path.Combine(ProgramDataDirectory, "rms.db");
    public static readonly string LogsDirectory = Path.Combine(ProgramDataDirectory, "logs");
    public static readonly string BackupsDirectory = Path.Combine(ProgramDataDirectory, "backups");

    private static readonly string ConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = DatabasePath,
        Mode = SqliteOpenMode.ReadWriteCreate,
        ForeignKeys = true
    }.ToString();

    static App()
    {
        // Register SQLite GUID type handlers once per AppDomain before any Dapper queries.
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        EnsureProgramDataFolders();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        _host.Start();
        
        // Apply database migrations before any business logic runs.
        using (var scope = _host.Services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();

            SeedAdminAccountAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            SeedProductsAndInventoryAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }

        Log.Information("RMS application starting up. Database: {DbPath}", DatabasePath);

        ShowLoginWindow();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("RMS application shutting down.");
        Log.CloseAndFlush();
        _host?.Dispose();
        base.OnExit(e);
    }

    private void ShowLoginWindow()
    {
        var loginWindow = new LoginWindow();
        var loginViewModel = _host!.Services.GetRequiredService<LoginViewModel>();

        loginViewModel.LoginSucceeded += (_, result) =>
        {
            Log.Information("User {UserName} logged in successfully.", result.UserName);
            var mainWindow = new MainWindow(
                _host!.Services.GetRequiredService<MainWindowViewModel>()
);
            mainWindow.Show();
            loginWindow.Close();
        };

        loginWindow.DataContext = loginViewModel;
        loginWindow.Show();
    }

    private static void EnsureProgramDataFolders()
    {
        Directory.CreateDirectory(ProgramDataDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }

    private static async Task SeedAdminAccountAsync(IServiceProvider services)
    {
        const string userName = "admin";
        const string password = "Admin@123";

        var readStore = services.GetRequiredService<IUserReadStore>();
        if (await readStore.GetByUserNameAsync(userName) is not null)
            return;

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var writeStore = services.GetRequiredService<IUserWriteStore>();

        var admin = User.Create(
            Guid.NewGuid(),
            userName,
            Email.Create("admin@rms.local"),
            PasswordHash.Create(passwordHasher.Hash(password)),
            "System Administrator",
            UserRole.Admin);

        await writeStore.InsertAsync(admin);
        Log.Information("Seeded default admin account. UserName: {UserName}", userName);
    }

    private static async Task SeedProductsAndInventoryAsync(IServiceProvider services)
    {
        using var connection = services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        var existing = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Products;");
        if (existing > 0)
        {
            Log.Information("Skipped product and inventory seeding because Products already contains data.");
            return;
        }

        var electronicsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var clothingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var groceriesId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var now = DateTime.UtcNow.ToString("O");

        var products = new[]
        {
            new { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), ProductCode = "PRD-LAPTOP01", Name = "Laptop HP ProBook", Description = "Normal stock", Barcode = "BAR-LAPTOP-001", CategoryId = electronicsId, SalePrice = 899.99m, CostPrice = 650.00m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null },
            new { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), ProductCode = "PRD-PHONE001", Name = "Smartphone Galaxy S24", Description = "Low stock (shows alert)", Barcode = "BAR-PHONE-001", CategoryId = electronicsId, SalePrice = 799.99m, CostPrice = 580.00m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null },
            new { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), ProductCode = "PRD-TSHIRT01", Name = "Cotton T-Shirt", Description = "Normal stock", Barcode = "BAR-TSHIRT-001", CategoryId = clothingId, SalePrice = 24.99m, CostPrice = 8.50m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null },
            new { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), ProductCode = "PRD-JEANS01", Name = "Denim Jeans", Description = "Out of stock", Barcode = "BAR-JEANS-001", CategoryId = clothingId, SalePrice = 59.99m, CostPrice = 22.00m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null },
            new { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), ProductCode = "PRD-RICE001", Name = "Basmati Rice 5kg", Description = "Normal stock", Barcode = "BAR-RICE-001", CategoryId = groceriesId, SalePrice = 14.99m, CostPrice = 9.50m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null },
            new { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), ProductCode = "PRD-OIL001", Name = "Olive Oil 1L", Description = "Normal stock", Barcode = "BAR-OIL-001", CategoryId = groceriesId, SalePrice = 12.99m, CostPrice = 7.80m, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null }
        };

        const string productSql = """
            INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt, @UpdatedAt);
            """;

        foreach (var p in products)
            await connection.ExecuteAsync(productSql, p);

        var inventoryItems = new[]
        {
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111a"), ProductId = products[0].Id, CurrentQuantity = 25, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 10 },
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111b"), ProductId = products[1].Id, CurrentQuantity = 8, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 10 },
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111c"), ProductId = products[2].Id, CurrentQuantity = 50, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 20 },
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111d"), ProductId = products[3].Id, CurrentQuantity = 0, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 15 },
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111e"), ProductId = products[4].Id, CurrentQuantity = 100, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 30 },
            new { Id = Guid.Parse("11111111-1111-1111-1111-11111111111f"), ProductId = products[5].Id, CurrentQuantity = 45, IsActive = 1, CreatedAt = now, UpdatedAt = (string?)null, LowStockThreshold = 25 }
        };

        const string inventorySql = """
            INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold)
            VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @UpdatedAt, @LowStockThreshold);
            """;

        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var p in products)
                await connection.ExecuteAsync(productSql, p, transaction);

            foreach (var i in inventoryItems)
                await connection.ExecuteAsync(inventorySql, i, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        Log.Information("Seeded {ProductCount} products and {InventoryCount} inventory items.", products.Length, inventoryItems.Length);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddRmsLogging(LogsDirectory);

        services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(DatabasePath));
        services.AddSingleton<IEventBus, InProcessEventBus>();
        services.AddSingleton<IEventStore, SqliteEventStore>();
        services.Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite");

        // Identity module — fully wired vertical slice.
        services.AddIdentityModule();
        services.AddIdentityInfrastructure();
        services.AddIdentityMigrations(ConnectionString);

        // Products module — fully wired vertical slice.
        services.AddProductsModule();
        services.AddProductsInfrastructure();
        services.AddProductsMigrations(ConnectionString);

        services.AddSingleton<IReceiptGenerator, WpfReceiptGenerator>();

        // Inventory module — fully wired vertical slice.
        services.AddInventoryModule();
        services.AddInventoryInfrastructure();
        services.AddInventoryMigrations(ConnectionString);

        // WPF view models.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<CreateProductViewModel>();
        services.AddTransient<EditProductViewModel>();

        services.AddTransient<InventoryListViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<CreateSaleViewModel>();
        services.AddTransient<SalesHistoryViewModel>();

        services.AddTransient<StockAdjustmentViewModel>();

        services.AddTransient<InventoryHistoryViewModel>();

        // WPF views.
        services.AddTransient<ProductListWindow>();
        services.AddTransient<CreateProductWindow>();
        services.AddTransient<EditProductWindow>();

        services.AddTransient<InventoryListWindow>();

        services.AddTransient<StockAdjustmentWindow>();

        services.AddTransient<InventoryHistoryWindow>();
    }
}

internal sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();
    }

    public System.Data.IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
