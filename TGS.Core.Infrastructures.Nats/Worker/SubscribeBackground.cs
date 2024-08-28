using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class SubscribeBackground<T>(
    INatsConnection connection,
    IMessageHandler<T?> handler,
    IEnumerable<ISubjectObject<T>> subjectObjects,
    ILogger<SubscribeBackground<T>> logger) : BackgroundService where T : class
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
                await foreach (var msg in sub.Msgs.ReadAllAsync(stoppingToken))
                    try
                    {
                        await handler.Handle(subjectObject.Subject, subjectObject.QueueGroup, msg.Headers, msg.Data,
                            stoppingToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Subscribe Error: {e}", e);
                    }

                await sub.DisposeAsync();
                logger.LogWarning("Unsubscribed");
            }, stoppingToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}