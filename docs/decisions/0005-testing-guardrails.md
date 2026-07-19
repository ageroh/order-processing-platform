# ADR 0005: Treat Tests As Architecture Guardrails

## Status

Accepted.

## Context

Most implementation work may be produced quickly by AI-assisted agents. That increases the value of executable guardrails that catch boundary violations and behavioral regressions early.

## Decision

Use:

- xUnit as the test runner
- Shouldly for readable assertions
- Moq where interaction-based mocks are genuinely needed
- Testcontainers for realistic infrastructure scenarios
- `GivenX_WhenY_ThenZ` naming for test methods

## Consequences

Tests communicate expected behavior to both humans and AI agents. Integration tests can validate realistic PostgreSQL and future messaging behavior without requiring full production infrastructure.

## Deferred

Full customer journey tests should be added when application use cases exist. Playwright is deferred unless a UI is introduced.
