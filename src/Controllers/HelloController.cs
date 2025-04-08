using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    //[Route("api/[controller]")] //controller вернет название контроллера, тоесть hello
    [Route("api")]
    public class HelloController(IHelloService helloService) : ControllerBase
    {

        private readonly IHelloService _helloService = helloService;

        [HttpGet]
        public IActionResult GetTest() => Ok("тесто");

        [HttpGet("hello")]
        public async Task<IActionResult> GetHello()
        {
            var result = await _helloService.SayHello();
            return Ok(result);
        }
    }
}