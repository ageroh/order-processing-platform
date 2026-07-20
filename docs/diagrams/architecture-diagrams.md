# Architecture Diagrams

The architecture diagrams are defined with Structurizr DSL in [workspace.dsl](workspace.dsl).

This keeps the diagrams close to the codebase, reviewable in Git, and aligned with C4-style architecture communication.

## Views

The workspace defines:

- `SystemContext`: the platform and external systems.
- `ContainerView`: API, Worker, PostgreSQL, and message transport.
- `ModuleView`: business modules and contract boundaries inside the API host.
- `CreateOrderFlow`: the representative create-order interaction flow.
- `OrderLifecycle`: custom view for `Pending`, `Accepted`, `Rejected`, and `Cancelled`.
- `DeploymentView`: cloud-neutral portable container deployment shape.

## Rendering

The file can be rendered with Structurizr tooling, such as Structurizr Lite or the Structurizr CLI.

The repository does not require a rendering tool to build or test the .NET solution; the DSL is an architecture artifact for reviewers and the delivery team.

## Lifecycle Assumption

Initial lifecycle states are deliberately small:

- `Pending`
- `Accepted`
- `Rejected`
- `Cancelled`

Fulfillment-specific states can be added later when shipping ownership and provider behavior are confirmed.
