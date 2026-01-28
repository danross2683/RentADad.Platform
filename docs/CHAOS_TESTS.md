# Chaos Test Checklist

Use this checklist to validate resilience before major releases.

## Database failure

- [ ] Stop database or block network access.
- [ ] Verify `/health/ready` fails quickly.
- [ ] Confirm API returns 500/503 for write paths.
- [ ] Restore database and confirm recovery.

## Webhook failures

- [ ] Point `Notifications:WebhookUrl` to a non‑responsive endpoint.
- [ ] Verify API requests still succeed.
- [ ] Confirm failure is logged and does not crash requests.

## External metrics exporter down

- [ ] Disable OTLP endpoint.
- [ ] Confirm API remains healthy and logs show exporter warnings.

## Rate limiting

- [ ] Exceed 60 req/min from a single client.
- [ ] Confirm `429` responses and no crashes.

