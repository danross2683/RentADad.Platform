# RentADad.Platform TODO

## P0 Core Stability

- [x] Add test database setup/teardown for integration tests
- [x] Add deterministic test data helpers for Jobs/Bookings/Providers
- [x] Add CI-friendly configuration for connection strings

## P1 API Behavior

- [ ] Add Jobs lifecycle integration tests (post/accept/start/complete/close)
- [ ] Add Booking edge-case tests (double confirm, cancel after confirm, expire)
- [ ] Add Provider availability tests (overlap rejection, remove availability)

## P1 Application

- [ ] Add domain rule error codes for Job/Booking/Provider
- [ ] Add application-level validation for date/time windows

## P2 Observability

- [ ] Add structured logging for lifecycle transitions
- [ ] Add correlation ID middleware

## P2 Data

- [ ] Add seed scenario for multi-provider job competition
- [ ] Add migration for job/booking indexes
