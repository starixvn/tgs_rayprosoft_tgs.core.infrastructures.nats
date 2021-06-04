using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGS.Core.Infrastructures.Nats
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddNats(this IServiceCollection services, IConfiguration configuration, string configSectionName = "NatsSetting")
        {
            //services.Configure<NatsSetting>(configuration.GetSection(configSectionName));
            services.AddSingleton<IMessagePackProvider, MessagePackProvider>();
            services.AddSingleton<INatsManager, NatsManager>();
            return services;
        }
    }
}