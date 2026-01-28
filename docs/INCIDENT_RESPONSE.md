# Incident Response Runbook

Use this checklist for production incidents. Keep it short and action‑oriented.

## 1. Assess & Stabilise

- Confirm incident scope (customers, endpoints, regions).
- Check current alerts (latency, error rate, saturation).
- If severe, consider traffic reduction or temporary feature flags.

## 2. Communicate

- Open an incident channel and assign a lead.
- Post an initial status update within 15 minutes.
- Track key timestamps and decisions.

## 3. Diagnose

- Review recent deploys and configuration changes.
- Check logs with correlation IDs.
- Check database health, connection pool, and slow queries.
- Validate external dependencies (auth, notifications, OTLP).

## 4. Mitigate

- Roll back to last known good release if needed.
- Apply hotfix only if rollback is not feasible.
- Confirm smoke tests after mitigation.

## 5. Recover

- Verify health endpoints and key flows.
- Monitor for 30–60 minutes after recovery.

## 6. Post‑Incident

- Capture root cause and contributing factors.
- Add or adjust alerts and dashboards.
- Schedule follow‑up work and document outcomes.

