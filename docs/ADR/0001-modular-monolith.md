# ADR 0001: Modular Monolith over Microservices

## Status
Accepted

## Context
The system is a single-tenant, offline desktop ERP/POS. There is no network
boundary requiring independent deployability, no team-scaling pressure that
justifies service boundaries, and SQLite is a single-file embedded database
unsuited to being split across services.

## Decision
Use a Modular Monolith: one process, one WPF host, multiple independently
designed modules (Sales, Inventory, Products, Customers, Suppliers, Identity,
Reporting) that communicate only through an in-process event bus and never
share project references with each other.

## Consequences
- Simpler deployment: a single Setup.exe installs one executable.
- Module boundaries are enforced by architecture tests, not network/IPC.
- We keep the *option* to extract a module into a separate service later,
  because coupling is already event-driven and one-directional toward
  BuildingBlocks.
- Trade-off: we lose the deployment isolation and independent scaling that
  microservices would give us — but those benefits are irrelevant for a
  single-machine POS terminal.
