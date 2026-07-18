# ADR 0002: Use Cloud-Neutral Container Artifacts

## Status

Accepted.

## Context

The production runtime platform is unknown. The platform may eventually run on Azure Container Apps, AWS ECS/Fargate, Kubernetes, Docker on virtual machines, private cloud, or another customer-approved runtime.

Choosing a specific cloud service too early would create unnecessary coupling.

## Decision

The first deployability contract is portable Docker images:

- `OrderProcessing.Api`
- `OrderProcessing.Worker`

Local development uses Docker Compose. GitHub Actions validates image builds.

## Consequences

The solution remains portable while still being deployable. Runtime-specific choices such as ingress, autoscaling, secrets, managed database, service bus, and observability backend can be made later.

## Deferred

The final runtime platform, production deployment topology, managed database choice, and secret store remain open customer/environment decisions.
