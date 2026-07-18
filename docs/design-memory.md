# Design Memory

This file captures the detailed architecture reasoning behind the Order Processing Platform. It is intentionally more detailed than the README so we can review assumptions, critique decisions, and refine the design before and during implementation.

## Current Architectural Position

The current recommendation is a cloud-neutral modular monolith, packaged as containers, with separate API and worker processes.

This is the starting point:

```text
OrderProcessing.Api
  Handles synchronous HTTP requests.
  Performs authentication and authorization.
  Validates request contracts.
  Calls module application services.
  Returns command results and query responses.

OrderProcessing.Worker
  Dispatches outbox messages.
  Processes inbound integration messages.
  Handles retries and delayed workflows.
  Calls external providers when work is not required in the user-facing request path.

Relational database
  Stores module-owned data.
  Stores outbox and inbox records.
  Stores order lifecycle history and audit-friendly state changes.

Messaging transport
  Infrastructure choice, hidden behind MassTransit configuration.
  Used for integration events and asynchronous workflows.
```

The design should avoid cloud-specific assumptions in business code. Infrastructure choices belong at the composition and deployment boundary.

## AI-Agent-Assisted Delivery Implication

If most implementation work is expected to be produced quickly by AI agents, the architecture must provide executable guardrails from the beginning.

The risk is not lack of code volume. The risk is fast code that slowly breaks boundaries, duplicates patterns, hides integration failures, or changes behavior without anyone noticing.

Therefore, the early foundation should prioritize:

- architecture tests that enforce module dependency rules
- domain tests for business invariants
- Testcontainers integration tests using real infrastructure dependencies
- scenario tests that represent customer journeys
- OpenTelemetry from the first slice
- consistent contracts and adapter interfaces
- generated-code-friendly conventions
- small vertical slices that agents can implement safely

The design should help agents move fast without making the system incoherent.

## Why Modular Monolith First

The platform has several natural business capabilities:

- Orders
- Catalog
- Inventory
- Pricing
- Payments
- Shipping

These look like future service candidates, but starting with separately deployed microservices too early creates complexity before the domain seams are proven.

Starting as a modular monolith gives us:

- one repository
- simpler local development
- easier refactoring while the domain is still evolving
- local transactions within module boundaries
- fewer operational moving parts
- clear future extraction paths

This is not a single-layer CRUD application. The modular monolith must enforce boundaries through projects, namespaces, contracts, tests, and data ownership rules.

## When to Split Into Independently Deployed Services

A module should become a separate deployable application only when there is a strong reason:

- different scaling profile
- independent team ownership
- different release cadence
- strong security or compliance isolation requirement
- mature data ownership boundary
- integration workload that should be isolated from order creation
- failure isolation becomes more valuable than transaction simplicity

Likely future split candidates:

- Payments
- Shipping
- Inventory integration
- Pricing/tax calculation
- Order reporting/read models

## Greenfield vs Brownfield Thinking

Greenfield customer:

- choose simple, well-supported technologies
- prefer managed infrastructure if allowed
- optimize for delivery speed and operational clarity
- avoid unnecessary distributed architecture
- keep provider-specific choices behind adapters

Brownfield customer:

- discover existing database, broker, identity, observability, CI/CD, and hosting standards
- integrate with existing operational ownership
- avoid introducing Kafka, Kubernetes, or a new cloud platform unless the benefits justify the adoption cost
- wrap legacy or enterprise systems behind ports/adapters
- document coexistence and migration strategy

Questions to confirm:

- Is this replacing an existing order system?
- Does the customer already have an ERP, WMS, PIM, payment provider, or ESB?
- Are there mandatory platform services?
- Who operates production infrastructure?
- Are managed services allowed?
- Is on-premises deployment required?

## System Design Method

Use a system-design-interview style flow:

1. Clarify functional requirements.
2. Clarify non-functional requirements.
3. Identify core entities and APIs.
4. Define data ownership and consistency boundaries.
5. Choose high-level architecture.
6. Deep dive into risky flows.
7. Discuss bottlenecks and scaling.
8. Explain tradeoffs and deferred choices.

