using LunkvayAPI.src;
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

            builder.Services.AddDbContext<LunkvayDBContext>(
                options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            //сервисы создаются до содания WebApplication
            builder.Services.AddCustomServices();

            //регистрация контроллеров
            builder.Services.AddControllers();

            WebApplication app = builder.Build();

            //Настройка маршрутов контроллеров
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LunkvayDBContext>();

                // Применяем миграции
                db.Database.Migrate();

                // Инициализируем начальные данные
                if (app.Environment.IsDevelopment())
                {
                    SeedData.Initialize(db);
                }
            }

            app.Run();
        }
    }
}
