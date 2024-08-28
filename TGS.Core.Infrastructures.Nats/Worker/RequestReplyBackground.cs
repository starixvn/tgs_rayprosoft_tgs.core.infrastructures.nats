using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class RequestReplyBackground<T, TResponse>(
    INatsConnection connection,
    IMessageReplyHandler<T?, TResponse> handler,
    IEnumerable<ISubjectObject<T, TResponse>> subjectObjects,
    ILogger<RequestReplyBackground<T, TResponse>> logger)
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
                await foreach (var msg in reader.ReadAllAsync(stoppingToken))
                    try
                    {
                        var response = await handler.Handle(subjectObject.Subject, subjectObject.QueueGroup,
                            msg.Headers, msg.Data, stoppingToken);

                        await msg.ReplyAsync(response, cancellationToken: stoppingToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("RequestReply Error: {e}", e);
                    }

                await sub.DisposeAsync();
                logger.LogWarning("Unsubscribed");
            }, stoppingToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}