For this exercise, the most important deep dives are:

- order creation
- cancellation
- inventory availability/reservation
- pricing and tax calculation
- payment authorization
- shipping initiation
- async messaging reliability
- provider failure and retries

## Discovery Questions

### Business and Domain

- What order types exist: physical goods, digital goods, subscriptions, services, or mixed baskets?
- Can an order contain products from multiple warehouses or suppliers?
- What are the valid order statuses?
- Who owns each status transition?
- What makes an order cancellable?
- Is cancellation allowed after payment authorization?
- Is cancellation allowed after payment capture?
- Is cancellation allowed after picking, packing, shipment, or delivery?
- Do we support partial cancellation or only full cancellation?
- Do we support returns and refunds?
- Are prices captured at order time?
- Are taxes calculated internally or through a third-party provider?
- Are discounts, promotions, handling fees, customs, or shipping charges in scope?

### Catalog

- What is the product source of truth?
- Is there a local catalog or an external PIM/ERP?
- What product snapshot must be stored on the order?
- Can products be discontinued after an order is placed?
- Are product variants, bundles, or kits in scope?

### Inventory

- Is inventory checked only, reserved, or decremented during order creation?
- If inventory is reserved, when does the reservation expire?
- Can inventory change between validation and payment authorization?
- Does the inventory provider support idempotent reservations?
- Is backordering allowed?
- Can one order be fulfilled by multiple warehouses?
- What happens if inventory provider is unavailable?

### Pricing and Tax

- Is pricing calculated internally or externally?
- Are tax rules country, region, product, or customer-type dependent?
- Are prices tax-inclusive or tax-exclusive?
- Can pricing change after an order is accepted?
- Are price overrides allowed?
- Is a pricing audit trail required?

### Payment

- Is payment authorized during order creation?
- Is payment captured immediately or later?
- Are multiple payment methods allowed?
- Are payment failures recoverable?
- Are refunds in scope?
- What is the PCI boundary?
- Are payment webhooks required?
- Does the provider support idempotency keys?

### Shipping

- Is shipping cost calculated before order acceptance?
- Is shipment creation immediate or delayed?
- Can an order have multiple shipments?
- Are tracking updates required?
- Are carrier callbacks in scope?
- Are delivery confirmation events required?

### Scale and Operations

- Expected orders per day?
- Expected peak orders per second?
- Expected read/write ratio?
- Expected number of concurrent customers?
- Required availability?
- Recovery time objective?
- Recovery point objective?
- Multi-region requirement?
- Data retention period?
- Audit retention period?
- Compliance requirements?

### Platform and Customer Context

- Is this greenfield or brownfield?
- Existing runtime platform?
- Existing broker?
- Existing database?
- Existing identity provider?
- Existing observability stack?
- Existing CI/CD standard?
- Existing infrastructure-as-code standard?
- Public cloud, private cloud, hybrid, or on-premises?

## Synchronous vs Asynchronous Design

Synchronous work should be limited to decisions required before the API can return a meaningful response.

Likely synchronous during order creation:

- validate request
- validate customer access
- check product sellability
- calculate price and tax
- validate or reserve inventory
- authorize payment if the business requires payment before order acceptance
- persist order
- write outbox event in the same transaction

Likely asynchronous after order acceptance:

- shipping arrangement
- customer notification
- analytics
- reporting projection
- ERP synchronization
- payment capture if capture is delayed
- inventory confirmation if provider uses callbacks

Important principle:

Do not synchronously call every external system just because the workflow touches them. Keep the request path small and resilient.

## Queueing and Messaging

MassTransit should be used as the messaging abstraction from the beginning.

The concrete transport should be selected based on customer context and workload characteristics.

Default development recommendation:

- MassTransit in-memory transport for fast local and integration-test feedback where a real broker is not required.
- A real transport through Testcontainers only for scenarios that need transport-specific validation.

Production recommendation:

- Decide after confirming customer standards, workload, replay needs, ordering requirements, and operational ownership.

MassTransit transport options:

