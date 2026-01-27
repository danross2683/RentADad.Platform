# RentADad.Platform TODO

## Foundations

- [x] Confirm .NET SDK LTS version to target (.NET 10 LTS)
- [x] Add local dev setup notes (env vars, ports, secrets)
- [x] Define initial API surface area and naming conventions

## Domain

- [x] Formalize job lifecycle states and transitions
- [x] Define booking invariants and validation rules
- [x] Model provider availability constraints
- [x] Add domain events for state changes

## Application

- [x] Create core use cases for job posting and booking
- [x] Add request/response DTOs and validators
- [x] Add application-level error handling patterns

## Infrastructure

- [x] Set up EF Core mappings for aggregates
- [x] Add PostgreSQL migration baseline
- [x] Wire up persistence + unit of work pattern

## API

- [x] Scaffold initial endpoints for jobs and bookings
- [x] Add auth policy scaffolding (JWT)
- [x] Enable OpenAPI docs with versioning

## Tests

- [x] Add domain unit tests for lifecycle transitions
- [x] Add application tests for use cases

## Documentation

- [x] Add architecture decision record template
- [x] Document lifecycle diagrams (job, booking, provider)
