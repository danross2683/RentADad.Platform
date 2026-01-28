# Logging Redaction Rules

This project avoids logging secrets or sensitive data. These rules apply to all request/response logging and any future audit or telemetry exports.

## Never log

- `Authorization` headers (JWT, bearer tokens)
- `X-API-Key` headers
- Cookies or session identifiers
- Full request/response bodies that may contain PII

## Allowed headers (default)

- `User-Agent`
- `Content-Type`
- `Accept`
- `X-Correlation-Id`
- Response `Content-Type`, `Content-Length`

## Redaction format

- Sensitive headers are replaced with `***`.
- Unknown headers are ignored unless explicitly allow‑listed.

## Notes

- If you need additional headers, add them to the allow‑list and verify they contain no secrets.
- Audit logs must follow the same redaction rules.

