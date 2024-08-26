using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class WorkQueueBackground<T>(
    INatsJSContext js,
    IWorkQueueHandler<T?> handler,
    IEnumerable<IWorkQueue<T>> queues) : BackgroundService where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumers = new List<INatsJSConsumer>();

        foreach (var queue in queues)
        {
            INatsJSStream stream;

            var config = new StreamConfig(queue.StreamName, queue.Subjects.ToArray())
            {
                Retention = StreamConfigRetention.Workqueue
            };

            try
            {
                stream = await js.GetStreamAsync(queue.StreamName, cancellationToken: stoppingToken);
            }
            catch
            {
                stream = await js.CreateStreamAsync(config, stoppingToken);
            }

            var durable =
                await stream.CreateOrUpdateConsumerAsync(new ConsumerConfig("processor"),
                    stoppingToken);

            consumers.Add(durable);
        }

        foreach (var consumer in consumers)
        await foreach (var msg in consumer.ConsumeAsync<T>(cancellationToken: stoppingToken))
            try
            {
                await handler.Handle(consumer.Info.Name, consumer.Info.Name, msg.Data, stoppingToken);
                await msg.AckAsync(new AckOpts { DoubleAck = true }, stoppingToken);
            }
            catch (Exception e)
            {
                await msg.NakAsync(new AckOpts { DoubleAck = true }, cancellationToken: stoppingToken);
                Console.WriteLine(e);
            }
    }
}