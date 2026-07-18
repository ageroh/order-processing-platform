# Infrastructure

This folder is reserved for future infrastructure-as-code once the customer runtime platform is confirmed.

The production platform is currently unknown. The first deployability target is therefore portable Docker images for `OrderProcessing.Api` and `OrderProcessing.Worker`.

Those images should be able to run later on:

- Azure Container Apps
- AWS ECS/Fargate
- Kubernetes
- Docker on virtual machines
- private cloud or on-premises container platforms

Current deployment contract:

- source control: GitHub
- CI: GitHub Actions
- runtime packaging: portable Docker images
- local development: Docker Compose
- default database: PostgreSQL
- messaging abstraction: MassTransit
- production transport: customer/platform decision
- observability: OpenTelemetry
