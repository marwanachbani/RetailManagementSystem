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
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Customers.Application;
using RMS.Modules.Customers.Infrastructure;
using RMS.Modules.Sales.Application;
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
            DevelopmentSeed.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
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

    public void ShowLoginWindow()
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

        // Sales module — fully wired vertical slice.
        services.AddSalesModule();
        services.AddSalesInfrastructure();
        services.AddSalesMigrations(ConnectionString);

        // Customers module — fully wired vertical slice.
        services.AddCustomersModule();
        services.AddCustomersInfrastructure();
        services.AddCustomersMigrations(ConnectionString);

        // WPF view models.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<CreateProductViewModel>();
        services.AddTransient<EditProductViewModel>();
        services.AddTransient<InventoryListViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<CreateSaleViewModel>();
        services.AddTransient<SalesHistoryViewModel>();
        services.AddTransient<StockAdjustmentViewModel>();
        services.AddTransient<InventoryHistoryViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CreateCustomerViewModel>();
        services.AddTransient<EditCustomerViewModel>();

        // WPF views.
        services.AddTransient<ProductListWindow>();
        services.AddTransient<CreateProductWindow>();
        services.AddTransient<EditProductWindow>();
        services.AddTransient<InventoryListWindow>();
        services.AddTransient<StockAdjustmentWindow>();
        services.AddTransient<InventoryHistoryWindow>();
        services.AddTransient<CreateSaleWindow>();
        services.AddTransient<SalesHistoryWindow>();
        services.AddTransient<CreateCustomerWindow>();
        services.AddTransient<EditCustomerWindow>();
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
