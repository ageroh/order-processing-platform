# Architecture Diagrams

These diagrams communicate the initial architecture at a level useful for implementation planning and interview discussion. They are intentionally cloud-neutral and focus on boundaries, flows, and deployment shape.

## 1. System Context

```mermaid
flowchart LR
    Customer[Customer / Client Application]
    Api[Order Processing Platform]
    Inventory[External Inventory System]
    Payment[Payment Provider]
    Shipping[Shipping Provider]
    Obs[Observability Backend]

    Customer -->|HTTP API| Api
    Api -->|availability check / future reservation| Inventory
    Api -->|authorize / future capture| Payment
    Api -->|shipment request / tracking callbacks| Shipping
    Api -->|traces / metrics / logs| Obs
```

## 2. Modular Monolith Container View

```mermaid
flowchart TB
    subgraph Platform[Order Processing Platform]
        Api[OrderProcessing.Api]
        Worker[OrderProcessing.Worker]

        subgraph Modules[Business Modules]
            Orders[Orders]
            Catalog[Catalog Contracts]
            Inventory[Inventory Contracts]
            Pricing[Pricing Contracts]
            Payments[Payments Contracts]
            Shipping[Shipping Contracts]
        end

        Db[(PostgreSQL)]
        Broker[MassTransit Transport Abstraction]
        Otel[OpenTelemetry]
    end

    Api --> Orders
    Worker --> Orders
    Orders --> Catalog
    Orders --> Inventory
    Orders --> Pricing
    Orders --> Payments
    Orders --> Shipping
    Orders --> Db
    Orders --> Broker
    Api --> Otel
    Worker --> Otel
```

## 3. Create Order Flow

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant Api as OrderProcessing.Api
    participant Orders as Orders Module
    participant Catalog as Catalog Contract
    participant Pricing as Pricing Contract
    participant Inventory as Inventory Contract
    participant Payments as Payments Contract
    participant Db as PostgreSQL
    participant Outbox as Outbox
    participant Worker as OrderProcessing.Worker
    participant Shipping as Shipping Contract

    Client->>Api: POST /orders
    Api->>Orders: CreateOrder command
    Orders->>Catalog: Validate product sellability
    Orders->>Pricing: Calculate prices, taxes, charges
    Orders->>Inventory: Validate availability
    Orders->>Payments: Authorize payment

    alt Accepted
        Orders->>Db: Persist order as Accepted
        Orders->>Outbox: Store OrderAccepted event
        Api-->>Client: 201 Created
        Worker->>Outbox: Dispatch pending event
        Worker->>Shipping: Start shipping workflow when applicable
    else Rejected
        Orders->>Db: Persist order as Rejected with reason
        Api-->>Client: 409 Conflict / validation problem
    end
```

## 4. Order Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Accepted: inventory, pricing, payment authorized
    Pending --> Rejected: validation, inventory, pricing, or payment failure
    Pending --> Cancelled: client cancels before acceptance completes
    Accepted --> Cancelled: full cancellation permitted
    Accepted --> [*]
    Rejected --> [*]
    Cancelled --> [*]
```

Initial lifecycle states are deliberately small: `Pending`, `Accepted`, `Rejected`, and `Cancelled`. Fulfillment-specific states can be added later when shipping ownership and provider behavior are confirmed.

## 5. Deployment Shape

```mermaid
flowchart TB
    subgraph Runtime[Customer-Approved Runtime]
        ApiContainer[API Docker Container]
        WorkerContainer[Worker Docker Container]
        Postgres[(PostgreSQL)]
        Transport[Message Transport]
        OtelCollector[OpenTelemetry Collector / Exporter]
    end

    GitHub[GitHub Actions CI] -->|build / test / image validation| ApiContainer
    GitHub -->|build / test / image validation| WorkerContainer

    ApiContainer --> Postgres
    WorkerContainer --> Postgres
    ApiContainer --> Transport
    WorkerContainer --> Transport
    ApiContainer --> OtelCollector
    WorkerContainer --> OtelCollector

    Runtime -. can be .-> DockerCompose[Docker Compose]
    Runtime -. can be .-> Vms[Docker on VMs]
    Runtime -. can be .-> Ecs[AWS ECS / Fargate]
    Runtime -. can be .-> ContainerApps[Azure Container Apps]
    Runtime -. can be .-> Kubernetes[Kubernetes]
```

The deployment contract is portable containers first. The production runtime, broker, and observability backend remain customer/environment decisions.
