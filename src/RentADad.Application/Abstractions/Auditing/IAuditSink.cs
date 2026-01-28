using System.Threading;
using System.Threading.Tasks;

namespace RentADad.Application.Abstractions.Auditing;

public interface IAuditSink
{
    Task WriteAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}
