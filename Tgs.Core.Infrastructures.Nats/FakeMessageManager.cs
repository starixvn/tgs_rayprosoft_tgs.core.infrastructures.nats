using System.Threading;
using System.Threading.Tasks;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats;

public class FakeMessageManager : IMessageManager
{
    public ValueTask PublishAsync<TData>(string subject, TData message, NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? natsSerialize = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<NatsMsg<TResponse>> RequestReplyAsync<TData, TResponse>(string subject, TData message, NatsHeaders? natsHeaders = null,
        INatsSerialize<TData>? requestSerializer = null, INatsDeserialize<TResponse>? replySerializer = null, NatsSubOpts? replyOpts = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(new NatsMsg<TResponse>());
    }

    public ValueTask<PubAckResponse> JsPublishAsync<TData>(string subject, TData message, NatsJSPubOpts? opts = null,
        NatsHeaders? natsHeaders = null, INatsSerialize<TData>? natsSerialize = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(new PubAckResponse());
    }

    public ValueTask<NatsJSPublishConcurrentFuture> JsPublishConcurrentAsync<TData>(string subject, TData message, NatsJSPubOpts? opts = null,
        NatsHeaders? natsHeaders = null, INatsSerialize<TData>? natsSerialize = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(new NatsJSPublishConcurrentFuture());
    }
}