# Secrets Rotation Guide

This guide covers rotating keys and credentials without downtime.

## What to rotate

- JWT signing key (`Auth:SigningKey`)
- API keys (`Auth:ApiKeys`)
- Database credentials (`ConnectionStrings:Default`)
- Webhook secrets (if used)

## Rotation process

1. **Prepare** new secret values in the secret store.
2. **Deploy** a version that supports dual‑key validation (if applicable).
3. **Switch** the active secret in production.
4. **Monitor** authentication and error rates.
5. **Remove** the old secret after the grace period.

## JWT signing key

- Prefer a dual‑key approach (current + previous) when rotating.
- Ensure token TTL is short enough for safe cut‑over.

## API keys

- Issue new keys, update clients, then revoke old keys.
- Keep key scopes limited to required access.

## Database credentials

- Rotate credentials during low‑traffic windows.
- Verify connection pool recycles after rotation.

## Post‑rotation checks

- Confirm authentication success rate and latency.
- Run smoke tests and verify health endpoints.

