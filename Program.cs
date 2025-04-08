using LunkvayAPI.src;

namespace LunkvayAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
