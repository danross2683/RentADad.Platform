using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Abstractions.Notifications;
using RentADad.Application.Abstractions.Auditing;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Bookings.Responses;
using RentADad.Application.Common.Paging;
using RentADad.Domain.Bookings;
using RentADad.Domain.Common;

namespace RentADad.Application.Bookings;

public sealed class BookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationSender _notifications;
    private readonly IAuditSink _auditSink;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookings, IUnitOfWork unitOfWork, INotificationSender notifications, IAuditSink auditSink, ILogger<BookingService> logger)
    {
        _bookings = bookings;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _auditSink = auditSink;
        _logger = logger;
    }

    public async Task<BookingResponse?> GetAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, cancellationToken);
        return booking is null ? null : ToResponse(booking);
    }

    public async Task<PagedResult<BookingResponse>> ListAsync(BookingListQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _bookings.ListAsync(query, cancellationToken);
        var items = paged.Items.Select(ToResponse).ToList();
        return new PagedResult<BookingResponse>(items, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateTimeWindow(request.StartUtc, request.EndUtc);

            var booking = new Booking(
                Guid.NewGuid(),
                request.JobId,
                request.ProviderId,
                request.StartUtc,
                request.EndUtc);

            _bookings.Add(booking);
            _logger.LogInformation("Booking created {BookingId} for job {JobId}", booking.Id, booking.JobId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("booking.created", new { booking.Id, booking.JobId }, cancellationToken);
            await _auditSink.WriteAsync("booking.created", new { booking.Id, booking.JobId, booking.Status }, cancellationToken);
            return ToResponse(booking);
        }
        catch (BookingDomainException)
        {
            throw;
        }
        catch (DomainRuleViolationException ex)
        {
            throw new BookingDomainException(ex.Message, MapBookingErrorCode(ex.Message));
        }
    }

    public Task<BookingResponse?> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Confirm());
    }

    public Task<BookingResponse?> DeclineAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Decline());
    }

    public Task<BookingResponse?> ExpireAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Expire());
    }

    public Task<BookingResponse?> CancelAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Cancel());
    }

    private async Task<BookingResponse?> ApplyAction(
        Guid bookingId,
        CancellationToken cancellationToken,
        Action<Booking> action)
    {
        var booking = await _bookings.GetForUpdateAsync(bookingId, cancellationToken);
        if (booking is null) return null;

        try
        {
            action(booking);
            _logger.LogInformation("Booking status changed {BookingId} -> {Status}", booking.Id, booking.Status);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("booking.status_changed", new { booking.Id, booking.Status }, cancellationToken);
            await _auditSink.WriteAsync("booking.status_changed", new { booking.Id, booking.Status }, cancellationToken);
            return ToResponse(booking);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new BookingDomainException(ex.Message, MapBookingErrorCode(ex.Message));
        }
    }

    private static BookingResponse ToResponse(Booking booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.JobId,
            booking.ProviderId,
            booking.StartUtc,
            booking.EndUtc,
            booking.Status.ToString(),
            booking.UpdatedUtc);
    }

    private static string MapBookingErrorCode(string message)
    {
        if (message.Contains("pending bookings can be confirmed", StringComparison.OrdinalIgnoreCase))
            return "booking_invalid_status_confirm";
        if (message.Contains("pending bookings can be declined", StringComparison.OrdinalIgnoreCase))
            return "booking_invalid_status_decline";
        if (message.Contains("pending bookings can expire", StringComparison.OrdinalIgnoreCase))
            return "booking_invalid_status_expire";
        if (message.Contains("confirmed bookings can be cancelled", StringComparison.OrdinalIgnoreCase))
            return "booking_invalid_status_cancel";
        if (message.Contains("Booking end must be after start", StringComparison.OrdinalIgnoreCase))
            return "booking_invalid_time_range";
        if (message.Contains("Booking id is required", StringComparison.OrdinalIgnoreCase))
            return "booking_id_required";
        if (message.Contains("Job id is required", StringComparison.OrdinalIgnoreCase))
            return "booking_job_required";
        if (message.Contains("Provider id is required", StringComparison.OrdinalIgnoreCase))
            return "booking_provider_required";

        return "booking_rule_violation";
    }

    private static void ValidateTimeWindow(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
            throw new BookingDomainException("StartUtc and EndUtc must be UTC.", "booking_time_not_utc");
        if (startUtc < DateTime.UtcNow)
            throw new BookingDomainException("StartUtc must be in the future.", "booking_time_in_past");
        if (endUtc <= startUtc)
            throw new BookingDomainException("EndUtc must be after StartUtc.", "booking_invalid_time_range");
    }
}
