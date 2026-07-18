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
- The production runtime platform is unknown.
- The first deployment contract is portable Docker images for the API and Worker.
- Those containers should be able to run later on Azure Container Apps, AWS ECS/Fargate, Kubernetes, Docker on virtual machines, private cloud, or another customer-approved platform.
- The initial runtime shape should include:
  - `OrderProcessing.Api` for synchronous HTTP APIs.
  - `OrderProcessing.Worker` for asynchronous processing, outbox dispatch, retries, and integration workflows.
- The application should run locally with Docker Compose.
- The customer context is assumed to be greenfield.
- Source control and CI should be hosted in GitHub.
- GitHub Actions should validate every pull request before merge.
- PostgreSQL is the default relational database, with schema boundaries per module.
- EF Core should be used behind module-owned persistence boundaries.
- External inventory, payment, tax, and shipping systems should be integrated through ports and adapters.
- Messaging should be implemented through MassTransit so transport details stay outside business modules.
- The concrete message transport is an infrastructure decision and should be replaceable.
- Business modules should not reference broker SDKs directly.
- Order state is authoritative in the Orders module.
- Order creation is a workflow, not simple CRUD.
- Orders should be accepted only after inventory availability, pricing, and payment authorization succeed.
- The initial order lifecycle uses four states: `Pending`, `Accepted`, `Cancelled`, and `Rejected`.
- Cancellation is full-order cancellation only.
- Cancellation rules belong in the domain model.
- Inventory availability validation, pricing, payment, and shipping are separate capabilities with clear contracts.
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
  Modules/
    Orders/
      OrderProcessing.Modules.Orders/
      OrderProcessing.Modules.Orders.Contracts/
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

## Architecture Diagrams

The architecture diagrams are in [docs/diagrams/architecture-diagrams.md](docs/diagrams/architecture-diagrams.md).

They include a simple system design overview and the order lifecycle.

## Initial API Surface

```http
POST /orders
GET /orders/{orderId}
POST /orders/{orderId}/cancel
GET /orders/{orderId}/lifecycle
```

## Remaining Decisions

- Final MassTransit production transport.
- Production runtime platform for the Docker images.
- Identity provider.
- Final observability backend.

## Next Steps

Slice 1 has created the foundation. Slice 2 has added the architecture diagrams. The remaining implementation should proceed in this order:

1. Orders domain model: aggregate, lines, lifecycle, cancellation policy, domain events.
2. Orders persistence: EF Core mappings, order tables, lifecycle table, outbox, PostgreSQL integration tests.
3. Application use cases: create, retrieve, cancel, lifecycle query, inventory/pricing/payment/shipping ports.
4. API completion: replace placeholder controller responses with real use cases and validation.
5. Messaging and Worker: outbox dispatcher, MassTransit publishing, retry/idempotency placeholders.
6. Customer journey tests: successful order, inventory rejection, payment failure, retrieve, cancel, dispatch event.
7. Handover artifacts: ADRs, module rules, deployment contract, future extraction guidance.

See [docs/design-memory.md](docs/design-memory.md) for detailed working notes and architectural reasoning.
