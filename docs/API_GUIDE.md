# API Guide

This document defines the initial API structure and policies.

## Endpoints

See `docs/API_CONVENTIONS.md` for the v1 endpoints list and naming.

## Contracts

See `docs/API_CONTRACTS.md` for request/response examples.

## Versioning & deprecation

See `docs/VERSIONING_POLICY.md` for versioning and deprecation rules.

## Auth scaffolding (JWT)

- Use bearer token authentication.
- Configure issuer, audience, and signing key via configuration.
- Protect all write endpoints by default.
- Use policies for role or capability-based access.

## OpenAPI

- Enable OpenAPI/Swagger for local development.
- Include API version in the document title and path.
- Use operation tags per resource: `Jobs`, `Bookings`, `Providers`, `Customers`, `Services`.

## Errors

- Use RFC 7807 ProblemDetails.
- Include a stable `errorCode` extension for domain errors.

## Usage limits

See `docs/API_USAGE_LIMITS.md` for default limits and retry guidance.

## Request tracing

See `docs/REQUEST_TRACING.md` for correlation ID usage.

## Health & metrics

- Liveness: `GET /health/live`
- Readiness: `GET /health/ready` (checks database connectivity)
- Metrics: exported via OTLP to the configured collector

## Idempotency

Write actions should be idempotent where possible. Clients are expected to:

- provide an `Idempotency-Key` header for create actions,
- retry safely on timeouts or transient failures,
- and avoid replaying different payloads with the same key.

The server may reject conflicting replays with a 409 when idempotency is enforced.
By default, keys are retained for 24 hours.

## API keys (service-to-service)

If `Auth:ApiKeys` is configured, requests may authenticate with:

- Header: `X-API-Key: <key>`

Valid API keys are granted the `admin` role for write access.

## Notifications (webhook)

If `Notifications:WebhookUrl` is configured, the API posts a JSON payload for key events:

- `job.created`, `job.status_changed`
- `booking.created`, `booking.status_changed`
- `provider.registered`, `provider.updated`
- `provider.availability_added`, `provider.availability_removed`
- `provider.availability_replaced`

## Caching

Provider availability lookups are cached in memory for a short TTL (default 30 seconds) to reduce repeated reads.

## Data retention

See `docs/DATA_RETENTION.md` for the current retention policy note.
