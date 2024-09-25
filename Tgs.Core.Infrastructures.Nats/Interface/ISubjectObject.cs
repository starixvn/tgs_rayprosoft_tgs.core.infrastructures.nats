using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface ISubjectObject<T>
{
    string Subject { get; set; }
    string? QueueGroup { get; set; }
    INatsDeserialize<T>? RequestSerializer { get; set; }
}

public interface ISubjectObject<T, TResponse>
{
    string Subject { get; set; }
    string? QueueGroup { get; set; }
    INatsSerialize<TResponse>? ReplySerializer { get; set; }
    INatsDeserialize<T>? RequestSerializer { get; set; }
}