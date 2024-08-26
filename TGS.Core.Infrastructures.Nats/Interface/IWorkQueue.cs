using System.Collections.Generic;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface IWorkQueue<T>
{
    string StreamName { get; set; }
    IEnumerable<string> Subjects { get; set; }
}