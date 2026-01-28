# API Consumer Guide

This guide is for external consumers integrating with the RentADad API.

## Base URL

Your base URL will be provided per environment (dev/staging/production).

## Authentication

Write operations require authentication. Read endpoints are public unless stated otherwise.

- **JWT**: Send `Authorization: Bearer <token>`
- **API key** (service‑to‑service): Send `X-API-Key: <key>`

## Versioning

All endpoints are versioned under `/api/v1`. Breaking changes will be avoided where possible.

## Idempotency

For write operations, send `Idempotency-Key: <uuid>` to safely retry without duplicates.

## Concurrency

Write endpoints return an `ETag`. Provide `If-Match` on updates to avoid overwriting newer data.

## Errors

Errors follow ProblemDetails and include:

- `traceId` for support
- `errorCode` for programmatic handling
- `version` for API versioning

## Common endpoints

- `GET /api/v1/jobs`
- `GET /api/v1/jobs/search`
- `POST /api/v1/jobs`
- `GET /api/v1/bookings/{bookingId}`
- `POST /api/v1/bookings`
- `GET /api/v1/providers/search`

See `docs/API_CONTRACTS.md` for full request/response examples.

## Support

Include the `X-Correlation-Id` (or `traceId`) when contacting support.

