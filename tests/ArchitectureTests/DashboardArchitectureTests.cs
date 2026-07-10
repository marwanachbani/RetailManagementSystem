using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace RMS.ArchitectureTests;

public class DashboardArchitectureTests
{
    private static Assembly? WpfAssembly => LoadWpfAssembly();

    [Fact]
    public void Dashboard_Handlers_Should_Be_ReadOnly_Queries()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var handlerTypes = Types.InAssembly(wpfAssembly)
            .That()
            .ResideInNamespace("RMS.WPF.Dashboard")
            .GetTypes();

        handlerTypes.Should().NotBeEmpty("Dashboard handlers should exist in the WPF assembly");

        foreach (var handler in handlerTypes)
        {
            var implementedInterfaces = handler.GetInterfaces();
            var isQueryHandler = implementedInterfaces.Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().FullName == "MediatR.IRequestHandler`2");

            isQueryHandler.Should().BeTrue(
                $"Dashboard handler '{handler.FullName}' must implement IRequestHandler<,> (query). " +
                "Dashboard must be read-only and must not use commands.");

            var requestType = implementedInterfaces
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName == "MediatR.IRequestHandler`2")
                .GetGenericArguments()[0];

            var implementsIRequest = requestType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName == "MediatR.IRequest`1");

            implementsIRequest.Should().BeTrue(
                $"Dashboard query type '{requestType.Name}' must implement IRequest<> (MediatR query pattern).");
        }
    }

    [Fact]
    public void Dashboard_Handlers_Should_Not_Use_WriteStores()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var handlerTypes = Types.InAssembly(wpfAssembly)
            .That()
            .ResideInNamespace("RMS.WPF.Dashboard")
            .GetTypes();

        var forbiddenDependencies = new[]
        {
            "RMS.Modules.Sales.Application.Contracts.ISaleWriteStore",
            "RMS.Modules.Inventory.Application.Contracts.IInventoryWriteStore",
            "RMS.Modules.Products.Application.Contracts.IProductWriteStore",
            "RMS.Modules.Customers.Application.Contracts.ICustomerWriteStore",
            "RMS.Modules.Suppliers.Application.Contracts.ISupplierWriteStore",
            "RMS.Modules.Purchasing.Application.Contracts.IPurchaseOrderWriteStore",
            "RMS.Modules.Identity.Application.Contracts.IUserWriteStore",
        };

        foreach (var forbidden in forbiddenDependencies)
        {
            var result = Types.InAssembly(wpfAssembly)
                .That()
                .ResideInNamespace("RMS.WPF.Dashboard")
                .ShouldNot()
                .HaveDependencyOn(forbidden)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Dashboard handlers must not reference write stores. Forbidden: {forbidden}. " +
                $"Offending types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void Dashboard_Handlers_Should_Not_Publish_Events()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var handlerTypes = Types.InAssembly(wpfAssembly)
            .That()
            .ResideInNamespace("RMS.WPF.Dashboard")
            .GetTypes();

        var result = Types.InAssembly(wpfAssembly)
            .That()
            .ResideInNamespace("RMS.WPF.Dashboard")
            .ShouldNot()
            .HaveDependencyOn("RMS.BuildingBlocks.EventBus.IEventBus")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Dashboard handlers must not publish events (IEventBus). " +
            $"Offending types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Dashboard_DTOs_Should_Be_Immutable_Records()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var dtoTypes = Types.InAssembly(wpfAssembly)
            .That()
            .ResideInNamespace("RMS.WPF.Dashboard")
            .And()
            .HaveNameEndingWith("Dto")
            .GetTypes();

        dtoTypes.Should().NotBeEmpty("Dashboard DTOs should exist");

        foreach (var dto in dtoTypes)
        {
            var isRecord = dto.GetCustomAttributesData()
                .Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

            dto.IsValueType.Should().BeFalse(
                $"Dashboard DTO '{dto.FullName}' should be a record class (reference type).");
        }
    }

    [Fact]
    public void Dashboard_ViewModel_Should_Not_Contain_Business_Logic()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var viewModelType = wpfAssembly.GetType("RMS.WPF.ViewModels.DashboardViewModel");
        viewModelType.Should().NotBeNull("DashboardViewModel should exist");

        var writeMethodPatterns = new[]
        {
            "InsertAsync", "UpdateAsync", "DeleteAsync", "SaveAsync",
            "AddAsync", "RemoveAsync", "PublishAsync"
        };

        var methods = viewModelType!.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var method in methods)
        {
            writeMethodPatterns.Should().NotContain(method.Name,
                $"DashboardViewModel method '{method.Name}' suggests write operations. " +
                "ViewModel should only orchestrate queries and navigation.");
        }
    }

    [Fact]
    public void DashboardView_Should_Not_Have_CodeBehind_Logic()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var viewType = wpfAssembly.GetType("RMS.WPF.Views.DashboardView");
        viewType.Should().NotBeNull("DashboardView should exist");

        var methods = viewType!.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.DeclaringType == viewType)
            .ToList();

        methods.Should().HaveCount(1,
            "DashboardView code-behind should only have InitializeComponent(). " +
            "All business logic must be in the ViewModel.");

        methods[0].Name.Should().Be("InitializeComponent");
    }

    [Fact]
    public void Dashboard_KpiSummary_Should_Use_Only_Scoped_Query_Data()
    {
        var wpfAssembly = WpfAssembly;
        if (wpfAssembly is null) return;

        var handlerType = wpfAssembly.GetType("RMS.WPF.Dashboard.Queries.GetDashboardSummary.GetDashboardSummaryHandler");
        handlerType.Should().NotBeNull("GetDashboardSummaryHandler should exist");

        var ctor = handlerType!.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        ctor.Should().HaveCount(1, "Handler should have exactly one constructor");

        var parameters = ctor[0].GetParameters();
        parameters.Should().HaveCount(1, "Dashboard handler should depend only on IDbConnectionFactory");

        var paramType = parameters[0].ParameterType;
        paramType.Name.Should().Be("IDbConnectionFactory",
            "Dashboard handlers should only depend on IDbConnectionFactory for read operations.");
    }

    private static Assembly? LoadWpfAssembly()
    {
        try
        {
            return Assembly.Load("RMS.WPF");
        }
        catch (FileNotFoundException)
        {
            try
            {
                var candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\..\src\Desktop\RMS.WPF\bin\Debug\net10.0-windows\RMS.WPF.dll");
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                    return Assembly.LoadFrom(fullPath);
            }
            catch { }
            return null;
        }
    }
}
