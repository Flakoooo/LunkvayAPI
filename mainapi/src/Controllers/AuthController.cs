using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = LunkvayAPI.src.Models.Requests.LoginRequest;
using RegisterRequest = LunkvayAPI.src.Models.Requests.RegisterRequest;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : Controller
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation("Вход пользователя {Email}", loginRequest.Email);
            ServiceResult<string> result = await _authService.Login(loginRequest);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешный вход {Email}", loginRequest.Email);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Регистрация пользователя {Login}", request.UserName);
            ServiceResult<User> result = await _authService.Register(request);

            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Успешная регистрация пользователя {Login}", result.Result.UserName);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
