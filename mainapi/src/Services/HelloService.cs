using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src.Services 
{
    public class HelloService : IHelloService
    {

        public async Task<string> SayHello()
        {
            await Task.Delay(1000);
            return "Здарова";
        }
    }
}