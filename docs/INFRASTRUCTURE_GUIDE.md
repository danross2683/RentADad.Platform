# Infrastructure Guide

This document defines the initial persistence and infrastructure patterns.

## Persistence boundaries

- Domain is persistence-ignorant.
- Infrastructure owns EF Core mappings and database configuration.
- Application depends on repository abstractions, not EF Core.

## EF Core mapping

- Use aggregate roots as DbSet entries.
- Map value objects as owned types.
- Enforce invariants with database constraints where possible.

## PostgreSQL baseline

- Use a single `AppDbContext`.
- Start with a baseline migration after the first aggregate mapping.
- Keep migrations in Infrastructure.

## Unit of Work

- Expose `IUnitOfWork` with a single `SaveChangesAsync` method.
- Repository methods should not call `SaveChangesAsync` directly.
