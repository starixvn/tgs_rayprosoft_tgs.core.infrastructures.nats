using System;
using System.Collections.Generic;
using NATS.Client.Core;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IWorkQueue<T>
{
    string Stream { get; set; }
    IEnumerable<string> Subjects { get; set; }
    INatsDeserialize<T>? Serializer { get; set; }
    TimeSpan? NakDelayTime { get; set; }
}