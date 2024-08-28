using System;
using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats;

public class MessagePackProvider<T>(MessagePackSerializerOptions opts) : INatsSerialize<T>, INatsDeserialize<T>
{
    public static MessagePackProvider<T> Default { get; } =
        new(MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(ContractlessStandardResolver.Instance));

    public T Deserialize(in ReadOnlySequence<byte> buffer)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(buffer, opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize. {ex}");
            throw;
        }
    }

    public void Serialize(IBufferWriter<byte> bufferWriter, T value)
    {
        try
        {
            MessagePackSerializer.Serialize(bufferWriter, value, opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to serialize. {ex}");
            throw;
        }
    }
}