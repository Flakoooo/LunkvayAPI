using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvatarController(ILogger<AvatarController> logger, IAvatarService avatarService) : Controller
    {
        private readonly ILogger<AvatarController> _logger = logger;
        private readonly IAvatarService _avatarService = avatarService;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAvatarById(string userId)
        {
            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            if (!Guid.TryParse(userId, out var guid))
            {
                _logger.LogWarning("Некорректный Id пользователя: {UserId}", userId);
                return BadRequest("Некорректный идентификатор пользователя");
            }

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarById(guid);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Аватар для {UserId} отправлен", userId);
                return File(result.Result!, MediaTypeNames.Image.Jpeg);
            }

            _logger.LogError("Ошибка: {Error} (Status: {StatusCode})", result.Error, result.StatusCode);
            return StatusCode(result.StatusCode, result.Error);
        }
    }
}
