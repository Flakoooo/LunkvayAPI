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
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _authService.Login(loginRequest);
            return Ok(token);
        }

        [HttpPost("logout")]
        public IActionResult Logout() => Ok();

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.Register(request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
