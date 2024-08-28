using System.Threading;
using System.Threading.Tasks;
using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IMessageReplyHandler<in TData, TResponse>
{
    Task<TResponse> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, TData? message,
        CancellationToken cancellationToken);
}