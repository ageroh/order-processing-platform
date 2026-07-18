# Design Memory

Detailed architecture notes for the Order Processing Platform. This file records the current decisions and implementation plan; it avoids repeating questions that have already been answered.

## Current Position

The platform is a greenfield, cloud-neutral modular monolith built with .NET 10 and EF Core 10.

The application is packaged as two portable Docker images:

- `OrderProcessing.Api`
- `OrderProcessing.Worker`

The final production runtime platform is unknown. The images should be able to run later on Azure Container Apps, AWS ECS/Fargate, Kubernetes, Docker on virtual machines, private cloud, or another customer-approved platform.

Source control and CI are GitHub-based. GitHub Actions validates restore, build, format, tests, and container image builds.

## Architecture Style

The solution follows an Ardalis.Modulith-style modular monolith:

- one deployable application boundary, currently API plus Worker images
- one implementation project per business module
- one `.Contracts` project per module when public contracts are needed
- module tests per module
- module implementation types are internal by default
- only the module service registrar is public
- architecture tests enforce module public-surface rules

Sources:

- https://github.com/ardalis/modulith
- https://ardalis.com/introducing-modular-monoliths-goldilocks-architecture/

No generic `BuildingBlocks` projects are kept at this stage. Shared domain/application abstractions should stay inside modules until at least two modules need the same concept and the abstraction has a stable meaning.

## Confirmed Decisions

- Language/runtime: .NET 10.
- Data access: EF Core 10.
- Database: PostgreSQL by default.
- API style: ASP.NET Core controllers, not minimal APIs.
- API documentation: Swagger/OpenAPI.
- Source control: GitHub.
- CI: GitHub Actions.
- Deployment artifact: portable Docker images.
- Local environment: Docker Compose.
- Customer context: greenfield.
- Architecture style: modular monolith.
- Module style: Ardalis.Modulith-inspired implementation project + contracts project + tests.
- Shared building blocks: not used initially.
- Messaging abstraction: MassTransit.
- Initial messaging transport: in-memory for fast feedback.
- Production messaging transport: deferred.
- Observability: OpenTelemetry from the beginning.
- Testing: xUnit with built-in assertions; no FluentAssertions.
- Integration testing: Testcontainers for realistic infrastructure scenarios.
- Event approach: event-driven integration patterns, not event sourcing.
- CQRS: command/query separation at application level, not separate read/write stores by default.

## Order Decisions

- Order state is authoritative in the Orders module.
- Order creation is a workflow, not CRUD.
- Orders are accepted only after inventory availability, pricing, and payment authorization succeed.
- Inventory behavior defaults to availability validation before acceptance.
- Inventory reservation is an extension point, not part of the first domain slice.
- Payment behavior defaults to authorization during order creation and capture later.
- Cancellation is full-order cancellation only.
- Initial order states are:
  - `Pending`
  - `Accepted`
  - `Cancelled`
  - `Rejected`

Future fulfillment states may be added later:

- `ReadyForFulfillment`
- `FulfillmentInProgress`
- `Shipped`
- `Completed`

## Current Solution Shape

```text
OrderProcessingPlatform.slnx

src/
  OrderProcessing.Api/
  OrderProcessing.Worker/
  Modules/
    Orders/
      OrderProcessing.Modules.Orders/
        Controllers/
        Domain/
        HttpModels/
        Outbox/
        Persistence/
        OrdersModuleServiceRegistrar.cs
      OrderProcessing.Modules.Orders.Contracts/
    Catalog/
      OrderProcessing.Modules.Catalog.Contracts/
    Inventory/
      OrderProcessing.Modules.Inventory.Contracts/
    Pricing/
      OrderProcessing.Modules.Pricing.Contracts/
    Payments/
      OrderProcessing.Modules.Payments.Contracts/
    Shipping/
      OrderProcessing.Modules.Shipping.Contracts/

tests/
  OrderProcessing.Modules.Orders.Tests/
  OrderProcessing.IntegrationTests/
  OrderProcessing.Architecture.Tests/

.github/
  workflows/
    ci.yml

infra/
  README.md
```

## Module Responsibilities

Orders:

- order aggregate
- order lifecycle
- cancellation rules
- order history
- order persistence
- order integration events

Catalog:

- product identity
- product status
- product sellability
- product snapshots for order use

Inventory:

