using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.ReadModels;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Application.Abstractions.ReadModels;

public interface IJobListingReader
{
    Task<PagedResult<JobListingRow>> ListAsync(JobListQuery query, CancellationToken cancellationToken = default);
}
