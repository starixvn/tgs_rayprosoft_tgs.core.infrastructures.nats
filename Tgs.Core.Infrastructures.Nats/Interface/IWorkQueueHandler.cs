using System.Threading;
using System.Threading.Tasks;
using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IWorkQueueHandler<in TData>
{
    Task Handle(string name, string subject, NatsHeaders? natsHeaders, TData? message,
        CancellationToken cancellationToken);
}