- RabbitMQ: good for work queues and routing-based integration events.
- Azure Service Bus: good for Azure customers needing managed queues/topics.
- Amazon SQS: good for AWS customers needing managed queue-based messaging.
- ActiveMQ or other supported enterprise transports: useful in brownfield environments.
- In-memory: useful for tests and local development, not production integration.

Messaging options that may be separate platform decisions:

- Kafka: good for high-throughput streams, replay, and analytics pipelines. It should be chosen only if event streaming is a real requirement, not merely because the platform uses events.
- EventBridge or an enterprise event bus may already exist in brownfield environments. In that case, integrate through an adapter rather than forcing a new transport.

Selection criteria:

- queue vs pub/sub vs event stream
- ordering requirements
- replay requirements
- message volume
- message size
- dead-letter handling
- duplicate detection
- retention
- operational maturity
- cloud/on-premises constraints
- existing customer standards

Application boundary:

```csharp
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
```

Application code can use an internal publishing abstraction or MassTransit interfaces at the composition boundary, but domain code should not know how events are transported.

Infrastructure configures MassTransit with the selected transport. Transport choices can include in-memory, RabbitMQ, Azure Service Bus, Amazon SQS, or a customer-supported transport.

## Reliability Patterns

Outbox:

- Store business state and outgoing messages in the same database transaction.
- A worker dispatches messages later.
- Prevents losing events after a successful database commit.

Inbox:

- Store processed inbound message IDs.
- Make message handlers idempotent.
- Protect against message redelivery.

Idempotency:

- Required for order creation if clients retry.
- Required for payment authorization and callbacks.
- Required for inventory reservation if the provider supports retry semantics.

Retries:

- Use exponential backoff.
- Do not retry non-transient business failures.
- Send poison messages to dead-letter storage.

Circuit breakers:

- Useful around unstable external providers.
- Avoid cascading failures.

Optimistic concurrency:

- Use for order state transitions.
- Prevent conflicting updates to the same order.

## Scaling Model

API:

- stateless
- horizontally scalable
- behind a load balancer or ingress
- no in-memory session state
- request timeouts
- rate limiting
- database connection pooling

Worker:

- scales independently from API
- throughput controlled by queue partitions, competing consumers, and provider rate limits
- must use idempotent handlers
- must handle poison messages

Database:

- module schemas initially
- indexes for order lookup, customer order history, and lifecycle queries
- read replicas for query-heavy workloads
- archival or partitioning for historical orders
- separate databases only when module extraction is justified

Read models:

- useful for order tracking and reporting
- can be built asynchronously
- avoid making the write model serve every query shape

## Data Ownership

Each module owns its persistence schema.

Example:

```text
orders.Orders
orders.OrderLines
orders.OrderLifecycleEvents
orders.OutboxMessages

catalog.Products

inventory.InventoryReservations
inventory.InboxMessages

payments.PaymentAttempts
payments.PaymentTransactions
payments.InboxMessages

shipping.Shipments
shipping.ShipmentEvents
shipping.InboxMessages
```

Rules:

- No module should directly modify another module's tables.
- Cross-module communication should use contracts, application services, events, or read models.
- Orders stores product and price snapshots needed for historical correctness.

## Order Lifecycle Draft

Candidate statuses:

```text
Draft
PendingValidation
Accepted
PaymentAuthorized
ReadyForFulfillment
FulfillmentInProgress
Shipped
Completed
Cancelled
Rejected
```

Representative transitions:

```text
Create order
  -> PendingValidation

Inventory unavailable
  -> Rejected

Inventory available and payment authorized
  -> Accepted
  -> PaymentAuthorized
  -> ReadyForFulfillment

Shipping started
  -> FulfillmentInProgress

Carrier confirms shipment
  -> Shipped

Delivery confirmed or business completion rule met
  -> Completed

Cancellation requested while permitted
  -> Cancelled
```

Cancellation policy should be explicit and tested.

Initial assumption:

- cancellable before fulfillment starts
- not cancellable after shipment
- payment reversal/refund behavior deferred until payment requirements are clarified

