# Domain Rules

This document captures the initial domain modeling decisions.

## Job lifecycle

### States

- Draft
- Posted
- Accepted
- InProgress
- Completed
- Closed
- Disputed
- Cancelled

### Transitions

- Draft -> Posted
- Posted -> Accepted
- Accepted -> InProgress
- InProgress -> Completed
- Completed -> Closed
- Completed -> Disputed
- Draft -> Cancelled
- Posted -> Cancelled
- Accepted -> Cancelled

### Notes

- A job cannot be Posted without at least one service and a location.
- A job cannot be Accepted without a confirmed booking.
- Completed jobs can be Closed by the customer, or Disputed within a policy window.
- Cancelled is terminal.

## Booking invariants

- A booking is always tied to one Job and one Provider.
- A job may have at most one Active booking.
- Active booking statuses: Pending, Confirmed.
- Only a Confirmed booking can move a job to Accepted.
- A booking cannot be Confirmed if the provider is unavailable in that time window.
- Expired bookings are terminal and cannot be Confirmed or Declined afterward.

## Provider availability

- Providers define availability windows with start and end times.
- Availability windows must not overlap.
- A booking can only be requested within an availability window.
- Once a booking is Confirmed, the provider is blocked for that time range.
- Blocking is enforced even if the job later gets Cancelled or Disputed, until explicitly released.

## Domain events

Emit events for state changes and time-driven policies:

- JobPosted
- JobAccepted
- JobStarted
- JobCompleted
- JobClosed
- JobDisputed
- JobCancelled
- BookingCreated
- BookingConfirmed
- BookingDeclined
- BookingExpired
- ProviderAvailabilityAdded
- ProviderAvailabilityRemoved
