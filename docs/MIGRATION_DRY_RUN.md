# Production Migration Dry‑Run Guide

Use this before applying migrations to production.

## Goals

- Validate that migrations apply cleanly.
- Estimate run time and locking behaviour.
- Verify application compatibility.

## Steps

1. Restore production backup to a staging database.
2. Run migrations against staging:

```
dotnet run --project ./src/RentADad.Api/RentADad.Api.csproj -- --apply-migrations-only
```

3. Confirm migrations completed without errors.
4. Run smoke tests against staging.
5. Record migration time and any warnings.

## Notes

- If migration time is high, schedule a longer maintenance window.
- Prefer forward‑fix migrations over rollbacks.

