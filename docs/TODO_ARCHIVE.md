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

## Archive 2026-01-27

## RentADad.Platform TODO 1

## Build

- [x] Scaffold solution and projects (.NET 10 LTS)
- [x] Configure core package references (EF Core, auth, validation)
- [x] Add initial domain model (Job, Booking, Provider)
- [x] Implement first API endpoints (Jobs)
- [x] Add initial migrations and database setup

## Next

- [x] Add EF mapping for Job.ServiceIds (currently ignored)
- [x] Add repositories + unit of work usage in API
- [x] Add application use cases for Jobs and Bookings
- [x] Wire Jobs endpoints to application layer
- [x] Wire Booking endpoints to application layer
- [x] Add Booking endpoints (create/confirm/decline/expire/cancel)
- [x] Add Provider endpoints (register/update/add availability)
- [x] Add request validation with FluentValidation
- [x] Add ProblemDetails error mapping for domain violations
- [x] Add integration tests for Jobs/Bookings API
- [x] Add initial seed data for local dev

## Archive 2026-01-27 (MVP Hardening)

### P0 Production Readiness

- [x] Add Dockerfile + docker compose for API (local + CI)
- [x] Add health checks (liveness/readiness) + basic probes
- [x] Add database migration workflow (startup apply or manual script)
- [x] Add configuration validation on startup (required settings)
- [x] Define versioning policy for API (SemVer, compatibility rules, deprecation window)

### P0 API Guardrails

- [x] Add request/response contracts doc (swagger + examples)
- [x] Add rate limiting / throttling defaults
- [x] Add auth placeholder (API key or JWT stub)
- [x] Add standardized ProblemDetails response shape (errorCode, traceId, version)
- [x] Introduce API versioning strategy (route or header) + default version

### P1 Data & Persistence

- [x] Add concurrency handling strategy for updates (rowversion or retry)
- [x] Add indexes for availability window search
- [x] Add pagination + filtering for list endpoints

### P1 Observability

- [x] Add OpenTelemetry tracing + basic metrics
- [x] Add structured request logging with latency
- [x] Add error logging with correlation id

### P2 Developer Experience

- [x] Add local dev bootstrap script
- [x] Add make/just tasks (build/test/format)
- [x] Add seed data CLI for demo scenarios

## Archive 2026-01-28 (Quality & Readiness)

### P0 Testing Completeness

- [x] Define test coverage targets by layer (domain/application/api)
- [x] Add missing API tests for new list/search endpoints
- [x] Add validation tests for paging/filtering inputs
- [x] Add concurrency tests using ETag/UpdatedUtc
- [x] Add negative tests for auth enabled mode

### P1 Reliability

- [x] Add retry policy for transient DB failures
- [x] Add idempotency guidance for write actions
- [x] Add background job to expire stale bookings (if needed)

### P1 Docs

- [x] Update API_CONTRACTS with paging parameters and examples (verify)
- [x] Document ETag usage and concurrency behavior
- [x] Document health checks and metrics endpoints

### P2 DX

- [x] Add docker-compose override for local debugging
- [x] Add sample .env for local configuration

## Archive 2026-01-28 (Productization & Platform)

### P0 Productization

- [x] Implement auth flows (JWT validation + roles/claims)
- [x] Add API key alternative for service-to-service use
- [x] Implement idempotency key storage (backed by DB)
- [x] Add outbound notification stub (email/sms webhook)

### P1 Operations

- [x] Add structured audit logs for state transitions
- [x] Add admin endpoints for job/booking oversight
- [x] Add backup/restore runbook
- [x] Add deployment checklist and rollback plan

### P1 Performance

- [x] Add caching strategy for provider availability searches
- [x] Add read models for job listings
- [x] Add bulk operations for provider availability

### P2 Security

- [x] Add security headers middleware
- [x] Add secrets management guidance
- [x] Add vulnerability scanning in CI

## Archive 2026-01-28 (Go-Live & Hardening)

### P0 Go-Live

- [x] Define production environment variables checklist
- [ ] Add GitHub Actions release workflow (build, test, artefact)
- [x] Add database migration job to CI/CD pipeline
- [x] Add production readiness checklist (health, backups, on-call)
- [x] Add smoke test script for API endpoints

### P1 Operability

- [x] Add alerting thresholds (latency, error rate, saturation)
- [x] Add dashboards for API, DB, and background jobs
- [x] Add structured request/response logging redaction rules
- [x] Add incident response runbook

### P2 Security & Compliance

- [x] Add dependency licence report in CI
- [x] Add secrets rotation guidance
- [x] Add threat model notes (entry points, trust boundaries)

### P2 Product

- [x] Add user-facing documentation for API consumers
- [x] Add status page or uptime link (if applicable)
