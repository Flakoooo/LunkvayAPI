using Lunkvay.src.Services;
using Lunkvay.src.Services.Interfaces;

namespace Lunkvay.src 
{
    public static class ServiceExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IHelloService, HelloService>();
        }
    }
}