## Proposed Solution Structure

```text
OrderProcessingPlatform.sln

src/
  OrderProcessing.Api/
    Program.cs
    Endpoints/
    Contracts/
    Observability/
    Security/

  OrderProcessing.Worker/
    Program.cs
    Outbox/
    Inbox/
    MessageHandlers/

  BuildingBlocks/
    OrderProcessing.BuildingBlocks.Domain/
    OrderProcessing.BuildingBlocks.Application/
    OrderProcessing.BuildingBlocks.Infrastructure/

  Modules/
    Orders/
      OrderProcessing.Modules.Orders.Domain/
      OrderProcessing.Modules.Orders.Application/
      OrderProcessing.Modules.Orders.Infrastructure/
      OrderProcessing.Modules.Orders.Contracts/

    Catalog/
      OrderProcessing.Modules.Catalog.Domain/
      OrderProcessing.Modules.Catalog.Application/
      OrderProcessing.Modules.Catalog.Infrastructure/
      OrderProcessing.Modules.Catalog.Contracts/

    Inventory/
      OrderProcessing.Modules.Inventory.Application/
      OrderProcessing.Modules.Inventory.Infrastructure/
      OrderProcessing.Modules.Inventory.Contracts/

    Pricing/
      OrderProcessing.Modules.Pricing.Application/
      OrderProcessing.Modules.Pricing.Infrastructure/
      OrderProcessing.Modules.Pricing.Contracts/

    Payments/
      OrderProcessing.Modules.Payments.Application/
      OrderProcessing.Modules.Payments.Infrastructure/
      OrderProcessing.Modules.Payments.Contracts/

    Shipping/
      OrderProcessing.Modules.Shipping.Application/
      OrderProcessing.Modules.Shipping.Infrastructure/
      OrderProcessing.Modules.Shipping.Contracts/

tests/
  OrderProcessing.Modules.Orders.Tests/
  OrderProcessing.IntegrationTests/
  OrderProcessing.Architecture.Tests/

docs/
  design-memory.md
  decisions/
  diagrams/

infra/
  README.md
```

## Security Notes

- Use OAuth2/OIDC for authentication.
- Use authorization policies for order access and cancellation.
- Enforce customer isolation.
- Do not store raw card data.
- Store secrets in an approved secret store.
- Use TLS.
- Validate input at the API boundary.
- Audit order state changes.
- Validate external webhooks.
- Apply least-privilege database access.

## Observability Notes

OpenTelemetry should be wired from the beginning in both API and Worker.

Goals:

- trace one order command across API, application handler, EF Core, outbox write, worker dispatch, and external adapter calls
- preserve correlation IDs across HTTP requests and messages
- emit metrics for business and technical health
- make agent-generated workflows debuggable

Initial instrumentation:

- ASP.NET Core instrumentation
- HttpClient instrumentation
- EF Core instrumentation
- runtime metrics
- process metrics
- custom Activities around command handlers
- custom Activities around worker message handlers
- custom metrics for order lifecycle transitions
- custom metrics for outbox lag and failed dispatches

Important fields:

- correlation id
- causation id
- order id
- command name
- event name
- provider name
- retry count
- message id

Local development can export to console or an OpenTelemetry collector container.

Production exporters should be chosen based on customer platform:

- OTLP collector
- Azure Monitor
- AWS X-Ray compatible collector
- Grafana Tempo/Prometheus/Loki
- Datadog
- New Relic
- customer standard

The application should emit OpenTelemetry signals without depending on a specific observability vendor.

Include:

- structured logs
- correlation IDs
- OpenTelemetry traces
- metrics
- health checks
- readiness checks
- worker retry metrics
- dead-letter queue metrics

Important trace boundaries:

- API request
- command handler
- EF Core transaction
- inventory call
- pricing/tax call
- payment call
- outbox write
- worker dispatch
- external provider callback

## Testing Strategy

Testing is a core architectural mechanism, not just a quality activity at the end.

Because implementation may be produced rapidly by AI agents, the test framework should be built before most feature work. The test suite becomes the executable system contract.

