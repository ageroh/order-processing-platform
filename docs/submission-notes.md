# Submission Notes

These notes summarize how to present and defend the architecture skeleton.

## What This Submission Is

This repository is an implementation-ready skeleton for an Order Processing Platform. It demonstrates solution structure, module boundaries, architectural decisions, deployability, observability, persistence direction, and testing guardrails.

It is not intended to be a complete order-processing product.

## Architectural Position

The selected architecture is a cloud-neutral, containerized modular monolith.

Key points to defend:

- The business capabilities are visible, but independent service boundaries are not yet proven.
- A modular monolith gives clear boundaries with lower operational complexity than starting with microservices.
- API and Worker containers keep runtime responsibilities separate while preserving a simple deployment model.
- Module `.Contracts` projects expose stable boundaries without leaking module internals.
- EF Core and PostgreSQL are used behind module-owned schemas.
- MassTransit keeps messaging transport choices replaceable.
- OpenTelemetry is wired early because order workflows cross HTTP, database, messaging, and provider boundaries.
- Testcontainers and focused module tests create guardrails for fast AI-assisted implementation.

## Functional Requirement Coverage

Creating an order:

- Represented by the initial `POST /orders` API surface.
- Supported by the Orders domain aggregate and order line invariants.
- Full application workflow is intentionally deferred to delivery backlog.

Retrieving an order:

- Represented by the initial `GET /orders/{orderId}` API surface.
- Supported by EF Core persistence mapping.

Cancelling an order:

- Represented by the initial `POST /orders/{orderId}/cancel` API surface.
- Domain rules support full cancellation for pending or accepted orders.

Tracking lifecycle:

- Supported by `OrderLifecycleEntry` and the `orders.order_lifecycle_events` table mapping.
- Represented by `GET /orders/{orderId}/lifecycle`.

Inventory availability:

- Treated as a separate Inventory module responsibility.
- Application port and provider adapter are deferred to backlog.

Pricing, taxes, and charges:

- Treated as a separate Pricing module responsibility.
- Orders stores an accepted pricing snapshot for historical correctness.

External systems:

- Inventory, payment, and shipping are modeled as module boundaries.
- MassTransit, outbox, and Worker process provide the async integration foundation.

## Non-Functional Requirement Coverage

Modularity:

- Modules are separated under `src/Modules`.
- Implementation types are internal by default; ASP.NET Core controllers are public framework-facing entry points.
- Module public-surface rules are documented and should be reviewed as modules evolve.

Maintainability:

- ADRs record decisions and tradeoffs.
- README stays concise; detailed reasoning lives in `docs/design-memory.md`.

Testability:

- xUnit, Shouldly, integration tests, module tests, and Testcontainers are established.
- Test names follow `GivenX_WhenY_ThenZ`.

Scalability:

- API and Worker are separate deployable containers.
- Async processing and outbox support scaling background workflows independently.
- Future module extraction remains possible.

Reliability:

- Transactional order persistence and outbox mapping are included.
- Retry, inbox, idempotency, and dead-letter handling are documented as implementation backlog.

Security:

- Security posture is documented: OIDC/OAuth2 readiness, authorization policies, no raw card data, TLS, webhook validation, and secret-store usage.
- Final identity provider is deferred.

Observability:

- OpenTelemetry is wired from the beginning in API and Worker hosts.
- Correlation and tracing boundaries are documented.

Deployment:

- API and Worker Dockerfiles exist.
- Docker Compose supports local execution.
- GitHub Actions validates build, test, format, and container image builds.

Extensibility:

- Module contracts, MassTransit abstraction, cloud-neutral containers, and documented backlog keep extension paths open.

## What Was Intentionally Not Fully Implemented

- Full create-order application workflow.
- Real inventory, pricing, payment, and shipping adapters.
- Production message broker configuration.
- Identity provider integration.
- Full outbox dispatcher.
- Full customer journey suite.
- Cloud-specific deployment manifests.

These are delivery backlog items because the assignment is about architecture, decomposition, and implementation readiness.

## AI Usage

AI was used to accelerate scaffolding, documentation, test conversion, and repeated consistency checks.

Human judgment was applied to:

- choose modular monolith over microservices
- avoid a premature `BuildingBlocks` project
- keep the runtime cloud-neutral
- use MassTransit rather than selecting a broker too early
- prioritize Testcontainers and focused tests as guardrails
- stop at skeleton depth instead of implementing a full product

## Critique

Strengths:

- Clear module boundaries.
- Good delivery-team handover artifacts.
- Early test, observability, CI, and container foundations.
- Representative domain and persistence depth without implementing every use case.

Tradeoffs:

- API endpoints are still placeholders.
- Integration module contracts are skeletal.
- Production runtime, broker, identity, and observability backend remain open.
- The domain model is representative and should evolve with real customer rules.

This is acceptable because the skeleton is designed to make those decisions explicit rather than hide them inside incomplete implementation code.
