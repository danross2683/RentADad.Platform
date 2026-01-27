# RentADad.Platform TODO

## Milestone: MVP Hardening (Next)

### P0 Production Readiness

- [ ] Add Dockerfile + docker compose for API (local + CI)
- [ ] Add health checks (liveness/readiness) + basic probes
- [ ] Add database migration workflow (startup apply or manual script)
- [ ] Add configuration validation on startup (required settings)

### P0 API Guardrails

- [ ] Add request/response contracts doc (swagger + examples)
- [ ] Add rate limiting / throttling defaults
- [ ] Add auth placeholder (API key or JWT stub)
- [ ] Add standardized ProblemDetails response shape (errorCode, traceId)

### P1 Data & Persistence

- [ ] Add concurrency handling strategy for updates (rowversion or retry)
- [ ] Add indexes for availability window search
- [ ] Add pagination + filtering for list endpoints

### P1 Observability

- [ ] Add OpenTelemetry tracing + basic metrics
- [ ] Add structured request logging with latency
- [ ] Add error logging with correlation id

### P2 Developer Experience

- [ ] Add local dev bootstrap script
- [ ] Add make/just tasks (build/test/format)
- [ ] Add seed data CLI for demo scenarios

---

## Archive (Completed - 2026-01-27)

### P0 Core Stability

- [x] Add test database setup/teardown for integration tests
- [x] Add deterministic test data helpers for Jobs/Bookings/Providers
- [x] Add CI-friendly configuration for connection strings

### P1 API Behavior

- [x] Add Jobs lifecycle integration tests (post/accept/start/complete/close)
- [x] Add Booking edge-case tests (double confirm, cancel after confirm, expire)
- [x] Add Provider availability tests (overlap rejection, remove availability)

### P1 Application

- [x] Add domain rule error codes for Job/Booking/Provider
- [x] Add application-level validation for date/time windows

### P2 Observability

- [x] Add structured logging for lifecycle transitions
- [x] Add correlation ID middleware

### P2 Data

- [x] Add seed scenario for multi-provider job competition
- [x] Add migration for job/booking indexes
