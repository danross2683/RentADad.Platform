# RentADad.Platform TODO

## Milestone: Go-Live & Hardening

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

- [ ] Add user-facing documentation for API consumers
- [ ] Add status page or uptime link (if applicable)
