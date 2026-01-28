# RentADad.Platform TODO

## Milestone: Productization & Platform

### P0 Productization

- [ ] Implement auth flows (JWT validation + roles/claims)
- [ ] Add API key alternative for service-to-service use
- [ ] Implement idempotency key storage (backed by DB)
- [ ] Add outbound notification stub (email/sms webhook)

### P1 Operations

- [ ] Add structured audit logs for state transitions
- [ ] Add admin endpoints for job/booking oversight
- [ ] Add backup/restore runbook
- [ ] Add deployment checklist and rollback plan

### P1 Performance

- [ ] Add caching strategy for provider availability searches
- [ ] Add read models for job listings
- [ ] Add bulk operations for provider availability

### P2 Security

- [ ] Add security headers middleware
- [ ] Add secrets management guidance
- [ ] Add vulnerability scanning in CI
