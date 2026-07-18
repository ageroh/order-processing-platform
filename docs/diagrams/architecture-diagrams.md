# Architecture Diagrams

These diagrams communicate the architecture at a level useful for implementation planning and interview discussion. They are intentionally cloud-neutral and focus on system boundaries, runtime shape, async processing, and order lifecycle rules.

## 1. System Design Overview

```mermaid
flowchart TB
    Client[Client / API Consumer]

    subgraph Platform[Order Processing Platform]
        Api[OrderProcessing.Api<br/>ASP.NET Core Controllers<br/>Swagger / Correlation Headers]
        Worker[OrderProcessing.Worker<br/>Async Processing<br/>Retries / Outbox Dispatch]

        subgraph Modules[Modular Monolith Modules]
            Orders[Orders<br/>Order aggregate<br/>Lifecycle<br/>Cancellation rules]
            Catalog[Catalog<br/>Product identity<br/>Sellability]
            Inventory[Inventory<br/>Availability check<br/>Future reservation]
            Pricing[Pricing<br/>Price / tax / charges]
            Payments[Payments<br/>Authorization<br/>Future capture]
            Shipping[Shipping<br/>Shipment initiation<br/>Tracking callbacks]
        end

        Db[(PostgreSQL<br/>EF Core module-owned schemas)]
        Outbox[(Outbox<br/>Integration events)]
        Bus[MassTransit<br/>Transport abstraction]
        Otel[OpenTelemetry<br/>Traces / metrics / logs]
    end

    subgraph External[External Customer / Vendor Systems]
        ExternalInventory[Inventory System]
        PaymentProvider[Payment Provider]
        ShippingProvider[Shipping Provider]
        ObservabilityBackend[Observability Backend]
    end

    Client -->|HTTP| Api
    Api --> Orders
    Worker --> Orders

    Orders --> Catalog
    Orders --> Pricing
    Orders --> Inventory
    Orders --> Payments
    Orders --> Shipping

    Orders --> Db
    Orders --> Outbox
    Worker --> Outbox
    Worker --> Bus

    Inventory --> ExternalInventory
    Payments --> PaymentProvider
    Shipping --> ShippingProvider

    Api --> Otel
    Worker --> Otel
    Otel --> ObservabilityBackend

    Bus -. production transport selected later .-> Transport[(Broker / Queue<br/>RabbitMQ, Azure Service Bus, SQS, etc.)]
```

The platform starts as a modular monolith packaged into portable API and Worker containers. Module boundaries are explicit now, while broker choice, runtime platform, and observability backend remain replaceable customer/environment decisions.

## 2. Order Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending: create order request received
    Pending --> Accepted: inventory available, pricing calculated, payment authorized
    Pending --> Rejected: validation, inventory, pricing, or payment failure
    Pending --> Cancelled: full cancellation before acceptance completes
    Accepted --> Cancelled: full cancellation permitted
    Accepted --> [*]
    Rejected --> [*]
    Cancelled --> [*]
```

Initial lifecycle states are deliberately small: `Pending`, `Accepted`, `Rejected`, and `Cancelled`. Fulfillment-specific states can be added later when shipping ownership and provider behavior are confirmed.
