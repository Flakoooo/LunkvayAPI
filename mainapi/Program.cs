using LunkvayAPI.Auth.Models.Utils;
using LunkvayAPI.Auth.Services;
using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Chats.Controllers;
using LunkvayAPI.Chats.Services;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Filters;
using LunkvayAPI.Data;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Services;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            ConfigureMiddleware(app);
            await InitializeDatabaseAsync(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<LunkvayDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            );

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddCors(options => options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .SetIsOriginAllowed(_ => true)
                      .AllowCredentials())
            );

            //Auth
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            //Profiles
            services.AddScoped<IProfileService, ProfileService>();
            //Avatars
            services.AddScoped<IAvatarService, AvatarService>();
            //Friends
            services.AddScoped<IFriendsService, FriendsService>();
            //Chats
            services.AddScoped<IChatNotificationService, ChatNotificationService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IChatImageService, ChatImageService>();
            services.AddScoped<IChatMemberService, ChatMemberService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();

            services.AddSignalR();

            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowAll");
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LunkvayDBContext>();

            await db.Database.MigrateAsync();

            if (app.Environment.IsDevelopment())
            {
                try
                {
                    await SeedData.Initialize(db);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogCritical(ex, "FATAL: Ќе удалось инициализировать тестовые данные");
                    Environment.Exit(1);
                }
            }
        }
    }
}
