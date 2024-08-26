using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class RequestReplyBackground<T, TResponse>(
    INatsConnection connection,
    IMessageReplyHandler<T?, TResponse> handler,
    IEnumerable<ISubjectObject<T, TResponse>> subjectObjects)
    : BackgroundService where T : class where TResponse : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var subjectObject in subjectObjects)
        {
            var sub = await connection.SubscribeCoreAsync<T>(subjectObject.Subject, subjectObject.QueueGroup,
                cancellationToken: stoppingToken);

            var reader = sub.Msgs;
            var task = Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in reader.ReadAllAsync(stoppingToken))
                    {
                        var order = msg.Data;

                        var response = await handler.Handle(subjectObject.Subject, subjectObject.QueueGroup, order,
                            stoppingToken);

                        await msg.ReplyAsync(response, cancellationToken: stoppingToken);
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