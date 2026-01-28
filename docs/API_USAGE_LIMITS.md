# API Usage Limits

These limits are defaults and may change by contract.

## Default limits

- Global limit: 60 requests per minute per client.
- Burst: no queueing (excess requests return `429`).

## Guidance

- Use idempotency keys for retries.
- Back off on `429` responses with exponential delay.
- Contact support for higher limits or dedicated capacity.

