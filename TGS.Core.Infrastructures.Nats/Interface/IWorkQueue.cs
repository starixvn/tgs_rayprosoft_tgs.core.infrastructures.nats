using System;
using System.Collections.Generic;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IWorkQueue<T>
{
    string Stream { get; set; }
    IEnumerable<string> Subjects { get; set; }
    TimeSpan? NakDelayTime { get; set; }
}