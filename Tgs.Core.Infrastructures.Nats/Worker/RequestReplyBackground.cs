using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats.Worker;

public class RequestReplyBackground<T, TResponse>(
    INatsConnection connection,
    IServiceProvider serviceProvider,
    IEnumerable<ISubjectObject<T, TResponse>> subjectObjects,
    ILogger<RequestReplyBackground<T, TResponse>> logger)
    : BackgroundService where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var subjectObject in subjectObjects)
        {
            var sub = await connection.SubscribeCoreAsync(subjectObject.Subject, subjectObject.QueueGroup,
                subjectObject.RequestSerializer,
                cancellationToken: stoppingToken);

            var reader = sub.Msgs;
            var task = Task.Run(async () =>
            {
                await foreach (var msg in reader.ReadAllAsync(stoppingToken))
                    try
                    {
                        using var scope = serviceProvider.CreateScope();

                        var messageHandler =
                            (IMessageReplyHandler<T, TResponse>)scope.ServiceProvider.GetRequiredService(
                                typeof(IMessageReplyHandler<T, TResponse>));

                        var response = await messageHandler.Handle(subjectObject.Subject, subjectObject.QueueGroup,
                            msg.Headers, msg.Data, stoppingToken);

                        await msg.ReplyAsync(response, msg.Headers, serializer: subjectObject.ReplySerializer, cancellationToken: stoppingToken);
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