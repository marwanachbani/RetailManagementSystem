# ADR 0002: Dapper + Explicit SQL over EF Core

## Status
Accepted

## Context
EF Core's change tracking, LINQ-to-SQL translation, and migrations model
encourage anemic domain models and hide the real SQL being executed — a poor
fit for a POS system where every stock movement and sale must be auditable,
performant, and predictable.

## Decision
Use Dapper for all data access, with hand-written SQL per query/command,
and FluentMigrator for schema migrations (not EF Core migrations).

## Consequences
- Full control over SQL, indexes, and query plans against SQLite.
- No Repository/Unit-of-Work abstraction — each vertical slice's Handler owns
  its persistence call via a thin `IDbConnectionFactory` (BuildingBlocks),
  using Dapper directly, wrapped in an explicit transaction where multiple
  writes need atomicity.
- More boilerplate SQL than EF Core, but in return: predictable performance,
  no hidden N+1 queries, and domain model design is driven by business rules
  rather than ORM mapping ergonomics.
