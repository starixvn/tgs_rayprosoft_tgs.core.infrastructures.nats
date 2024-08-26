using System.Threading.Tasks;

namespace TGS.Core.Infrastructures.Nats.Interface;

public interface ISubScribeHandler<in TResponse>
{
    Task ExecuteAsync(TResponse model);
}