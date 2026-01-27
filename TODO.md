# RentADad.Platform TODO

## Milestone: Quality & Readiness

### P0 Testing Completeness

- [x] Define test coverage targets by layer (domain/application/api)
- [x] Add missing API tests for new list/search endpoints
- [x] Add validation tests for paging/filtering inputs
- [x] Add concurrency tests using ETag/UpdatedUtc
- [x] Add negative tests for auth enabled mode

### P1 Reliability

- [ ] Add retry policy for transient DB failures
- [ ] Add idempotency guidance for write actions
- [ ] Add background job to expire stale bookings (if needed)

### P1 Docs

- [ ] Update API_CONTRACTS with paging parameters and examples (verify)
- [ ] Document ETag usage and concurrency behavior
- [ ] Document health checks and metrics endpoints

### P2 DX

- [ ] Add docker-compose override for local debugging
- [ ] Add sample .env for local configuration