### Initial Testing Foundation

Use:

- xUnit or NUnit for .NET tests
- FluentAssertions or Shouldly for readable assertions
- Testcontainers for real PostgreSQL/SQL Server and transport-backed integration tests when needed
- WebApplicationFactory for API bootstrapping where useful
- Respawn or a deterministic database reset strategy between integration tests
- architecture tests to enforce module boundaries
- fake external providers for deterministic customer journeys

Playwright is valuable when there is a user interface or browser-based workflow. For this backend-first skeleton, Testcontainers provides more immediate value because it validates API, EF Core, database, messaging, outbox, and worker behavior with realistic infrastructure.

If a UI or operational portal is added later, Playwright can be introduced for end-to-end browser journeys.

### Priority Tests

Prioritize:

- order aggregate rules
- cancellation policy
- order lifecycle transitions
- pricing snapshot behavior
- command handler behavior with mocked ports
- architecture tests for module dependencies
- outbox persistence behavior
- idempotent message handling
- API-level customer journeys using Testcontainers
- worker processing through MassTransit, using in-memory transport first and a real transport container where transport behavior matters
- observability smoke tests for correlation IDs and trace propagation

Later:

- API tests
- provider contract tests
- load tests
- resilience tests

### Initial Customer Journey Scenarios

Scenario 1: Create order successfully

```text
Given a product is sellable
And inventory is available
And pricing succeeds
And payment authorization succeeds
When the customer creates an order
Then the order is accepted
And the order lines contain product and price snapshots
And an OrderCreated integration event is stored in the outbox
```

Scenario 2: Reject order when inventory is unavailable

```text
Given a product is sellable
And inventory is unavailable
When the customer creates an order
Then the order is rejected or not accepted according to the chosen business rule
And payment authorization is not attempted
And the lifecycle records the inventory failure
```

Scenario 3: Cancel order while permitted

```text
Given an accepted order
And fulfillment has not started
When the customer cancels the order
Then the order is cancelled
And an OrderCancelled integration event is stored in the outbox
```

Scenario 4: Prevent cancellation after fulfillment starts

```text
Given an order in FulfillmentInProgress
When the customer cancels the order
Then cancellation is rejected
And the order state remains unchanged
```

Scenario 5: Dispatch outbox event

```text
Given an order event exists in the outbox
When the worker runs
Then the event is published through MassTransit
And the outbox message is marked as dispatched
```

### Testcontainers Scope

Start with containers for:

- relational database
- message transport only when validating transport-specific behavior

Optional later:

- OpenTelemetry collector
- local object store if documents or labels are introduced
- fake HTTP provider containers for inventory, payment, tax, or shipping

Testcontainers should validate the integration behavior that is most likely to break:

- EF Core mappings
- migrations
- transactions
- outbox write and dispatch
- message publishing
- inbox deduplication
- worker retry behavior
- API-to-database lifecycle

## GitHub Actions CI/CD

The repository will be hosted in GitHub, and GitHub Actions should be part of the initial foundation.

CI is especially important when AI agents generate most of the implementation, because it provides a shared executable contract for every change.

### Pull Request Workflow

Every pull request should run:

- restore
- build
- format check
- unit tests
- architecture tests
- integration tests with Testcontainers
- container image build validation for API
- container image build validation for Worker

Recommended workflow file:

```text
.github/workflows/ci.yml
```

Initial CI stages:

```text
checkout
setup-dotnet
dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes
dotnet test --no-build
docker build API image
docker build Worker image
```

Testcontainers requirements:

- GitHub-hosted Linux runners can run Docker-based Testcontainers.
- Tests should avoid fixed ports.
- Tests should generate isolated database names or use reset strategies.
- Integration tests should be deterministic and parallel-safe where practical.
- Long-running end-to-end tests can be separated later if they slow PR feedback too much.

### Branch Protection

Recommended GitHub branch protection:

- require CI to pass before merge
- require pull request review
- require linear history if the team prefers it
- prevent direct commits to main
- require conversation resolution

