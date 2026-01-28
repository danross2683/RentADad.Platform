using System.Threading;
using System.Threading.Tasks;

namespace RentADad.Application.Abstractions.Notifications;

public interface INotificationSender
{
    Task NotifyAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}
