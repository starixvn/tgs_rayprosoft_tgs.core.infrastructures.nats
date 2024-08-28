using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats;

public class MessagePackRegistry : INatsSerializerRegistry
{
    public static readonly MessagePackRegistry Default = new();

    public INatsSerialize<T> GetSerializer<T>()
    {
        return MessagePackProvider<T>.Default;
    }

    public INatsDeserialize<T> GetDeserializer<T>()
    {
        return MessagePackProvider<T>.Default;
    }
}