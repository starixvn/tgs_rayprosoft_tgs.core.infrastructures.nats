using System.Threading;
using System.Threading.Tasks;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IMessageManager
{
    ValueTask PublishAsync<TData>(string subject, TData message, NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = default);

    ValueTask<NatsMsg<TResponse>> RequestReplyAsync<TData, TResponse>(string subject, TData message,
        NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? requestSerializer = default,
        INatsDeserialize<TResponse>? replySerializer = default,
        NatsSubOpts? replyOpts = default, CancellationToken cancellationToken = default);

    ValueTask<PubAckResponse> JsPublishAsync<TData>(string subject, TData message, NatsJSPubOpts? opts = null,
        NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = default);

    ValueTask<NatsJSPublishConcurrentFuture> JsPublishConcurrentAsync<TData>(string subject, TData message,
        NatsJSPubOpts? opts = null, NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = default);
}