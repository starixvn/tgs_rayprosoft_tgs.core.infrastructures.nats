using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats;

public class SubjectObject<T> : ISubjectObject<T> where T : class
{
    public required string Subject { get; set; }
    public string? QueueGroup { get; set; }
    public INatsDeserialize<T>? Serializer { get; set; } = default;
}

public class SubjectObject<T, TResponse> : ISubjectObject<T, TResponse> where T : class where TResponse : class
{
    public required string Subject { get; set; }
    public string? QueueGroup { get; set; }
    public INatsDeserialize<T>? Serializer { get; set; } = default;
}