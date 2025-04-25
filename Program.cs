using LunkvayAPI.src;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            //сервисы создаются до содания WebApplication
            builder.Services.AddCustomServices();

            //регистрация контроллеров
            builder.Services.AddControllers();

            WebApplication app = builder.Build();

            //Настройка маршрутов контроллеров
            app.MapControllers();
            
            app.Run();
        }
    }
}
