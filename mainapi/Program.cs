using LunkvayAPI.src;
using LunkvayAPI.src.Controllers;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<LunkvayDBContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddCustomServices();

            builder.Services.AddSignalR();
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            WebApplication app = builder.Build();

            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();

            using (IServiceScope scope = app.Services.CreateScope())
            {
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

            app.Run();
        }
    }
}
