# ADR 0004: Use MassTransit As Messaging Abstraction

## Status

Accepted.

## Context

The system must integrate with external inventory, payment, and shipping systems. Some operations should be asynchronous, retryable, and observable.

The concrete queueing or broker technology depends on customer standards and runtime platform.

## Decision

Use MassTransit as the application messaging abstraction.

The skeleton uses in-memory transport for fast local/test feedback. Production transport remains deferred.

## Consequences

Business modules do not depend directly on broker SDKs. The platform can later choose RabbitMQ, Azure Service Bus, Amazon SQS, or another supported transport without rewriting domain logic.

The outbox pattern remains the reliability boundary for publishing integration events.

## Deferred

Final production transport, retry policies, dead-letter topology, inbox deduplication, and operational dashboards remain implementation backlog items.
