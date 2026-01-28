# Dashboards

This document outlines recommended dashboards and the API endpoint used to feed them.

## Dashboard endpoint

`GET /api/v1/admin/dashboard/summary`

Returns:

- Jobs by status
- Bookings by status
- Total providers
- API version + uptime
- Alerting threshold config

This endpoint is intended for internal dashboards and should be protected (admin access).

## Suggested dashboards

### API Overview

- Request rate (RPS)
- 5xx rate
- Latency p50/p95/p99
- Active requests

### Job & Booking Health

- Jobs by status (stacked)
- Bookings by status (stacked)
- Booking expiry executions and failures

### Database

- Connection pool utilisation
- Slow query rate / p95 duration
- Error rate

### Auth

- 401/403 rate
- Token issuance rate (dev only)

## Notes

- Use OpenTelemetry metrics for real‑time charts.
- The dashboard endpoint provides simple counts and configuration context.