- availability validation
- future reservation contract
- external inventory adapter

Pricing:

- price calculation
- tax calculation
- additional charges
- accepted price snapshot

Payments:

- payment authorization
- future capture
- provider callbacks
- idempotency

Shipping:

- future shipment creation
- carrier adapters
- tracking callbacks

## Runtime Components

```text
Client
  -> OrderProcessing.Api
  -> Orders module
  -> PostgreSQL
  -> Outbox
  -> OrderProcessing.Worker
  -> external inventory/payment/shipping adapters
```

The API handles synchronous request/response operations. The Worker handles asynchronous integration, outbox dispatch, retries, and future long-running workflows.

## Synchronous Path

Order creation should synchronously:

- validate the request
- check product sellability
- calculate pricing and taxes
- validate inventory availability
- authorize payment
- persist order state
- write an outbox message in the same transaction

This gives the caller a clear accepted/rejected result.

## Asynchronous Path

Asynchronous processing should handle:

- outbox dispatch
- notifications
- ERP synchronization
- analytics
- shipping initiation
- payment capture if capture remains delayed
- provider callbacks

## Messaging

MassTransit is used as the messaging abstraction so business modules do not depend directly on broker SDKs.

Current transport:

- MassTransit in-memory for local/test feedback.

Deferred production transport options:

- RabbitMQ
- Azure Service Bus
- Amazon SQS
- ActiveMQ or enterprise transport
- customer-standard platform transport

Kafka/EventBridge should be considered only if the system needs event streaming, replay, or enterprise event routing.

## Data Ownership

Each module owns its persistence schema.

Initial Orders schema direction:

```text
orders.orders
orders.order_lines
orders.order_lifecycle_events
orders.outbox_messages
```

Rules:

- no module directly modifies another module's tables
- cross-module communication uses contracts, application services, events, or read models
- Orders stores product and price snapshots for historical correctness

## Reliability

Initial reliability patterns:

- transactional persistence for order state
- outbox for outgoing integration events
- inbox later for incoming message deduplication
- idempotency keys for retried commands
- optimistic concurrency for order state changes
- retry with exponential backoff for transient provider failures
- dead-letter handling for poison messages

## Observability

OpenTelemetry is configured from the beginning.

Signals:

- traces
- metrics
- structured logs through normal .NET logging
- correlation IDs

Important trace boundaries:

- API request
- module controller/action
- command handler
- EF Core transaction
- outbox write
- worker dispatch
- external provider adapter

Exporter choice is platform-dependent. Local development can use console export or a future OpenTelemetry collector container.

## Security

Initial security posture:

- OAuth2/OIDC-ready authentication
- authorization policies for order access and cancellation
- customer isolation checks
- no raw payment card data
- TLS
- input validation at the API boundary
- audit-friendly lifecycle records
- validated external webhooks
- secrets stored in an approved secret store

Full identity provider selection is deferred.

## Testing Strategy

Testing is part of the architecture because fast AI-assisted implementation needs executable guardrails.

Initial test types:

- module domain tests
- architecture tests for module boundaries
- API integration tests with `WebApplicationFactory`
- Testcontainers-based PostgreSQL tests when enabled
- future customer journey tests

No FluentAssertions is used due to licensing concerns. Use test-framework built-in assertions.

Initial customer journey scenarios:

- create order successfully
- reject order when inventory is unavailable
- retrieve existing order
- cancel order while permitted
- reject cancellation when not permitted
- dispatch outbox event

## CI/CD

GitHub Actions is the CI platform.

The workflow should run:

- `dotnet restore`
- `dotnet build --no-restore`
- `dotnet format --verify-no-changes`
- `dotnet test --no-build`
- API Docker image build
- Worker Docker image build

CD is deferred until the production runtime platform is known. The first deployability milestone is validated Docker images.

## Slice Plan

### Slice 1: Foundation

Status: implemented.

Purpose:

- establish the solution shape
- enforce module boundaries early
- make the API and Worker deployable as containers
- add CI and test guardrails before feature implementation

Implemented artifacts:

- `OrderProcessingPlatform.slnx`
- `OrderProcessing.Api`
- `OrderProcessing.Worker`
- Orders implementation module
- Orders contracts project
- contract-only shells for Catalog, Inventory, Pricing, Payments, and Shipping
- architecture tests
- integration tests
- module tests
- API and Worker Dockerfiles
- Docker Compose
- GitHub Actions CI
- PostgreSQL EF Core setup
- MassTransit in-memory setup
- OpenTelemetry setup
- module-owned Orders controller
- outbox placeholder

