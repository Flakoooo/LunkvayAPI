
using LunkvayIdentityService.Models.Utils;
using LunkvayIdentityService.Services.Classes;
using LunkvayIdentityService.Services.Interfaces;
using LunkvayIdentityService.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayIdentityService
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

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAvatarService, AvatarService>();

            services.AddSignalR();

            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowAll");
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
                    logger.LogCritical(ex, "FATAL: �� ������� ���������������� �������� ������");
                    Environment.Exit(1);
                }
            }
        }
    }
}
