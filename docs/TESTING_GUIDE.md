# Testing Guide

This document defines the testing approach for the platform.

## Coverage targets

- Domain: 90%+ statement coverage for core aggregates (Job, Booking, Provider).
- Application: 80%+ statement coverage focused on use cases and error mapping.
- API: 70%+ for endpoint flows, validation, and error handling.
- P0 behaviors: lifecycle transitions, validation rules, and concurrency.

## Domain tests

- Focus on lifecycle transitions and invariants.
- Use pure unit tests with no infrastructure dependencies.
- Cover terminal state rules (Cancelled, Closed, Disputed).

## Application tests

- Validate use case orchestration and error mapping.
- Use fake repositories and fake unit of work.
- Assert ProblemDetails error shapes for failures.
