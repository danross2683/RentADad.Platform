# Production Environment Variables Checklist

Use this checklist before first production deploy and after any change to configuration. Values are supplied via environment variables (preferred) or secret store injection.

## Required

- `ConnectionStrings:Default`
  - PostgreSQL connection string for production database.
- `Auth:Enabled`
  - `true` in production.
- `Auth:Issuer`
- `Auth:Audience`
- `Auth:SigningKey`

## Recommended

- `Database:AutoMigrate`
  - `false` in production if you run migrations separately.
- `OpenTelemetry:OtlpEndpoint`
  - Endpoint for tracing/metrics export.
- `Notifications:WebhookUrl`
  - Outbound webhook for notifications (if used).
- `Caching:ProviderAvailabilitySeconds`
  - Cache TTL for provider availability reads (default 30).

## Optional

- `Auth:ApiKey`
  - Required only if API key auth is enabled.
- `ASPNETCORE_URLS`
  - Hosting URL binding (e.g. `http://0.0.0.0:8080`).
- `ASPNETCORE_ENVIRONMENT`
  - Set to `Production` for live deployments.

## Quick verification

- Confirm secrets are injected from the secret store and are not present in files.
- Verify `Auth:Enabled=true` and the signing key is rotated before go‑live.
- Confirm OTLP endpoint is reachable from the runtime network.

