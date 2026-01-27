# Dev Setup

This project is API-only. Use the steps below to run locally.

## Prerequisites

- .NET SDK 10 LTS
- Docker (for PostgreSQL)

## Environment variables

Set these before running the API:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__Default=Host=localhost;Port=5432;Database=rentadad;Username=postgres;Password=postgres`
- `Jwt__Issuer=rentadad.local`
- `Jwt__Audience=rentadad.local`
- `Jwt__Key=dev-only-change-me`

## Ports

- API: `http://localhost:5080` (or the value from `launchSettings.json` when added)
- PostgreSQL: `localhost:5432`

## Secrets

For local development, prefer user-secrets or environment variables. Never commit secrets.

## Run locally

```shell
docker compose up -d
dotnet restore
dotnet ef database update
dotnet run --project src/RentADad.Api
```
