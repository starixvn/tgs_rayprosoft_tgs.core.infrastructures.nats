using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class SubscribeBackground<T>(
    INatsConnection connection,
    IMessageHandler<T?> handler,
    IEnumerable<ISubjectObject<T>> subjectObjects) : BackgroundService where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var subjectObject in subjectObjects)
        {
            var sub = await connection.SubscribeCoreAsync<T>(subjectObject.Subject, subjectObject.QueueGroup,
                cancellationToken: stoppingToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in sub.Msgs.ReadAllAsync(stoppingToken))
                    {
                        var order = msg.Data;
                        await handler.Handle(subjectObject.Subject, subjectObject.QueueGroup, order, stoppingToken);
                    }

                    await sub.DisposeAsync();
                    Console.WriteLine("Unsubscribed");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }, stoppingToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}