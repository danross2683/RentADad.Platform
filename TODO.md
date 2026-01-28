# RentADad.Platform TODO

## Milestone: Productization & Platform

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