Verification:

```text
dotnet restore OrderProcessingPlatform.slnx
dotnet build OrderProcessingPlatform.slnx --no-restore
dotnet test OrderProcessingPlatform.slnx --no-build
dotnet format OrderProcessingPlatform.slnx --verify-no-changes --no-restore
```

Docker image validation is configured in GitHub Actions but was not run locally because Docker is not installed in this environment.

Requirements covered:

- modularity
- maintainability
- testability
- observability foundation
- deployment readiness
- extensibility foundation

### Slice 2: Orders Domain

Purpose:

- implement the core order business model and rules before persistence/application plumbing grows around it

Scope:

- `Order` aggregate
- `OrderLine`
- lifecycle records
- creation invariants
- full-order cancellation policy
- domain events
- module tests for accepted, rejected, and cancelled flows

Requirements covered:

- creating an order containing one or more products
- cancelling an order when permitted
- tracking the lifecycle of an order
- testability
- maintainability

### Slice 3: Orders Persistence

Purpose:

- persist the Orders module state with EF Core/PostgreSQL and prove mappings with realistic tests

Scope:

- order tables
- order line tables
- lifecycle event table
- outbox table
- EF Core configurations
- optimistic concurrency token
- Orders schema ownership
- Testcontainers-backed PostgreSQL integration tests

Requirements covered:

- retrieving an existing order
- tracking lifecycle
- reliability
- scalability foundation

### Slice 4: Application Use Cases

Purpose:

- implement command/query flows and external capability ports

Scope:

- `CreateOrderCommand`
- `GetOrderQuery`
- `CancelOrderCommand`
- `GetOrderLifecycleQuery`
- inventory availability port
- pricing/tax port
- payment authorization port
- shipping trigger port
- deterministic fake adapters for tests

Requirements covered:

- creating an order
- retrieving an order
- cancelling an order
- validating inventory availability
- calculating pricing, taxes, and additional charges
- integrating with external inventory, payment, and shipping systems through ports

### Slice 5: API Completion

Purpose:

- replace placeholder controller responses with real application use cases

Scope:

- `POST /orders`
- `GET /orders/{orderId}`
- `POST /orders/{orderId}/cancel`
- `GET /orders/{orderId}/lifecycle`
- request validation
- problem details
- idempotency header handling
- correlation headers
- Swagger operation documentation

Requirements covered:

- functional HTTP API
- security foundation
- observability foundation
- reliability through idempotent command handling

### Slice 6: Messaging and Worker

Purpose:

- make async integration reliable without choosing a production broker prematurely

Scope:

- integration event contracts
- outbox dispatcher
- MassTransit publisher
- in-memory transport for initial tests
- inbox placeholder
- retry/dead-letter strategy placeholders
- worker tests

Requirements covered:

- external system integration
- reliability
- scalability
- observability
- extensibility

### Slice 7: Customer Journey Tests

Purpose:

- protect the MVP behavior with executable business scenarios

Scope:

- successful order creation
- inventory unavailable rejection
- payment authorization failure
- retrieve order
- cancel permitted order
- reject cancellation when not permitted
- outbox event persisted
- worker dispatches event

Requirements covered:

- all functional requirements at scenario level
- testability
- reliability
- maintainability

### Slice 8: Handover Artifacts

Purpose:

- make the skeleton easy for a delivery team to continue

Scope:

- ADR for modular monolith
- ADR for MassTransit transport abstraction
- ADR for Testcontainers-first integration tests
- ADR for OpenTelemetry
- ADR for GitHub Actions CI
- architecture diagram
- module dependency rules
- future extraction guidance
- deployment contract

Requirements covered:

- maintainability
- extensibility
- deployment
- architectural communication

## Remaining Decisions

- final production runtime platform
- final MassTransit production transport
- identity provider
- final observability backend
- exact provider contracts for inventory, payment, tax, and shipping
- whether any module later becomes an independently deployed service
- whether a shared kernel becomes justified after more modules exist

## AI Usage

AI can help scaffold, refactor, and generate tests quickly. Human judgment remains responsible for module boundaries, consistency decisions, deployment assumptions, and deciding when abstractions are justified.
