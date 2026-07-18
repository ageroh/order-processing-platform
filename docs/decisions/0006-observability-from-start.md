# ADR 0006: Wire OpenTelemetry From The Beginning

## Status

Accepted.

## Context

Order processing crosses HTTP requests, database transactions, asynchronous messages, and external providers. Failures must be diagnosable across those boundaries.

## Decision

Add OpenTelemetry instrumentation at the API and Worker composition roots from the beginning.

Trace boundaries should include:

- HTTP request
- command/query handling
- EF Core transaction
- outbox write and dispatch
- message publishing/consumption
- external provider adapter call

## Consequences

The skeleton is ready for traces, metrics, logs, and correlation without choosing a final observability backend.

## Deferred

The final exporter, sampling policy, log aggregation backend, dashboards, and alerting rules are runtime/platform decisions.
