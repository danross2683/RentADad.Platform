# Secrets Management Guide

This project assumes secrets are never committed to source control. Use environment-level configuration or secret stores depending on where the service runs.

## Principles

- Do not commit secrets, tokens, or credentials to Git.
- Prefer short‑lived credentials and rotate regularly.
- Limit scope: each environment gets its own secrets.
- Audit access and changes.

## Local development

Use a `.env` file or your shell environment to supply secrets. We also support `dotnet user-secrets` for local-only values.

Recommended options:

- `.env` file (see `.env.example`), loaded by your shell or tooling.
- `dotnet user-secrets` for values you do not want in files.

Example:

```
dotnet user-secrets init
dotnet user-secrets set "Auth:SigningKey" "dev-only-change-me"
```

## CI/CD

Store secrets in your CI provider’s secret store and inject them as environment variables during build/deploy.

Minimum required secrets when `Auth:Enabled=true`:

- `Auth:Issuer`
- `Auth:Audience`
- `Auth:SigningKey`

## Production

Use a managed secret store and inject secrets at runtime:

- Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault.
- Rotate secrets regularly and keep access policies tight.

## Logs and telemetry

- Do not log secrets or full tokens.
- If you must log headers, redact `Authorization` and `X-API-Key`.

## Rotation checklist

- Rotate signing keys and API keys on a schedule.
- Update environments in order: staging, then production.
- Monitor authentication failures after rotation.

