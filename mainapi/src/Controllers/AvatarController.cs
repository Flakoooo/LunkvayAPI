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
        public async Task<IActionResult> GetUserAvatarById(Guid userId)
        {
            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarById(userId);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Аватар для {UserId} отправлен", userId);
                return File(result.Result!, MediaTypeNames.Image.Jpeg);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
