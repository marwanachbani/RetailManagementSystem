# NuGet Packages per Project

## src/BuildingBlocks/RMS.BuildingBlocks.csproj
- MediatR
- FluentValidation
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Dapper
- Microsoft.Data.Sqlite
- Serilog
- Serilog.Extensions.Logging
- Serilog.Sinks.File

## src/Modules/{Module}/Domain/RMS.Modules.{Module}.Domain.csproj
- (none — Domain stays dependency-free except ProjectReference to BuildingBlocks)

## src/Modules/{Module}/Application/RMS.Modules.{Module}.Application.csproj
- MediatR
- FluentValidation
- Microsoft.Extensions.DependencyInjection.Abstractions

## src/Modules/{Module}/Infrastructure/RMS.Modules.{Module}.Infrastructure.csproj
- Dapper
- FluentMigrator.Runner

## src/Desktop/RMS.WPF/RMS.WPF.csproj
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Microsoft.Data.Sqlite
- FluentMigrator.Runner
- Serilog / Serilog.Extensions.Logging / Serilog.Sinks.File
- QuestPDF
- BCrypt.Net-Next

## tests/UnitTests/RMS.UnitTests.csproj
- xunit, xunit.runner.visualstudio
- FluentAssertions
- Microsoft.NET.Test.Sdk

## tests/IntegrationTests/RMS.IntegrationTests.csproj
- xunit, xunit.runner.visualstudio
- FluentAssertions
- Microsoft.NET.Test.Sdk
- Microsoft.Data.Sqlite
- FluentMigrator.Runner

## tests/ArchitectureTests/RMS.ArchitectureTests.csproj
- xunit, xunit.runner.visualstudio
- FluentAssertions
- NetArchTest.Rules
- Microsoft.NET.Test.Sdk
- ProjectReference to every module's Domain/Application/Infrastructure project
  (architecture tests are the one place allowed to reference everything,
  since their entire job is to police the boundaries)

---

## Project creation commands (PowerShell / dotnet CLI, run from repo root)

```powershell
dotnet new sln -n RetailManagementSystem

# BuildingBlocks
dotnet new classlib -n RMS.BuildingBlocks -o src/BuildingBlocks -f net10.0

# Modules (repeat per module: Sales, Inventory, Products, Customers, Suppliers, Identity, Reporting)
dotnet new classlib -n RMS.Modules.Sales.Domain -o src/Modules/Sales/Domain -f net10.0
dotnet new classlib -n RMS.Modules.Sales.Application -o src/Modules/Sales/Application -f net10.0
dotnet new classlib -n RMS.Modules.Sales.Infrastructure -o src/Modules/Sales/Infrastructure -f net10.0

# Desktop
dotnet new wpf -n RMS.WPF -o src/Desktop/RMS.WPF -f net10.0-windows

# Tests
dotnet new xunit -n RMS.UnitTests -o tests/UnitTests -f net10.0
dotnet new xunit -n RMS.IntegrationTests -o tests/IntegrationTests -f net10.0
dotnet new xunit -n RMS.ArchitectureTests -o tests/ArchitectureTests -f net10.0

# Add all projects to the solution
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }

# Wire references (example for Sales — repeat per module)
dotnet add src/Modules/Sales/Domain reference src/BuildingBlocks
dotnet add src/Modules/Sales/Application reference src/Modules/Sales/Domain src/BuildingBlocks
dotnet add src/Modules/Sales/Infrastructure reference src/Modules/Sales/Application src/Modules/Sales/Domain src/BuildingBlocks

# Desktop references every module's three layers + BuildingBlocks
dotnet add src/Desktop/RMS.WPF reference src/BuildingBlocks `
  src/Modules/Sales/Domain src/Modules/Sales/Application src/Modules/Sales/Infrastructure `
  src/Modules/Inventory/Domain src/Modules/Inventory/Application src/Modules/Inventory/Infrastructure `
  src/Modules/Products/Domain src/Modules/Products/Application src/Modules/Products/Infrastructure `
  src/Modules/Customers/Domain src/Modules/Customers/Application src/Modules/Customers/Infrastructure `
  src/Modules/Suppliers/Domain src/Modules/Suppliers/Application src/Modules/Suppliers/Infrastructure `
  src/Modules/Identity/Domain src/Modules/Identity/Application src/Modules/Identity/Infrastructure `
  src/Modules/Reporting/Domain src/Modules/Reporting/Application src/Modules/Reporting/Infrastructure

# Architecture tests reference everything (so they can police it)
dotnet add tests/ArchitectureTests reference src/BuildingBlocks <every module's three layers...>

dotnet add tests/UnitTests reference src/BuildingBlocks
dotnet add tests/IntegrationTests reference src/BuildingBlocks
```

> Note: this repo's solution file (`RetailManagementSystem.sln`) and every `.csproj`
> were already generated for you — these commands are here so you understand
> (and can reproduce/extend) the structure on a machine with the .NET 10 SDK,
> since this archive was built without a live SDK available.
