using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController(IAuthService authService) : Controller
    {
        private readonly IAuthService _authService = authService;
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest) => Ok(_authService.Login(loginRequest));

        [HttpPost("logout")]
        public IActionResult Logout() => Ok();

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authService.Register(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
