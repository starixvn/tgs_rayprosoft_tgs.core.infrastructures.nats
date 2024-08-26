using System.Threading;
using System.Threading.Tasks;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IMessageHandler<in TData>
{
    Task Handle(string subject, string? queueGroup, TData? message, CancellationToken cancellationToken);
}