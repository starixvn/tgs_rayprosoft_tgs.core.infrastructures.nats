using System;
using System.Collections.Generic;
using TGS.Core.Infrastructures.Nats.Interface;

namespace TGS.Core.Infrastructures.Nats;

public class WorkQueue<T> : IWorkQueue<T> where T : class
{
    public required IEnumerable<string> Subjects { get; set; }
    public required string Stream { get; set; }
    public TimeSpan? NakDelayTime { get; set; }
}