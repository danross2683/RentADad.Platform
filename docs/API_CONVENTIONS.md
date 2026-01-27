# API Conventions

This document defines the initial API surface area and naming conventions.

## Base

- Base path: `/api/v1`
- Content type: `application/json`

## Resource naming

- Use plural nouns for collections: `jobs`, `bookings`, `providers`
- Use kebab-case for multi-word segments: `job-types`, `provider-availability`
- Use nested resources only when the child has no independent lifecycle

## Common patterns

- `GET /api/v1/{resource}` list
- `GET /api/v1/{resource}/{id}` get by id
- `POST /api/v1/{resource}` create
- `PUT /api/v1/{resource}/{id}` replace
- `PATCH /api/v1/{resource}/{id}` partial update
- `DELETE /api/v1/{resource}/{id}` delete

## Core resources

- `jobs`
- `bookings`
- `providers`
- `customers`
- `services`

## Initial endpoints (v1)

### Jobs

- `GET /api/v1/jobs`
- `GET /api/v1/jobs/search?page=1&pageSize=50&status=Posted&customerId=...`
- `GET /api/v1/jobs/{jobId}`
- `POST /api/v1/jobs`
- `PUT /api/v1/jobs/{jobId}`
- `PATCH /api/v1/jobs/{jobId}`
- `POST /api/v1/jobs/{jobId}:post`
- `POST /api/v1/jobs/{jobId}:accept`
- `POST /api/v1/jobs/{jobId}:start`
- `POST /api/v1/jobs/{jobId}:complete`
- `POST /api/v1/jobs/{jobId}:close`
- `POST /api/v1/jobs/{jobId}:dispute`
- `POST /api/v1/jobs/{jobId}:cancel`

### Bookings

- `GET /api/v1/bookings/search?page=1&pageSize=50&status=Pending&jobId=...&providerId=...`
- `GET /api/v1/bookings/{bookingId}`
- `POST /api/v1/bookings`
- `PUT /api/v1/bookings/{bookingId}`
- `PATCH /api/v1/bookings/{bookingId}`
- `POST /api/v1/bookings/{bookingId}:confirm`
- `POST /api/v1/bookings/{bookingId}:decline`
- `POST /api/v1/bookings/{bookingId}:expire`
- `POST /api/v1/bookings/{bookingId}:cancel`

### Providers

- `GET /api/v1/providers/search?page=1&pageSize=50&displayName=...`
- `GET /api/v1/providers/{providerId}`
- `POST /api/v1/providers`
- `PUT /api/v1/providers/{providerId}`
- `PATCH /api/v1/providers/{providerId}`

### Customers

- `GET /api/v1/customers`
- `GET /api/v1/customers/{customerId}`
- `POST /api/v1/customers`
- `PUT /api/v1/customers/{customerId}`
- `PATCH /api/v1/customers/{customerId}`

### Services

- `GET /api/v1/services`
- `GET /api/v1/services/{serviceId}`
- `POST /api/v1/services`
- `PUT /api/v1/services/{serviceId}`
- `PATCH /api/v1/services/{serviceId}`

## Notes

- State transitions are modeled as explicit actions using `:verb` endpoints.
- Use standard RFC 7807 problem details for errors.
- Prefer UUIDs for identifiers.
