workspace "Order Processing Platform" "Cloud-neutral modular monolith skeleton for order processing." {
    model {
        customer = person "Client / API Consumer" "Creates, retrieves, and cancels orders through the HTTP API."

        inventorySystem = softwareSystem "External Inventory System" "Customer or vendor system used to validate availability."
        paymentProvider = softwareSystem "Payment Provider" "External provider used for payment authorization and future capture."
        shippingProvider = softwareSystem "Shipping Provider" "External provider used for future shipment initiation and tracking callbacks."
        observabilityBackend = softwareSystem "Observability Backend" "Customer-selected backend for traces, metrics, logs, and alerts."

        orderProcessing = softwareSystem "Order Processing Platform" "Processes orders through a modular monolith with API and Worker containers." {
            api = container "OrderProcessing.Api" "Synchronous HTTP API using controllers, Swagger, and correlation headers." ".NET 10 / ASP.NET Core" {
                ordersModule = component "Orders Module" "Owns order aggregate, lifecycle, cancellation rules, order history, and order persistence." ".NET module"
                catalogContracts = component "Catalog Contracts" "Product identity, product status, and sellability boundary." ".NET contracts"
                inventoryContracts = component "Inventory Contracts" "Availability validation and future reservation boundary." ".NET contracts"
                pricingContracts = component "Pricing Contracts" "Price, tax, and additional charge calculation boundary." ".NET contracts"
                paymentContracts = component "Payments Contracts" "Payment authorization, future capture, and callback boundary." ".NET contracts"
                shippingContracts = component "Shipping Contracts" "Shipment initiation and tracking callback boundary." ".NET contracts"

                ordersModule -> catalogContracts "Checks product sellability through contract boundary"
                ordersModule -> inventoryContracts "Validates inventory availability through contract boundary"
                ordersModule -> pricingContracts "Requests price, tax, and charge calculation through contract boundary"
                ordersModule -> paymentContracts "Requests payment authorization through contract boundary"
                ordersModule -> shippingContracts "Raises future shipping integration need through contract boundary"
            }

            worker = container "OrderProcessing.Worker" "Background process for future outbox dispatch, retries, and integration workflows." ".NET 10 Worker Service"
            database = container "PostgreSQL" "Module-owned schemas for orders, order lines, lifecycle entries, and outbox messages." "PostgreSQL / EF Core" "Database"
            transport = container "Message Transport" "Replaceable transport configured through MassTransit." "MassTransit abstraction" "Queue"
        }

        customer -> api "Uses" "HTTPS/JSON"
        api -> database "Persists order state and outbox messages" "EF Core/Npgsql"
        worker -> database "Reads pending outbox messages and order state" "EF Core/Npgsql"
        worker -> transport "Publishes and consumes integration messages" "MassTransit"
        api -> inventorySystem "Validates availability" "Provider adapter"
        api -> paymentProvider "Authorizes payment" "Provider adapter"
        worker -> shippingProvider "Initiates future shipping workflow" "Provider adapter"
        api -> observabilityBackend "Exports telemetry" "OpenTelemetry"
        worker -> observabilityBackend "Exports telemetry" "OpenTelemetry"

        pending = element "Pending" "Order State" "Order request received and being evaluated." "Order State"
        accepted = element "Accepted" "Order State" "Inventory, pricing, and payment authorization succeeded." "Order State"
        rejected = element "Rejected" "Order State" "Validation, inventory, pricing, or payment authorization failed." "Order State"
        cancelled = element "Cancelled" "Order State" "Full-order cancellation was permitted and recorded." "Order State"

        pending -> accepted "inventory available, pricing calculated, payment authorized"
        pending -> rejected "validation, inventory, pricing, or payment failure"
        pending -> cancelled "full cancellation before acceptance completes"
        accepted -> cancelled "full cancellation permitted"

        deploymentEnvironment "Portable Containers" {
            deploymentNode "Customer-approved runtime" "Docker Compose, Docker on VMs, ECS/Fargate, Azure Container Apps, Kubernetes, or private cloud." "Container runtime" {
                containerInstance api
                containerInstance worker
            }

            deploymentNode "Data Store" "Customer-approved PostgreSQL hosting." "PostgreSQL" {
                containerInstance database
            }

            deploymentNode "Broker / Queue" "Customer-approved MassTransit transport." "Queueing infrastructure" {
                containerInstance transport
            }
        }
    }

    views {
        systemContext orderProcessing "SystemContext" {
            include *
            autoLayout lr
            title "System Context"
        }

        container orderProcessing "ContainerView" {
            include *
            autoLayout lr
            title "Container View"
        }

        component api "ModuleView" {
            include *
            autoLayout lr
            title "Module View"
        }

        dynamic orderProcessing "CreateOrderFlow" {
            customer -> api "POST /orders"
            api -> database "Persist order and outbox message"
            api -> inventorySystem "Validate inventory availability"
            api -> paymentProvider "Authorize payment"
            worker -> database "Read pending outbox message"
            worker -> transport "Publish integration event"
            title "Create Order Flow"
        }

        custom "OrderLifecycle" {
            include pending accepted rejected cancelled
            autoLayout lr
            title "Order Lifecycle"
        }

        deployment orderProcessing "Portable Containers" "DeploymentView" {
            include *
            autoLayout lr
            title "Cloud-Neutral Deployment View"
        }

        styles {
            element "Person" {
                shape Person
                background #08427b
                color #ffffff
            }

            element "Software System" {
                background #1168bd
                color #ffffff
            }

            element "Container" {
                background #438dd5
                color #ffffff
            }

            element "Component" {
                background #85bbf0
                color #000000
            }

            element "Database" {
                shape Cylinder
                background #438dd5
                color #ffffff
            }

            element "Queue" {
                shape Pipe
                background #438dd5
                color #ffffff
            }

            element "Order State" {
                shape RoundedBox
                background #2e7d32
                color #ffffff
            }
        }
    }
}
