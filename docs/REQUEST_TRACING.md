# Request Tracing Guide

To trace a request end‑to‑end, include a correlation ID and retain it in logs.

## How to send a correlation ID

Send `X-Correlation-Id` with every request. If you do not supply one, the API will generate it.

Example:

```
X-Correlation-Id: 6fdb2f1e6f4d4c6b9d1b7f3f4f4d2b1a
```

## How to use it

- Log the correlation ID on the client side.
- Include it when contacting support.
- Use it to stitch together client logs and API logs.

