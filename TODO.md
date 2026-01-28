# RentADad.Platform TODO

## Milestone: Quality & Readiness

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
