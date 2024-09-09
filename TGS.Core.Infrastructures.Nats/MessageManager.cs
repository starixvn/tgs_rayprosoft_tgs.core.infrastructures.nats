using System.Threading;
using System.Threading.Tasks;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats;

public class MessageManager(INatsConnection connection, INatsJSContext natsJsContext) : IMessageManager
{
    public ValueTask PublishAsync<TData>(string subject, TData message,
        NatsHeaders? natsHeaders = null, INatsSerialize<TData>? natsSerialize = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PublishAsync(subject, message, natsHeaders, serializer: natsSerialize,
            cancellationToken: cancellationToken);
    }

    public ValueTask<NatsMsg<TResponse>> RequestReplyAsync<TData, TResponse>(string subject, TData message,
        NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? requestSerializer = null, INatsDeserialize<TResponse>? replySerializer = null,
        NatsSubOpts? replyOpts = null, CancellationToken cancellationToken = default)
    {
        return connection.RequestAsync(subject, message, natsHeaders, requestSerializer,
            replySerializer, replyOpts: replyOpts, cancellationToken: cancellationToken);
    }

    public ValueTask<PubAckResponse> JsPublishAsync<TData>(string subject, TData message, NatsJSPubOpts? opts = null,
        NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = default)
    {
        return natsJsContext.PublishAsync(subject, message, headers: natsHeaders, opts: opts, serializer: natsSerialize,
            cancellationToken: cancellationToken);
    }

    public ValueTask<NatsJSPublishConcurrentFuture> JsPublishConcurrentAsync<TData>(string subject, TData message,
        NatsJSPubOpts? opts = null, NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = default)
    {
        return natsJsContext.PublishConcurrentAsync(subject, message, headers: natsHeaders, opts: opts,
            serializer: natsSerialize,
            cancellationToken: cancellationToken);
    }
}