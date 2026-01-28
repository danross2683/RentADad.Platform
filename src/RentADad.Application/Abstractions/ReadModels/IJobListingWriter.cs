using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Jobs.ReadModels;

namespace RentADad.Application.Abstractions.ReadModels;

public interface IJobListingWriter
{
    Task UpsertAsync(JobListingWriteModel model, CancellationToken cancellationToken = default);
}
