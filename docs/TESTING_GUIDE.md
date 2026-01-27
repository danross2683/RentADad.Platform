# Testing Guide

This document defines the testing approach for the platform.

## Domain tests

- Focus on lifecycle transitions and invariants.
- Use pure unit tests with no infrastructure dependencies.
- Cover terminal state rules (Cancelled, Closed, Disputed).

## Application tests

- Validate use case orchestration and error mapping.
- Use fake repositories and fake unit of work.
- Assert ProblemDetails error shapes for failures.
