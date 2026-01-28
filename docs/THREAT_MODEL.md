# Threat Model Notes

This is a lightweight threat model focused on entry points and trust boundaries.

## Entry points

- Public API endpoints (`/api/v1/*`)
- Health endpoints (`/health/live`, `/health/ready`)
- Admin endpoints (`/api/v1/admin/*`)
- Auth dev token endpoint (Development only)

## Trust boundaries

- Internet → API (public boundary)
- API → Database (trusted internal boundary)
- API → Notification webhook (external boundary)
- API → Observability exporter (external boundary)

## Key risks

- Credential leakage (JWT signing key, API keys)
- Injection via request payloads
- Privilege escalation via role misuse
- Data exposure via logs
- Dependency vulnerabilities

## Mitigations (current)

- JWT auth + role‑based authorisation for write/admin paths
- API key support for service‑to‑service calls
- Request validation with FluentValidation
- Security headers + HTTPS redirection
- Vulnerability scanning + licence reporting in CI
- Redacted logging rules

## Gaps / follow‑ups

- Rate limit tuning for abuse scenarios
- WAF/CDN configuration (if deployed publicly)
- Secret rotation automation

