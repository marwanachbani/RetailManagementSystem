# Retail Management System (RMS)

Production-grade, 100% offline Windows Desktop ERP/POS platform.

WPF + .NET 10 + SQLite + Dapper + FluentMigrator + Serilog + QuestPDF + BCrypt.

Architecture: DDD, Modular Monolith, Vertical Slice Architecture, CQRS,
Event-Driven communication via an in-process Event Bus, Hybrid Event Store.

See `docs/Architecture` for the dependency rules and `docs/ADR` for decision records.

## Sprint 0 — Scope

Sprint 0 establishes the skeleton only: solution structure, project references,
NuGet packages, BuildingBlocks primitives (Result, DomainEvent, IEventBus,
exceptions), the bootstrap host (DI + Serilog + FluentMigrator runner) and a
minimal WPF shell that boots through that host. No business features yet —
those start in Sprint 1 (Identity module: Login + User aggregate).

## Build

```
dotnet restore
dotnet build
dotnet test
```

Run the desktop app:

```
dotnet run --project src/Desktop/RMS.WPF/RMS.WPF.csproj
```
