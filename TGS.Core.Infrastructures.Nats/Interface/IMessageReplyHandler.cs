using System.Threading;
using System.Threading.Tasks;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IMessageReplyHandler<in TData, TResponse>
{
    Task<TResponse> Handle(string subject, string? queueGroup, TData? message, CancellationToken cancellationToken);
}