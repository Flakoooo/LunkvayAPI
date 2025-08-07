using LunkvayAPI.src.Services;
using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src 
{
    public static class ServiceExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            _ = services.AddScoped<IUserService, UserService>();
            _ = services.AddScoped<IAuthService, AuthService>();
            _ = services.AddScoped<IProfileService, ProfileService>();
            _ = services.AddScoped<IAvatarService, AvatarService>();
            _ = services.AddScoped<IFriendsService, FriendsService>();
            _ = services.AddScoped<IChatService, ChatService>();
        }
    }
}