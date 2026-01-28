# Alerting Thresholds

These thresholds are a starting point. Tune them with real traffic patterns.

## API (HTTP)

- **Availability**: alert if 5xx rate > 1% for 5 minutes.
- **Latency (p95)**: alert if p95 > 1.5s for 5 minutes.
- **Latency (p99)**: alert if p99 > 3s for 5 minutes.
- **Saturation**: alert if request queue length > 0 for 3 minutes.

## Database

- **Connection errors**: any sustained errors for 2 minutes.
- **Slow queries**: p95 DB command > 500ms for 5 minutes.
- **Pool exhaustion**: available connections < 10% for 5 minutes.

## Background Jobs

- **Booking expiry lag**: last run > 10 minutes ago.
- **Job failures**: any unhandled exception > 0 in 10 minutes.

## Auth

- **Auth failures**: 401/403 rate > 5% for 5 minutes (watch for key rotation).

## Notes

- For early go‑live, prefer low‑noise alerts and focus on availability + latency.
- Add runbooks/links to each alert once dashboards exist.

