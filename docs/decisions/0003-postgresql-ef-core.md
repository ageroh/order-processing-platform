# ADR 0003: Use PostgreSQL With EF Core

## Status

Accepted.

## Context

Orders require transactional consistency for aggregate state, order lines, lifecycle entries, and outbox records. The platform also needs a persistence model that is familiar to delivery teams and testable with realistic infrastructure.

## Decision

Use PostgreSQL as the default relational database and EF Core as the data access technology.

Each module owns its persistence schema. The Orders module starts with:

- `orders.orders`
- `orders.order_lines`
- `orders.order_lifecycle_events`
- `orders.outbox_messages`

## Consequences

This supports transactional writes, clear data ownership, and realistic integration testing with Testcontainers.

EF Core is kept behind module boundaries so persistence choices do not leak into public contracts.

## Deferred

Detailed migrations, read-model tuning, archival strategy, and production database operations are implementation backlog items.
