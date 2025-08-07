using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(IUserService userService, ILogger<UserController> logger) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<UserController> _logger = logger;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            _logger.LogInformation("Запрос пользователя {UserId}", userId);
            ServiceResult<Models.DTO.UserDTO> result = await _userService.GetUserById(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Пользователь {UserId} успешно отправлен", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        //сделать постраничную загрузку
        [HttpGet("all")]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Запрос списка пользователей");
            ServiceResult<IEnumerable<Models.DTO.UserDTO>> result = await _userService.GetUsers();

            if (result.IsSuccess)
            {
                _logger.LogDebug("Список пользователей отправлен");
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
