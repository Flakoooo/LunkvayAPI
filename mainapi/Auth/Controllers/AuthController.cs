using LunkvayAPI.Auth.Services;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = LunkvayAPI.Auth.Models.Requests.LoginRequest;
using RegisterRequest = LunkvayAPI.Auth.Models.Requests.RegisterRequest;

namespace LunkvayAPI.Auth.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : Controller
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest loginRequest)
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
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Регистрация пользователя {Login}", request.UserName);
            ServiceResult<User> result = await _authService.Register(request);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешная регистрация пользователя {Login}", result.Result?.UserName);
                return Ok();
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
