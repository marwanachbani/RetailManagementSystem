using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace RMS.ArchitectureTests;

public class ModuleIsolationTests
{
    private static readonly string[] ModuleNames =
        { "Sales", "Inventory", "Products", "Customers", "Suppliers", "Identity", "Reporting", "Purchasing", "Settings", "Audit" };

    public static IEnumerable<object[]> ModulePairs()
    {
        foreach (var a in ModuleNames)
            foreach (var b in ModuleNames)
                if (a != b)
                    yield return new object[] { a, b };
    }

    [Theory]
    [MemberData(nameof(ModulePairs))]
    public void Module_Should_Not_Reference_Other_Module(string moduleA, string moduleB)
    {
        var assembly = LoadModuleAssembly(moduleA);
        if (assembly is null) return; // module not yet implemented this sprint

        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"RMS.Modules.{moduleA}")
            .ShouldNot()
            .HaveDependencyOn($"RMS.Modules.{moduleB}")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"module '{moduleA}' must not reference module '{moduleB}' directly. " +
            $"Offending types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Theory]
    [InlineData("Sales")]
    [InlineData("Inventory")]
    [InlineData("Products")]
    [InlineData("Customers")]
    [InlineData("Suppliers")]
    [InlineData("Identity")]
    [InlineData("Reporting")]
    [InlineData("Settings")]
    [InlineData("Audit")]
    public void Domain_Should_Not_Depend_On_Infrastructure_Or_Application(string moduleName)
    {
        var assembly = LoadDomainAssembly(moduleName);
        if (assembly is null) return;

        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Dapper",
                "Microsoft.Data.Sqlite",
                "PresentationFramework", // WPF
                $"RMS.Modules.{moduleName}.Infrastructure",
                $"RMS.Modules.{moduleName}.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer of '{moduleName}' must stay pure. Offending types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    private static Assembly? LoadModuleAssembly(string moduleName)
    {
        try
        {
            return Assembly.Load($"RMS.Modules.{moduleName}.Domain");
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private static Assembly? LoadDomainAssembly(string moduleName)
    {
        try
        {
            return Assembly.Load($"RMS.Modules.{moduleName}.Domain");
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}
