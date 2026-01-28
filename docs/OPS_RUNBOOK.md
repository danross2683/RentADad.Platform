# Ops Runbook

## Backup & restore (PostgreSQL)

### Backup

```shell
pg_dump -h <host> -p 5432 -U <user> -F c -b -f rentadad.backup rentadad
```

### Restore

```shell
pg_restore -h <host> -p 5432 -U <user> -d rentadad -c rentadad.backup
```

## Notes

- Store backups in secure, encrypted storage.
- Verify restore regularly in a non-production environment.
- Coordinate restores with application downtime where required.

## Alerting thresholds

See `docs/ALERTING_THRESHOLDS.md` for default thresholds and tuning guidance.

## Deployment checklist

- Validate configuration for target environment.
- Run database migrations (or confirm auto-migrate is enabled).
- Smoke-test health endpoints: `/health/live` and `/health/ready`.
- Verify authentication paths (JWT and/or API key).
- Confirm monitoring/alerts are wired (OTLP exporter).

## Rollback plan

- Revert application deployment to the previous version.
- If migrations were applied, assess rollback feasibility:
  - Prefer forward-fix migrations.
  - Use database restore only if required and coordinated.
- Re-run smoke tests after rollback.
