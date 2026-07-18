# ADR 0001: Start As A Modular Monolith

## Status

Accepted.

## Context

The business boundaries for order processing are visible but not yet proven by production scale, team ownership, release cadence, or customer infrastructure constraints.

The platform must support order creation, retrieval, cancellation, lifecycle tracking, inventory validation, pricing, payment, and shipping integration. These concerns are related, but they should not collapse into one unstructured codebase.

## Decision

Start with a modular monolith inspired by Ardalis.Modulith:

- one API host for synchronous HTTP access
- one Worker host for asynchronous processing
- business modules under `src/Modules`
- module implementation types internal by default
- `.Contracts` projects for public module contracts
- architecture tests to guard public surface and dependencies

## Consequences

This keeps deployment simple while preserving module boundaries. The team can move quickly without introducing distributed-system costs too early.

If a module later needs independent deployment, the contracts, persistence ownership, and async boundaries provide an extraction path.

## Deferred

Independent service deployment is deferred until there is evidence from scale, ownership, integration pressure, or release cadence.
