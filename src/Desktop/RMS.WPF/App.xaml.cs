using System.IO;
using System.Windows;
using System.Windows.Threading;
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
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Purchasing.Infrastructure;
using RMS.Modules.Reporting.Application;
using RMS.Modules.Reporting.Infrastructure;
using RMS.Modules.Suppliers.Application;
using RMS.Modules.Suppliers.Infrastructure;
using RMS.Modules.Settings.Application;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;
using RMS.Modules.Settings.Infrastructure;
using RMS.WPF.ViewModels;
using RMS.WPF.Settings;
using RMS.WPF.Services;
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

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            EnsureProgramDataFolders();

            // SQLite has no native DECIMAL type — Microsoft.Data.Sqlite returns
            // NUMERIC(18,2) columns (SalePrice, CostPrice, totals, etc.) as double,
            // and Dapper's default mapping throws "Error parsing column N" trying to
            // cast double -> decimal. Registering a handler fixes every query that
            // reads a decimal column across every module, app-wide, in one place.
            SqlMapper.AddTypeHandler(new DecimalTypeHandler());
            SqlMapper.AddTypeHandler(new NullableDecimalTypeHandler());

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
                EnsureSettingsFolders(scope.ServiceProvider);
            }

            Log.Information("RMS application starting up. Database: {DbPath}", DatabasePath);

            ShowLoginWindow();
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application startup");
            MessageBox.Show(
                $"Fatal error during startup:\n\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                "RMS - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Fatal(ex, "Unhandled AppDomain exception");
        MessageBox.Show(
            $"Unhandled AppDomain exception:\n\n{ex?.GetType().Name}: {ex?.Message}\n\n{ex?.StackTrace}",
            "RMS - Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled dispatcher exception");
        MessageBox.Show(
            $"Unhandled UI exception:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
            "RMS - Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unobserved task exception");
        e.SetObserved();
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

            var session = _host!.Services.GetRequiredService<ICurrentSessionService>();
            session.SignIn(result.UserId, result.UserName, result.FullName, result.Role);

            var mainViewModel = _host!.Services.GetRequiredService<MainWindowViewModel>();
            mainViewModel.CurrentUserName = string.IsNullOrWhiteSpace(result.FullName) ? result.UserName : result.FullName;

            var mainWindow = new MainWindow(mainViewModel, session);
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

    /// <summary>
    /// Creates the default ProgramData folder structure required by the Settings
    /// module so exports, reports, receipts, backups and other artifacts always
    /// have a valid destination on first launch.
    /// </summary>
    private static void EnsureSettingsFolders(IServiceProvider services)
    {
        var resolver = services.GetService<IFolderResolver>();
        if (resolver is null) return;

        Directory.CreateDirectory(resolver.BaseDirectory);
        foreach (var folder in SettingCatalog.FolderDefinitions)
            resolver.EnsureExists(resolver.GetDefaultPath(folder.FolderSubPath!));
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

        services.AddSingleton<ICurrentSessionService, CurrentSessionService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFolderBrowserService, FolderBrowserService>();

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
        services.AddSingleton<IPurchaseOrderDocumentGenerator, WpfPurchaseOrderDocumentGenerator>();

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

        // Purchasing module — fully wired vertical slice.
        services.AddPurchasingModule();
        services.AddPurchasingInfrastructure();
        services.AddPurchasingMigrations(ConnectionString);

        // Reporting module — fully wired vertical slice.
        services.AddReportingModule();
        services.AddReportingInfrastructure();
        services.AddReportingMigrations(ConnectionString);

        // Suppliers module — fully wired vertical slice.
        services.AddSuppliersModule();
        services.AddSuppliersInfrastructure();
        services.AddSuppliersMigrations(ConnectionString);

        // Settings module — fully wired vertical slice.
        services.AddSettingsModule(ProgramDataDirectory);
        services.AddSettingsInfrastructure();
        services.AddSettingsMigrations(ConnectionString);

        // Dashboard queries live in the WPF assembly — register MediatR handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(App).Assembly));

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
        services.AddTransient<SaleDetailsViewModel>();
        services.AddTransient<StockAdjustmentViewModel>();
        services.AddTransient<InventoryHistoryViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CreateCustomerViewModel>();
        services.AddTransient<EditCustomerViewModel>();

        services.AddTransient<SupplierListViewModel>();
        services.AddTransient<CreateSupplierViewModel>();
        services.AddTransient<EditSupplierViewModel>();

        services.AddTransient<PurchaseOrdersViewModel>();
        services.AddTransient<CreatePurchaseOrderViewModel>();
        services.AddTransient<EditPurchaseOrderViewModel>();
        services.AddTransient<ReceiveGoodsViewModel>();
        services.AddTransient<PurchaseHistoryViewModel>();

        services.AddTransient<SalesReportViewModel>();
        services.AddTransient<InventoryReportViewModel>();
        services.AddTransient<PurchasingReportViewModel>();
        services.AddTransient<CustomerReportViewModel>();
        services.AddTransient<SupplierReportViewModel>();
        services.AddTransient<ProductReportViewModel>();
        services.AddTransient<FinancialReportViewModel>();
        services.AddTransient<ReportsViewModel>();

        // WPF views.
        services.AddTransient<ProductListWindow>();
        services.AddTransient<CreateProductWindow>();
        services.AddTransient<EditProductWindow>();
        services.AddTransient<InventoryListWindow>();
        services.AddTransient<StockAdjustmentWindow>();
        services.AddTransient<InventoryHistoryWindow>();
        services.AddTransient<CreateSaleWindow>();
        services.AddTransient<SalesHistoryWindow>();
        services.AddTransient<SaleDetailsWindow>();
        services.AddTransient<CreateCustomerWindow>();
        services.AddTransient<EditCustomerWindow>();

        services.AddTransient<SupplierListView>();
        services.AddTransient<CreateSupplierWindow>();
        services.AddTransient<EditSupplierWindow>();

        services.AddTransient<CreatePurchaseOrderWindow>();
        services.AddTransient<EditPurchaseOrderWindow>();
        services.AddTransient<ReceiveGoodsWindow>();
        services.AddTransient<PurchaseHistoryWindow>();

        services.AddTransient<ReportsView>();

        // Settings module — WPF shell.
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsView>();
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
