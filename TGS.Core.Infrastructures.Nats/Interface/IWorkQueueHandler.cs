using System.Threading;
using System.Threading.Tasks;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IWorkQueueHandler<in TData>
{
    Task Handle(string name, string subject, TData? message, CancellationToken cancellationToken);
}