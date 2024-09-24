using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using NATS.Client.Hosting;
using NATS.Client.JetStream;
using NATS.Client.Serializers.Json;
using TGS.Core.Infrastructures.Nats.Interface;
using TGS.Core.Infrastructures.Nats.Worker;

namespace TGS.Core.Infrastructures.Nats;

public static class ServiceExtensions
{
    public static IServiceCollection AddNatsMessage(this IServiceCollection services, string url,
        INatsSerializerRegistry? natsSerializerRegistry = null)
    {
        return services.AddNats(3, _ => NatsOpts.Default with
            {
                Url = url,
                SerializerRegistry = natsSerializerRegistry ?? NatsJsonSerializerRegistry.Default
            })
            .AddTransient<INatsJSContext>(provider =>
            {
                var connection = provider.GetRequiredService<NatsConnection>();
                return new NatsJSContext(connection);
            })
            .AddTransient<IMessageManager, MessageManager>();
    }

    public static IServiceCollection AddFakeNatsMessage(this IServiceCollection services)
    {
        foreach (var desc in services.Where(
                     desc => desc.ServiceType == typeof(IHostedService)
                             && (desc.ImplementationType?.Name == typeof(WorkQueueBackground<>).Name ||
                                 desc.ImplementationType?.Name == typeof(SubscribeBackground<>).Name ||
                                 desc.ImplementationType?.Name == typeof(RequestReplyBackground<,>).Name)).ToArray())
        {
            _ = services.Remove(desc);
        }

        return services.AddTransient<IMessageManager, FakeMessageManager>();
    }

    public static IServiceCollection SubscribeRegister<T>(this IServiceCollection services, Type messageHandler,
        string subject, string? queueGroup = null, INatsDeserialize<T>? serializer = default) where T : class
    {
        var subjectObject = new SubjectObject<T>
        {
            Subject = subject,
            QueueGroup = queueGroup,
            Serializer = serializer
        };

        if (services.FirstOrDefault(d => d.ServiceType == typeof(IEnumerable<ISubjectObject<T>>))
                ?.ImplementationInstance is not List<ISubjectObject<T>> existingList)
        {
            var newList = new List<ISubjectObject<T>> { subjectObject };
            services.AddSingleton<IEnumerable<ISubjectObject<T>>>(newList);
        }
        else
        {
            existingList.Add(subjectObject);
        }

        services.AddTransient(typeof(IMessageHandler<T>), messageHandler);

        services.AddHostedService<SubscribeBackground<T>>();

        return services;
    }

    public static IServiceCollection RequestReplyRegister<T, TResponse>(this IServiceCollection services,
        Type messageHandler,
        string subject, string? queueGroup,
        INatsDeserialize<T>? serializer = default) where T : class
    {
        var subjectObject = new SubjectObject<T, TResponse>
        {
            Subject = subject,
            QueueGroup = queueGroup,
            Serializer = serializer
        };

        if (services.FirstOrDefault(d => d.ServiceType == typeof(IEnumerable<ISubjectObject<T, TResponse>>))
                ?.ImplementationInstance is not List<ISubjectObject<T, TResponse>> existingList)
        {
            var newList = new List<ISubjectObject<T, TResponse>> { subjectObject };
            services.AddSingleton<IEnumerable<ISubjectObject<T, TResponse>>>(newList);
        }
        else
        {
            existingList.Add(subjectObject);
        }

        services.AddTransient(typeof(IMessageReplyHandler<T, TResponse>), messageHandler);


        services.AddHostedService<RequestReplyBackground<T, TResponse>>();
        return services;
    }

    public static IServiceCollection WorkQueueRegister<T>(this IServiceCollection services, Type messageHandler,
        string streamName, IEnumerable<string> subjects, TimeSpan? nakDelayTime = null,
        INatsDeserialize<T>? serializer = default) where T : class
    {
        var workQueue = new WorkQueue<T>
        {
            Stream = streamName,
            Subjects = subjects,
            NakDelayTime = nakDelayTime,
            Serializer = serializer
        };

        if (services.FirstOrDefault(d => d.ServiceType == typeof(IEnumerable<IWorkQueue<T>>))
                ?.ImplementationInstance is not List<IWorkQueue<T>> existingList)
        {
            var newList = new List<IWorkQueue<T>> { workQueue };
            services.AddSingleton<IEnumerable<IWorkQueue<T>>>(newList);
        }
        else
        {
            existingList.Add(workQueue);
        }

        services.AddTransient(typeof(IWorkQueueHandler<T>), messageHandler);

        services.AddHostedService<WorkQueueBackground<T>>();

        return services;
    }
}