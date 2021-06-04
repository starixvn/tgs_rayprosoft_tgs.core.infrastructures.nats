using System;
using MessagePack;

namespace TGS.Core.Infrastructures.Nats
{
    public interface IMessagePackProvider
    {
        T Deserialize<T>(byte[] arrayToDeserialize);

        byte[] Serialize<T>(T objectToSerialize);
    }

    public class MessagePackProvider : IMessagePackProvider
    {
        private readonly MessagePackSerializerOptions _messagePackSerializerOptions =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        public MessagePackProvider()
        {

        }

        T IMessagePackProvider.Deserialize<T>(byte[] arrayToDeserialize)
        {
            try
            {
                return MessagePackSerializer.Deserialize<T>(arrayToDeserialize, _messagePackSerializerOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize. {ex}");
                throw;
            }
        }

        byte[] IMessagePackProvider.Serialize<T>(T objectToSerialize)
        {
            try
            {
                return MessagePackSerializer.Serialize(objectToSerialize, _messagePackSerializerOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to serialize. {ex}");
                throw;
            }
        }
    }
}