### Container Images

The initial CI should validate image builds but does not need to publish images until the deployment target is confirmed.

Later pipeline stages can add:

- Software Bill of Materials generation
- vulnerability scanning
- image publishing to GitHub Container Registry or customer registry
- signed images
- deployment to dev/staging

### Deployment

Deployment should remain runtime-neutral until customer infrastructure is confirmed.

Possible future deployment workflows:

- publish container images to GitHub Container Registry
- deploy to Kubernetes using Helm
- deploy to managed containers
- deploy through customer CI/CD tooling
- trigger downstream deployment pipeline

GitHub Actions owns CI by default. CD can either stay in GitHub Actions or hand off to the customer's deployment platform.

## Decisions to Confirm

- Modular monolith as starting point.
- API and worker as separate processes.
- Container packaging.
- Cloud-neutral production posture.
- Docker Compose for local development.
- GitHub as source control and GitHub Actions as the default CI platform.
- MassTransit as the messaging abstraction.
- Transport selection deferred to customer/platform context.
- Broker abstraction in infrastructure.
- Relational database with module schemas.
- Outbox/inbox patterns.
- EF Core 10.
- Order cancellation policy.
- Inventory reservation strategy.
- Payment timing.
- Shipping trigger.
- Testcontainers as the initial integration testing foundation.
- OpenTelemetry as the initial observability foundation.

## Conversation Trail

This section captures the architecture direction agreed during planning.

Initial request:

- Create an implementation-ready skeleton for an Order Processing Platform.
- Use .NET 10 and EF Core 10.
- Focus on architecture, module boundaries, responsibilities, and delivery handover artifacts.
- Start with planning and assumptions before implementation.

Architecture discussion:

- The first proposal was a modular monolith with API and Worker processes.
- The architecture should not overfit to Azure or AWS.
- Deployment should be cloud-neutral and container-friendly.
- Infrastructure choices should depend on the customer context, especially greenfield vs brownfield.
- Queueing should not be hardcoded to a single broker.
- System design reasoning should follow a clarify-design-deep-dive-tradeoff flow.

Messaging discussion:

- Event-driven patterns are useful from the beginning.
- The system should not be fully event-sourced by default.
- CQRS should be used lightly at the application layer.
- MassTransit should be used as the messaging abstraction.
- The concrete MassTransit transport should be selected later based on customer/platform context.

Testing and agent-delivery discussion:

- Most implementation may be produced quickly by AI agents.
- Fast implementation requires executable guardrails.
- Testcontainers should be part of the initial foundation.
- Customer journey tests should be implemented early.
- Playwright is lower priority unless a browser UI is introduced.
- OpenTelemetry should be wired from the beginning.

Deployability discussion:

- Source control and CI will use GitHub.
- GitHub Actions should validate every pull request.
- CI should run restore, build, format checks, unit tests, architecture tests, integration tests, and container build validation.
- CD remains runtime-neutral until the customer deployment target is known.

Documentation discussion:

- README should stay clean and concise.
- Detailed reasoning, decisions, and slices should live in this design memory file.

## Current Decision Record

Confirmed or currently preferred decisions:

- Use .NET 10.
- Use EF Core 10.
- Start with a modular monolith.
- Use separate deployable processes for API and Worker.
- Package the platform as containers.
- Keep production deployment cloud-neutral.
- Use Docker Compose for local development.
- Host source code in GitHub.
- Use GitHub Actions for CI.
- Use a relational database with module-owned schemas.
- Use MassTransit as the messaging abstraction.
- Defer concrete transport selection to customer/platform context.
- Use event-driven integration patterns.
- Use outbox/inbox for messaging reliability.
- Use CQRS-style command/query separation in the application layer.
- Do not use event sourcing by default.
- Use Testcontainers early for realistic integration and customer journey tests.
- Use OpenTelemetry from the beginning.
- Keep external inventory, payment, tax, and shipping behind ports/adapters.

Deferred decisions:

