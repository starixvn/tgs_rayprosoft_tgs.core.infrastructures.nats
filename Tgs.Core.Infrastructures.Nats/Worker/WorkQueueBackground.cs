using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class WorkQueueBackground<T>(
    INatsJSContext js,
    IWorkQueueHandler<T?> handler,
    IEnumerable<IWorkQueue<T>> queues,
    ILogger<WorkQueueBackground<T>> logger) : BackgroundService where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var works = new List<(INatsJSConsumer consumer, IWorkQueue<T> workQueue)>();

        foreach (var queue in queues)
        {
            INatsJSStream stream;

            var config = new StreamConfig(queue.Stream, queue.Subjects.ToArray())
            {
                Retention = StreamConfigRetention.Workqueue
            };

            try
            {
                stream = await js.GetStreamAsync(queue.Stream, cancellationToken: stoppingToken);
            }
            catch
            {
                stream = await js.CreateStreamAsync(config, stoppingToken);
            }

            var durable =
                await stream.CreateOrUpdateConsumerAsync(new ConsumerConfig("processor"),
                    stoppingToken);

            works.Add((durable, queue));
        }

        foreach (var (consumer, workQueue) in works)
        await foreach (var msg in consumer.ConsumeAsync(workQueue.Serializer, cancellationToken: stoppingToken))
            try
            {
                await handler.Handle(consumer.Info.Name, consumer.Info.Name, msg.Headers, msg.Data, stoppingToken);
                await msg.AckAsync(new AckOpts { DoubleAck = true }, stoppingToken);
                msg.EnsureSuccess();
            }
            catch (Exception e)
            {
                logger.LogError("WorkQueue Error: {e}", e);

                logger.LogError("WorkQueue Error Message: {msg}", msg.Data);

                var nakDelayTime = workQueue.NakDelayTime;

                if (nakDelayTime is not null)
                    await msg.NakAsync(delay: nakDelayTime.Value, cancellationToken: stoppingToken);
            }
    }
}