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

- [ ] Create core use cases for job posting and booking
- [ ] Add request/response DTOs and validators
- [ ] Add application-level error handling patterns

## Infrastructure

- [ ] Set up EF Core mappings for aggregates
- [ ] Add PostgreSQL migration baseline
- [ ] Wire up persistence + unit of work pattern

## API

- [ ] Scaffold initial endpoints for jobs and bookings
- [ ] Add auth policy scaffolding (JWT)
- [ ] Enable OpenAPI docs with versioning

## Tests

- [ ] Add domain unit tests for lifecycle transitions
- [ ] Add application tests for use cases

## Documentation

- [ ] Add architecture decision record template
- [ ] Document lifecycle diagrams (job, booking, provider)
