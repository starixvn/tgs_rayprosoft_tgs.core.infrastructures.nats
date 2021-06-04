using MessagePack;

namespace TGS.Core.Infrastructures.Nats.Model
{
    [MessagePackObject(true)]
    public class ObjectFileEventModel
    {
        public string Id { get; set; }
        public string[] FileNames { get; set; }
    }
}