- Final production database choice.
- Final MassTransit transport.
- Final production runtime platform.
- Inventory check vs reservation vs decrement behavior.
- Payment authorization vs capture timing.
- Full vs partial cancellation.
- Exact order lifecycle states.
- Whether any module should later become an independently deployed service.
- Whether Kafka/EventBridge/event streaming is required.
- Whether Playwright is needed after a UI exists.

## Implementation Slices

### Slice 1: Foundation

Create the implementation foundation:

- .NET 10 solution
- API project
- Worker project
- BuildingBlocks projects
- module folder/project structure
- EF Core 10 package setup
- MassTransit package setup
- OpenTelemetry bootstrap
- Docker Compose baseline
- GitHub Actions CI workflow
- Testcontainers integration test project

Requirements covered:

- modularity
- maintainability
- testability
- observability
- deployment readiness
- extensibility

### Slice 2: Orders Domain

Implement the core order model:

- `Order` aggregate
- `OrderLine`
- `OrderStatus`
- lifecycle records
- domain events
- cancellation policy
- creation invariants
- domain tests

Requirements covered:

- creating an order
- cancelling an order when permitted
- tracking order lifecycle
- maintainability
- testability

### Slice 3: Orders Persistence

Add persistence for Orders:

- `OrdersDbContext`
- EF Core configurations
- order tables
- order line tables
- lifecycle history table
- optimistic concurrency
- outbox table
- repository/unit-of-work boundary if useful
- Testcontainers database integration tests

Requirements covered:

- retrieving an existing order
- tracking lifecycle
- reliability
- scalability foundation
- maintainability

### Slice 4: Application Use Cases

Implement application-level command/query flows:

- `CreateOrderCommand`
- `GetOrderQuery`
- `CancelOrderCommand`
- `GetOrderLifecycleQuery`
- command/query handlers
- inventory port
- pricing port
- payment port
- shipping port
- fake adapters for deterministic scenarios

Requirements covered:

- creating an order
- retrieving an order
- cancelling an order
- validating inventory availability
- calculating pricing, taxes, and additional charges
- external integration boundaries

### Slice 5: API Surface

Expose the initial API:

- `POST /orders`
- `GET /orders/{orderId}`
- `POST /orders/{orderId}/cancel`
- `GET /orders/{orderId}/lifecycle`
- request/response contracts
- validation
- problem details
- correlation IDs
- authorization placeholders

Requirements covered:

- functional API behavior
- security foundation
- observability foundation
- maintainability

### Slice 6: Messaging and Worker

Implement the async foundation:

- integration event contracts
- MassTransit configuration
- in-memory transport for fast feedback
- transport abstraction/configuration point
- outbox dispatcher
- inbox placeholder
- worker message handlers
- worker tests
- Testcontainers transport tests only where needed

Requirements covered:

- integration with external systems
- reliability
- scalability
- observability
- extensibility

### Slice 7: Customer Journey Tests

Add executable business scenarios:

- successful order creation
- retrieve existing order
- reject or fail order when inventory is unavailable
- pricing/tax calculation included in order snapshot
- payment authorization failure
- cancel order while permitted
- reject cancellation after fulfillment starts
- outbox event is persisted
- worker dispatches event through MassTransit

Requirements covered:

- all functional requirements at scenario level
- testability
- reliability
- maintainability

### Slice 8: Architecture Handover

Add handover artifacts:

- ADR for modular monolith
- ADR for MassTransit and transport abstraction
- ADR for Testcontainers-first integration testing
- ADR for OpenTelemetry
- ADR for GitHub Actions CI
- architecture diagram
- module dependency rules
- future service extraction guidance
- deployment contract

Requirements covered:

- maintainability
- deployment
- extensibility
- architectural communication

## Human Judgment and AI Usage

AI can help generate drafts, compare options, scaffold code, and keep the documentation consistent.

Human judgment is required for:

- choosing the architectural style
- deciding what must be synchronous
- deciding what can be asynchronous
- defining domain boundaries
- defining consistency boundaries
- choosing infrastructure based on customer reality
- resisting premature microservices
- deciding what to defer

This file should be treated as working memory, not final truth.
