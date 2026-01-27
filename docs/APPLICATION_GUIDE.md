# Application Layer Guide

This document defines the application layer scope and patterns.

## Core use cases (v1)

### Jobs

- CreateJob (Draft)
- UpdateJob (Draft)
- PostJob
- CancelJob
- StartJob
- CompleteJob
- CloseJob
- DisputeJob

### Bookings

- CreateBooking (Pending)
- ConfirmBooking
- DeclineBooking
- ExpireBooking
- CancelBooking

### Providers

- RegisterProvider
- UpdateProviderProfile
- AddAvailability
- RemoveAvailability

## DTOs and validation

- Request/response DTOs mirror use cases, not EF entities.
- Validate input in the application layer before invoking domain logic.
- Prefer explicit validators per request type.
- Use clear error codes for domain rule violations.

## Error handling

- Return RFC 7807 problem details for application errors.
- Use standard error categories: `validation`, `conflict`, `not_found`, `forbidden`.
- Map domain rule violations to `409 Conflict` with stable error codes.
- Map validation errors to `400 Bad Request` with field-level details.
