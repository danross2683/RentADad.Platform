using System;
using System.Collections.Generic;
using System.Linq;
using RentADad.Domain.Common;

namespace RentADad.Domain.Jobs;

public sealed class Job
{
    private readonly List<JobService> _services = new();

    private Job()
    {
        Location = string.Empty;
        UpdatedUtc = DateTime.UtcNow;
    }

    public Job(Guid id, Guid customerId, string location, IEnumerable<Guid> serviceIds)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Job id is required.");
        if (customerId == Guid.Empty) throw new DomainRuleViolationException("Customer id is required.");
        if (serviceIds == null) throw new DomainRuleViolationException("Service ids are required.");

        Id = id;
        CustomerId = customerId;
        Location = location ?? string.Empty;
        _services.AddRange(serviceIds.Where(s => s != Guid.Empty).Select(id => new JobService(Guid.NewGuid(), id)));
        Status = JobStatus.Draft;
        UpdatedUtc = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public string Location { get; private set; }
    public IReadOnlyCollection<Guid> ServiceIds => _services.Select(service => service.ServiceId).ToList();
    public IReadOnlyCollection<JobService> Services => _services;
    public JobStatus Status { get; private set; }
    public Guid? ActiveBookingId { get; private set; }
    public DateTime UpdatedUtc { get; private set; }

    public void UpdateLocation(string location)
    {
        EnsureDraft();
        Location = location ?? string.Empty;
    }

    public void AddService(Guid serviceId)
    {
        EnsureDraft();
        if (serviceId == Guid.Empty) throw new DomainRuleViolationException("Service id is required.");
        if (_services.Any(service => service.ServiceId == serviceId)) return;
        _services.Add(new JobService(Guid.NewGuid(), serviceId));
    }

    public void RemoveService(Guid serviceId)
    {
        EnsureDraft();
        var service = _services.FirstOrDefault(item => item.ServiceId == serviceId);
        if (service == null) return;
        _services.Remove(service);
    }

    public void ClearServices()
    {
        EnsureDraft();
        _services.Clear();
    }

    public void Post()
    {
        EnsureStatus(JobStatus.Draft, "Only draft jobs can be posted.");
        if (_services.Count == 0) throw new DomainRuleViolationException("At least one service is required.");
        if (string.IsNullOrWhiteSpace(Location)) throw new DomainRuleViolationException("Location is required.");
        Status = JobStatus.Posted;
    }

    public void Accept(Guid bookingId)
    {
        EnsureStatus(JobStatus.Posted, "Only posted jobs can be accepted.");
        if (bookingId == Guid.Empty) throw new DomainRuleViolationException("Booking id is required.");
        ActiveBookingId = bookingId;
        Status = JobStatus.Accepted;
    }

    public void Start()
    {
        EnsureStatus(JobStatus.Accepted, "Only accepted jobs can start.");
        Status = JobStatus.InProgress;
    }

    public void Complete()
    {
        EnsureStatus(JobStatus.InProgress, "Only in-progress jobs can be completed.");
        Status = JobStatus.Completed;
    }

    public void Close()
    {
        EnsureStatus(JobStatus.Completed, "Only completed jobs can be closed.");
        Status = JobStatus.Closed;
    }

    public void Dispute()
    {
        EnsureStatus(JobStatus.Completed, "Only completed jobs can be disputed.");
        Status = JobStatus.Disputed;
    }

    public void Cancel()
    {
        if (Status is JobStatus.Draft or JobStatus.Posted or JobStatus.Accepted)
        {
            Status = JobStatus.Cancelled;
            return;
        }

        throw new DomainRuleViolationException("Only draft, posted, or accepted jobs can be cancelled.");
    }

    private void EnsureDraft()
    {
        EnsureStatus(JobStatus.Draft, "Only draft jobs can be modified.");
    }

    private void EnsureStatus(JobStatus expected, string message)
    {
        if (Status != expected) throw new DomainRuleViolationException(message);
    }
}
