using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Abstractions.Notifications;
using RentADad.Application.Abstractions.Auditing;
using RentADad.Application.Abstractions.ReadModels;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.Requests;
using RentADad.Application.Jobs.ReadModels;
using RentADad.Application.Jobs.Responses;
using RentADad.Domain.Common;
using RentADad.Domain.Jobs;

namespace RentADad.Application.Jobs;

public sealed class JobService
{
    private readonly IJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationSender _notifications;
    private readonly IAuditSink _auditSink;
    private readonly IJobListingReader _jobListings;
    private readonly IJobListingWriter _jobListingWriter;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository jobs,
        IUnitOfWork unitOfWork,
        INotificationSender notifications,
        IAuditSink auditSink,
        IJobListingReader jobListings,
        IJobListingWriter jobListingWriter,
        ILogger<JobService> logger)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _auditSink = auditSink;
        _jobListings = jobListings;
        _jobListingWriter = jobListingWriter;
        _logger = logger;
    }

    public async Task<List<JobResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _jobs.ListAsync(cancellationToken);
        return jobs.Select(ToResponse).ToList();
    }

    public async Task<PagedResult<JobResponse>> ListAsync(JobListQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _jobListings.ListAsync(query, cancellationToken);
        var items = paged.Items.Select(ToResponse).ToList();
        return new PagedResult<JobResponse>(items, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<JobResponse?> GetAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _jobs.GetByIdAsync(jobId, cancellationToken);
        return job is null ? null : ToResponse(job);
    }

    public async Task<JobResponse> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = new Job(
                Guid.NewGuid(),
                request.CustomerId,
                request.Location ?? string.Empty,
                request.ServiceIds ?? new List<Guid>());

            _logger.LogInformation("Job created {JobId} for customer {CustomerId}", job.Id, job.CustomerId);
            _jobs.Add(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("job.created", new { job.Id, job.CustomerId }, cancellationToken);
            await _auditSink.WriteAsync("job.created", new { job.Id, job.CustomerId, job.Status }, cancellationToken);
            await _jobListingWriter.UpsertAsync(ToWriteModel(job), cancellationToken);
            return ToResponse(job);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new JobDomainException(ex.Message, MapJobErrorCode(ex.Message));
        }
    }

    public async Task<JobResponse?> UpdateAsync(Guid jobId, UpdateJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobs.GetForUpdateAsync(jobId, cancellationToken);
        if (job is null) return null;

        try
        {
            job.UpdateLocation(request.Location ?? string.Empty);
            job.ClearServices();
            foreach (var serviceId in request.ServiceIds ?? new List<Guid>())
            {
                job.AddService(serviceId);
            }

            _logger.LogInformation("Job updated {JobId}", job.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _jobListingWriter.UpsertAsync(ToWriteModel(job), cancellationToken);
            return ToResponse(job);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new JobDomainException(ex.Message, MapJobErrorCode(ex.Message));
        }
    }

    public async Task<JobResponse?> PatchAsync(Guid jobId, PatchJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobs.GetForUpdateAsync(jobId, cancellationToken);
        if (job is null) return null;

        try
        {
            if (request.Location is not null)
            {
                job.UpdateLocation(request.Location);
            }

            if (request.ServiceIds is not null)
            {
                job.ClearServices();
                foreach (var serviceId in request.ServiceIds)
                {
                    job.AddService(serviceId);
                }
            }

            _logger.LogInformation("Job patched {JobId}", job.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _jobListingWriter.UpsertAsync(ToWriteModel(job), cancellationToken);
            return ToResponse(job);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new JobDomainException(ex.Message, MapJobErrorCode(ex.Message));
        }
    }

    public Task<JobResponse?> PostAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Post());
    }

    public Task<JobResponse?> AcceptAsync(Guid jobId, AcceptJobRequest request, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Accept(request.BookingId));
    }

    public Task<JobResponse?> StartAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Start());
    }

    public Task<JobResponse?> CompleteAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Complete());
    }

    public Task<JobResponse?> CloseAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Close());
    }

    public Task<JobResponse?> DisputeAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Dispute());
    }

    public Task<JobResponse?> CancelAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(jobId, cancellationToken, job => job.Cancel());
    }

    private async Task<JobResponse?> ApplyAction(
        Guid jobId,
        CancellationToken cancellationToken,
        Action<Job> action)
    {
        var job = await _jobs.GetForUpdateAsync(jobId, cancellationToken);
        if (job is null) return null;

        try
        {
            action(job);
            _logger.LogInformation("Job lifecycle transition {JobId} -> {Status}", job.Id, job.Status);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("job.status_changed", new { job.Id, job.Status }, cancellationToken);
            await _auditSink.WriteAsync("job.status_changed", new { job.Id, job.Status }, cancellationToken);
            await _jobListingWriter.UpsertAsync(ToWriteModel(job), cancellationToken);
            return ToResponse(job);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new JobDomainException(ex.Message, MapJobErrorCode(ex.Message));
        }
    }

    private static JobResponse ToResponse(Job job)
    {
        return new JobResponse(
            job.Id,
            job.CustomerId,
            job.Location,
            job.ServiceIds.ToArray(),
            job.Status.ToString(),
            job.ActiveBookingId,
            job.UpdatedUtc);
    }

    private static JobResponse ToResponse(JobListingRow row)
    {
        return new JobResponse(
            row.Id,
            row.CustomerId,
            row.Location,
            row.ServiceIds,
            row.Status,
            row.ActiveBookingId,
            row.UpdatedUtc);
    }

    private static JobListingWriteModel ToWriteModel(Job job)
    {
        return new JobListingWriteModel(
            job.Id,
            job.CustomerId,
            job.Location,
            job.ServiceIds.ToArray(),
            job.Status.ToString(),
            job.ActiveBookingId,
            job.UpdatedUtc);
    }

    private static string MapJobErrorCode(string message)
    {
        if (message.Contains("draft jobs can be posted", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_post";
        if (message.Contains("posted jobs can be accepted", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_accept";
        if (message.Contains("accepted jobs can start", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_start";
        if (message.Contains("in-progress jobs can be completed", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_complete";
        if (message.Contains("completed jobs can be closed", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_close";
        if (message.Contains("completed jobs can be disputed", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_dispute";
        if (message.Contains("draft, posted, or accepted jobs can be cancelled", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_cancel";
        if (message.Contains("Only draft jobs can be modified", StringComparison.OrdinalIgnoreCase))
            return "job_invalid_status_update";
        if (message.Contains("At least one service is required", StringComparison.OrdinalIgnoreCase))
            return "job_services_required";
        if (message.Contains("Location is required", StringComparison.OrdinalIgnoreCase))
            return "job_location_required";
        if (message.Contains("Booking id is required", StringComparison.OrdinalIgnoreCase))
            return "job_booking_required";

        return "job_rule_violation";
    }
}
