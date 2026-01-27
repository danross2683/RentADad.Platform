# API Guide

This document defines the initial API structure and policies.

## Endpoints

See `docs/API_CONVENTIONS.md` for the v1 endpoints list and naming.

## Contracts

See `docs/API_CONTRACTS.md` for request/response examples.

## Auth scaffolding (JWT)

- Use bearer token authentication.
- Configure issuer, audience, and signing key via configuration.
- Protect all write endpoints by default.
- Use policies for role or capability-based access.

## OpenAPI

- Enable OpenAPI/Swagger for local development.
- Include API version in the document title and path.
- Use operation tags per resource: `Jobs`, `Bookings`, `Providers`, `Customers`, `Services`.

## Errors

- Use RFC 7807 ProblemDetails.
- Include a stable `errorCode` extension for domain errors.
