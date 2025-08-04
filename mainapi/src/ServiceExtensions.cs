using LunkvayAPI.src.Services;
using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src 
{
    public static class ServiceExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAvatarService, AvatarService>();
            services.AddScoped<IFriendsService, FriendsService>();
        }
    }
}