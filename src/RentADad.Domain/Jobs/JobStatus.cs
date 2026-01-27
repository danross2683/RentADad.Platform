namespace RentADad.Domain.Jobs;

public enum JobStatus
{
    Draft = 0,
    Posted = 1,
    Accepted = 2,
    InProgress = 3,
    Completed = 4,
    Closed = 5,
    Disputed = 6,
    Cancelled = 7
}
