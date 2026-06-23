# Dependency Graph — Sprint 0

```
                            ┌─────────────────────┐
                            │   RMS.BuildingBlocks │
                            │  (Result, Domain     │
                            │   primitives,        │
                            │   EventBus, Logging,│
                            │   Contracts,         │
                            │   Exceptions)        │
                            └──────────▲───────────┘
                                       │
        ┌────────────┬────────────────┼────────────────┬────────────┬────────────┐
        │             │                │                │            │            │
   Sales.Domain  Inventory.Domain Products.Domain Customers.Domain Suppliers.Domain Identity.Domain  Reporting.Domain
        ▲             ▲                ▲                ▲            ▲            ▲                ▲
        │             │                │                │            │            │                │
  Sales.Application Inventory.App.  Products.App.   Customers.App. Suppliers.App. Identity.App.   Reporting.App.
        ▲             ▲                ▲                ▲            ▲            ▲                ▲
        │             │                │                │            │            │                │
  Sales.Infra.    Inventory.Infra. Products.Infra.  Customers.Infra. Suppliers.Infra. Identity.Infra. Reporting.Infra.
        ▲             ▲                ▲                ▲            ▲            ▲                ▲
        └────────────┴────────────────┴────────────────┴────────────┴────────────┴────────────────┘
                                       │
                              ┌────────┴────────┐
                              │    RMS.WPF       │
                              │ (composition root│
                              │  wires every     │
                              │  module's DI     │
                              │  extension; talks│
                              │  to Application   │
                              │  layer via        │
                              │  MediatR/handlers)│
                              └───────────────────┘
```

Key rule visualized: every arrow points *up* toward BuildingBlocks or *up*
through its own module's layers. There are **no horizontal arrows** between
modules — that's enforced by `tests/ArchitectureTests`.

Modules talk to each other only through `IEventBus` (defined in
BuildingBlocks, implemented as `InProcessEventBus`), never through direct
project references or service calls.
