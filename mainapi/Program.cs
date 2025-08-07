using LunkvayAPI.src;
using LunkvayAPI.src.Controllers;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            _ = builder.Services.AddDbContext<LunkvayDBContext>(options =>
            {
                _ = options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            _ = builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddCustomServices();

            _ = builder.Services.AddSignalR();
            _ = builder.Services.AddControllers(options =>
            {
                _ = options.Filters.Add<GlobalExceptionFilter>();
            });

            WebApplication app = builder.Build();

            _ = app.MapHub<ChatHub>("/chatHub");
            _ = app.MapControllers();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                LunkvayDBContext db = scope.ServiceProvider.GetRequiredService<LunkvayDBContext>();

                db.Database.Migrate();

                if (app.Environment.IsDevelopment())
                {
                    SeedData.Initialize(db);
                }
            }

            app.Run();
        }
    }
}
