# Architecture — Dependency Rules

## Allowed References

```
Modules.* (Domain)         -> BuildingBlocks
Modules.* (Application)    -> Modules.*.Domain, BuildingBlocks
Modules.* (Infrastructure) -> Modules.*.Application, Modules.*.Domain, BuildingBlocks
Desktop.RMS.WPF             -> all Modules.*.Application (composition only, via DI),
                                BuildingBlocks
```

## Forbidden References

```
Modules.Sales      -> Modules.Inventory   (FORBIDDEN)
Modules.Inventory   -> Modules.Customers  (FORBIDDEN)
Modules.Customers  -> Modules.Reporting   (FORBIDDEN)
Modules.*.Domain   -> Dapper / SQLite / WPF / any Infrastructure project (FORBIDDEN)
Modules.*.Domain   -> Modules.*.Application (FORBIDDEN, Domain has zero outward deps)
Desktop.RMS.WPF     -> Modules.*.Infrastructure directly (FORBIDDEN — only via DI
                       registration in the Infrastructure's own composition extension,
                       WPF never new()'s a repository/connection itself)
Desktop.RMS.WPF     -> Modules.*.Domain directly for persistence (FORBIDDEN — UI talks
                       to Application layer commands/queries only)
```

## Cross-module communication

Modules NEVER call each other's Application/Infrastructure layers directly.
All cross-module communication goes through:

- **Domain Events** — raised inside an aggregate, dispatched after a successful
  unit of work commit.
- **Integration Events** — published on the in-process `IEventBus`
  (`BuildingBlocks.EventBus`) for other modules' Application-layer handlers to
  subscribe to.

Example flow:

```
Sales.Application.CreateSale.SaleHandler
  -> Sale aggregate raises SaleCompletedDomainEvent
  -> after commit, mapped to SaleCompletedIntegrationEvent
  -> published on IEventBus
  -> Inventory.Application.Handlers.OnSaleCompleted   (decrements stock)
  -> Reporting.Application.Handlers.OnSaleCompleted   (projects daily sales)
  -> Identity/Audit.Application.Handlers.OnSaleCompleted (writes audit trail)
```

No module has a project reference to another module's project. This is verified
by `tests/ArchitectureTests` using `NetArchTest.Rules` / reflection-based checks
that run on every build.

## Hybrid Event Store

- Current-state tables (e.g. `Products`, `StockItems`, `Sales`) hold the
  read/write-optimized current snapshot used by everyday queries.
- An append-only `EventStore` table holds every domain event ever raised:
  `EventId, AggregateId, AggregateType, EventType, Payload (JSON), OccurredOn, Version`.
- This gives us full audit history and the option to rebuild projections or
  add event-sourced aggregates later, without paying the full ES complexity
  tax on day one.

## Vertical Slice layout (per feature)

```
Modules/Sales/Application/CreateSale/
    CreateSaleCommand.cs
    CreateSaleHandler.cs
    CreateSaleValidator.cs
    CreateSaleResultDto.cs
```

Each slice is self-contained: one file per concern, minimal sharing across
slices except through Domain and BuildingBlocks.
