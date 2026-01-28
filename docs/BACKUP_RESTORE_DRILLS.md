# Backup & Restore Drill Schedule

Establish a regular drill to ensure backups are usable and recovery steps are current.

## Schedule

- **Monthly**: restore to staging and validate core flows.
- **Quarterly**: full restore drill with smoke tests and performance checks.
- **After major schema changes**: run an extra drill.

## Drill steps

1. Restore the latest production backup into staging.
2. Apply any pending migrations.
3. Run smoke tests (`scripts/smoke-test.*`).
4. Validate read/write endpoints.
5. Record duration and issues.

## Evidence

- Store drill results in a shared log.
- Note any gaps and follow‑up actions.

