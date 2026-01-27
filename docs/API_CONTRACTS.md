# API Contracts (v1)

This document provides request/response examples for the current API surface area.
For OpenAPI, run the API in Development and use the OpenAPI document exposed by the app.

## Base

- Base path: `/api/v1`
- Content type: `application/json`
- Dates: ISO-8601 UTC (`2026-01-27T12:00:00Z`)

## Jobs

### Create Job

`POST /api/v1/jobs`

Request:
```json
{
  "customerId": "c4108f2c-9b0b-45c3-8fa6-fb6c2b0e17a2",
  "location": "Wellington",
  "serviceIds": [
    "0e0fd1e0-6f45-4b92-b8a2-35a8dfb5b91f",
    "6b6a1f2e-27c4-4978-bb4b-1ad39ad6e8a0"
  ]
}
```

Response (201):
```json
{
  "id": "bc1270b0-6f54-4bce-b64a-b6031c50d93b",
  "customerId": "c4108f2c-9b0b-45c3-8fa6-fb6c2b0e17a2",
  "location": "Wellington",
  "serviceIds": [
    "0e0fd1e0-6f45-4b92-b8a2-35a8dfb5b91f",
    "6b6a1f2e-27c4-4978-bb4b-1ad39ad6e8a0"
  ],
  "status": "Draft",
  "activeBookingId": null
}
```

### Update Job (replace)

`PUT /api/v1/jobs/{jobId}`

Request:
```json
{
  "location": "Auckland",
  "serviceIds": [
    "0e0fd1e0-6f45-4b92-b8a2-35a8dfb5b91f"
  ]
}
```

### Patch Job

`PATCH /api/v1/jobs/{jobId}`

Request:
```json
{
  "location": "Hamilton"
}
```

### Job Actions

- `POST /api/v1/jobs/{jobId}:post`
- `POST /api/v1/jobs/{jobId}:accept`
- `POST /api/v1/jobs/{jobId}:start`
- `POST /api/v1/jobs/{jobId}:complete`
- `POST /api/v1/jobs/{jobId}:close`
- `POST /api/v1/jobs/{jobId}:dispute`
- `POST /api/v1/jobs/{jobId}:cancel`

Accept request:
```json
{
  "bookingId": "e9bdf01c-0b16-4d36-ae01-329f6c2f02b9"
}
```

Response (200): `JobResponse` (same shape as Create response).

### Get Job

- `GET /api/v1/jobs`
- `GET /api/v1/jobs/{jobId}`

Response (200):
```json
{
  "id": "bc1270b0-6f54-4bce-b64a-b6031c50d93b",
  "customerId": "c4108f2c-9b0b-45c3-8fa6-fb6c2b0e17a2",
  "location": "Wellington",
  "serviceIds": [
    "0e0fd1e0-6f45-4b92-b8a2-35a8dfb5b91f"
  ],
  "status": "Posted",
  "activeBookingId": null
}
```

## Bookings

### Create Booking

`POST /api/v1/bookings`

Request:
```json
{
  "jobId": "bc1270b0-6f54-4bce-b64a-b6031c50d93b",
  "providerId": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "startUtc": "2026-01-30T08:00:00Z",
  "endUtc": "2026-01-30T10:00:00Z"
}
```

Response (201):
```json
{
  "id": "e9bdf01c-0b16-4d36-ae01-329f6c2f02b9",
  "jobId": "bc1270b0-6f54-4bce-b64a-b6031c50d93b",
  "providerId": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "startUtc": "2026-01-30T08:00:00Z",
  "endUtc": "2026-01-30T10:00:00Z",
  "status": "Pending"
}
```

### Booking Actions

- `POST /api/v1/bookings/{bookingId}:confirm`
- `POST /api/v1/bookings/{bookingId}:decline`
- `POST /api/v1/bookings/{bookingId}:expire`
- `POST /api/v1/bookings/{bookingId}:cancel`

Response (200): `BookingResponse` (same shape as Create response).

### Get Booking

`GET /api/v1/bookings/{bookingId}`

Response (200):
```json
{
  "id": "e9bdf01c-0b16-4d36-ae01-329f6c2f02b9",
  "jobId": "bc1270b0-6f54-4bce-b64a-b6031c50d93b",
  "providerId": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "startUtc": "2026-01-30T08:00:00Z",
  "endUtc": "2026-01-30T10:00:00Z",
  "status": "Confirmed"
}
```

## Providers

### Register Provider

`POST /api/v1/providers`

Request:
```json
{
  "providerId": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "displayName": "Rent-A-Dad Joe"
}
```

Response (201):
```json
{
  "id": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "displayName": "Rent-A-Dad Joe",
  "availabilities": []
}
```

### Update Provider

`PUT /api/v1/providers/{providerId}`

Request:
```json
{
  "displayName": "Rent-A-Dad Joseph"
}
```

### Add Availability

`POST /api/v1/providers/{providerId}/availability`

Request:
```json
{
  "startUtc": "2026-02-01T09:00:00Z",
  "endUtc": "2026-02-01T12:00:00Z"
}
```

Response (200):
```json
{
  "id": "a1b7a0d2-7b2a-47e3-8c58-a3d69246c64c",
  "displayName": "Rent-A-Dad Joe",
  "availabilities": [
    {
      "id": "4a7d3270-4b1b-4f5a-9b8f-19b80d95f7a2",
      "startUtc": "2026-02-01T09:00:00Z",
      "endUtc": "2026-02-01T12:00:00Z"
    }
  ]
}
```

### Remove Availability

`DELETE /api/v1/providers/{providerId}/availability/{availabilityId}`

Response (200): `ProviderResponse` (same shape as Add Availability response).

### Get Provider

`GET /api/v1/providers/{providerId}`

Response (200): `ProviderResponse`.

## Errors

All errors use ProblemDetails (RFC 7807). Domain errors include an `errorCode` extension.

Example:
```json
{
  "type": "about:blank",
  "title": "Domain rule violation",
  "status": 409,
  "detail": "Availability windows must not overlap.",
  "errorCode": "provider_availability_overlap"
}
```
