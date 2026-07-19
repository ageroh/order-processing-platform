# Implementation Backlog

This backlog is the handover path for a delivery team. It intentionally does not claim the platform is complete.

## Current Skeleton

Implemented foundation:

- modular monolith solution structure
- API and Worker hosts
- Orders module with representative domain model
- EF Core/PostgreSQL Orders persistence mapping
- module contract shells for Catalog, Inventory, Pricing, Payments, and Shipping
- MassTransit abstraction wiring
- OpenTelemetry wiring
- Docker and Docker Compose baseline
- GitHub Actions CI
- architecture, module, and integration test foundations
- diagrams and ADRs

## Recommended MVP Path

1. Add application boundary contracts:
   - `CreateOrderCommand`
   - `CancelOrderCommand`
   - `GetOrderQuery`
   - `GetOrderLifecycleQuery`
   - command/query result types

2. Define external capability ports:
   - inventory availability check
   - pricing and tax calculation
   - payment authorization
   - shipping initiation

3. Implement a thin vertical create-order path:
   - validate request
   - call deterministic fake ports
   - persist accepted or rejected order
   - write outbox message
   - return HTTP result

4. Add customer journey tests:
   - successful order
   - inventory unavailable
   - payment authorization failure
   - retrieve order
   - permitted cancellation
   - rejected cancellation

5. Implement outbox dispatch:
   - publish through MassTransit
   - add retry policy
   - add failure visibility
   - replace the current Worker heartbeat placeholder with real dispatch and workflow processing

## Open Questions

- Which identity provider should secure the API?
- Which production runtime will host the containers?
- Which MassTransit transport is customer-approved?
- Should inventory be validation-only or reservation-based?
- Which tax/pricing source is authoritative?
- Should payment capture happen immediately or after fulfillment starts?
- Which shipping provider owns tracking callbacks?
- What are the customer isolation and authorization rules?
- What observability backend and alerting standards are required?

## AI-Agent Guidance

AI agents can safely work on:

- adding command/query skeletons inside a module
- adding tests before implementing handlers
- adding fake adapters for deterministic tests
- extending docs and ADRs when decisions change
- adding EF mappings within module-owned schemas

AI agents should not decide without review:

- new cross-module abstractions
- production broker choice
- identity provider choice
- cloud-specific deployment architecture
- module extraction into microservices
- payment or security-sensitive behavior
