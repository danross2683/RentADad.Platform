# RentADad.Platform TODO

## Milestone: Go-Live & Hardening

### P0 Go-Live

- [x] Define production environment variables checklist
- [x] Add GitHub Actions release workflow (build, test, artefact)
- [x] Add database migration job to CI/CD pipeline
- [x] Add production readiness checklist (health, backups, on-call)
- [x] Add smoke test script for API endpoints

### P1 Operability

- [ ] Add alerting thresholds (latency, error rate, saturation)
- [ ] Add dashboards for API, DB, and background jobs
- [ ] Add structured request/response logging redaction rules
- [ ] Add incident response runbook

### P2 Security & Compliance

- [ ] Add dependency licence report in CI
- [ ] Add secrets rotation guidance
- [ ] Add threat model notes (entry points, trust boundaries)

### P2 Product

- [ ] Add user-facing documentation for API consumers
- [ ] Add status page or uptime link (if applicable)
