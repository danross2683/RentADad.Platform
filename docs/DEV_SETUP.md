# Dev Setup

This project is API-only. Use the steps below to run locally.

## Prerequisites

- .NET SDK 10 LTS
- Docker (for PostgreSQL)

## Environment variables

Set these before running the API:

- `ASPNETCORE_ENVIRONMENT=Development`
- `RentADad_ConnectionStrings__Default=Host=localhost;Port=5432;Database=rentadad;Username=postgres;Password=postgres`
- Optional: `RentADad_Auth__Enabled=true` (requires issuer/audience/signing key)
  - `RentADad_Auth__Issuer=rentadad.local`
  - `RentADad_Auth__Audience=rentadad.local`
  - `RentADad_Auth__SigningKey=dev-only-change-me`
- Or copy `.env.example` to `.env` and load it via your shell.

## Ports

- API: `http://localhost:5080` (or the value from `launchSettings.json` when added)
- PostgreSQL: `localhost:5432`

## Secrets

For local development, prefer user-secrets or environment variables. Never commit secrets.

## Run locally

```shell
./scripts/dev-bootstrap.ps1
dotnet run --project src/RentADad.Api
```

## Task runner

We use `just` for common tasks:

```shell
just restore
just build
just test
just migrate
just seed-demo
just run
```
