# Production Readiness Checklist

Use this checklist before first release and for every major change.

## Configuration

- [ ] All required production environment variables are set (see `docs/PRODUCTION_ENV_VARS.md`).
- [ ] Secrets are injected from a secret store and not in files or logs.
- [ ] `Auth:Enabled=true` and JWT settings are validated.
- [ ] `Database:AutoMigrate` set according to migration strategy.

## Data & Migrations

- [ ] Backups are configured and a restore has been tested.
- [ ] Migrations are applied via CI/CD or a controlled manual step.
- [ ] Connection string uses least‑privilege credentials.

## Reliability

- [ ] Health checks return 200 for `/health/live` and `/health/ready`.
- [ ] Rate limiting is enabled and limits are agreed.
- [ ] Background jobs are running (booking expiry).

## Observability

- [ ] OTLP exporter configured (tracing + metrics).
- [ ] Error logs include correlation IDs.
- [ ] Alerts set for latency, error rate, and saturation.

## Security

- [ ] Security headers are enabled (HSTS, CSP, etc.).
- [ ] Dependency vulnerability scan passes.
- [ ] API key and JWT paths are validated.

## Release

- [ ] Release artefact created and stored.
- [ ] Rollback steps verified in `docs/OPS_RUNBOOK.md`.
- [ ] Smoke test script passes.

