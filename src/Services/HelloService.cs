using Lunkvay.src.Services.Interfaces;

namespace Lunkvay.src.Services 
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