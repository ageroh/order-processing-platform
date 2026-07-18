# Order Processing Platform

Architecture skeleton for a modular Order Processing Platform using .NET 10 and EF Core 10.

This repository is intended to become an implementation-ready foundation for a delivery team. The first goal is to agree the architectural direction and assumptions, then build the solution in slices.

## Design Direction

The proposed starting architecture is a cloud-neutral, containerized modular monolith with separate API and background worker processes.

```text
Client
  -> OrderProcessing.Api
  -> Orders module
  -> EF Core persistence
  -> Outbox
  -> OrderProcessing.Worker
  -> External inventory, payment, tax, and shipping adapters
```

This gives us strong module boundaries without introducing distributed-system complexity before the business boundaries, scale needs, and customer infrastructure constraints are confirmed.

## Core Assumptions

- The platform targets .NET 10 and EF Core 10.
- The first implementation should be a modular monolith, not microservices.
- The platform should be packaged as containers but should not depend on a specific cloud provider.
- The initial runtime shape should include:
  - `OrderProcessing.Api` for synchronous HTTP APIs.
  - `OrderProcessing.Worker` for asynchronous processing, outbox dispatch, retries, and integration workflows.
- The application should run locally with Docker Compose.
- The production runtime could be Kubernetes, managed containers, virtual machines, private cloud, public cloud, or another customer-approved platform.
- Source control and CI should be hosted in GitHub.
- GitHub Actions should validate every pull request before merge.
- The database should be relational, with schema boundaries per module.
- EF Core should be used behind module-owned persistence boundaries.
- External inventory, payment, tax, and shipping systems should be integrated through ports and adapters.
- Messaging should be implemented through MassTransit so transport details stay outside business modules.
- The concrete message transport is an infrastructure decision and should be replaceable.
- Business modules should not reference broker SDKs directly.
- Order state is authoritative in the Orders module.
- Order creation is a workflow, not simple CRUD.
- Cancellation rules belong in the domain model.
- Inventory validation, pricing, payment, and shipping are separate capabilities with clear contracts.
- Reliability should be designed around idempotency, retries, outbox/inbox patterns, and observable workflows.
- Automated tests are a first-class architecture boundary because most implementation work is expected to be produced quickly by AI-assisted agents.
- Testcontainers should be used early for realistic database, messaging, and API integration scenarios.
- OpenTelemetry should be wired from the beginning for traces, metrics, logs, and correlation across API, worker, database, messaging, and external adapters.

## Proposed Modules

```text
Orders
  Owns order aggregate, lifecycle, cancellation rules, order history, and order persistence.

Catalog
  Owns product identity, product status, and sellability rules.

Inventory
  Owns availability and reservation contracts, plus inventory-provider adapters.

Pricing
  Owns pricing, tax, additional charges, and price breakdown contracts.

Payments
  Owns payment authorization, capture placeholders, provider integration, and callbacks.

Shipping
  Owns shipment creation, carrier integration placeholders, and tracking callbacks.
```

## Expected Solution Shape

```text
src/
  OrderProcessing.Api/
  OrderProcessing.Worker/
  BuildingBlocks/
  Modules/
    Orders/
    Catalog/
    Inventory/
    Pricing/
    Payments/
    Shipping/

tests/
  OrderProcessing.Modules.Orders.Tests/
  OrderProcessing.IntegrationTests/
  OrderProcessing.Architecture.Tests/

.github/
  workflows/
    ci.yml

docs/
  design-memory.md
  decisions/
  diagrams/

infra/
  README.md
```

## Initial API Surface

```http
POST /orders
GET /orders/{orderId}
POST /orders/{orderId}/cancel
GET /orders/{orderId}/lifecycle
```

## Key Open Decisions

- Which database is preferred: PostgreSQL, SQL Server, or customer standard?
- Which MassTransit transport is preferred: in-memory for local tests, RabbitMQ, Azure Service Bus, AWS SQS, an enterprise transport, or no external broker initially?
- Is the customer greenfield or brownfield?
- Is inventory checked, reserved, decremented, or handled externally?
- Is payment authorized during order creation or handled asynchronously?
- Which order states are required by the business?
- Is cancellation full only, or partial as well?
- Which runtime platform is expected in production?

## Next Slice

The next slice should create the .NET solution skeleton, starting with:

- API project
- Worker project
- shared building blocks
- Orders module
- representative order aggregate
- EF Core Orders DbContext
- outbox entity
- Testcontainers-based integration test foundation
- focused tests for order creation, retrieval, cancellation, outbox persistence, and lifecycle tracking
- OpenTelemetry bootstrap for API and Worker
- GitHub Actions CI workflow for restore, build, test, architecture checks, and container build validation

See [docs/design-memory.md](docs/design-memory.md) for detailed working notes and architectural reasoning.
