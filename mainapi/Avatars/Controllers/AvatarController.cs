using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace LunkvayAPI.Avatars.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class AvatarController(
        ILogger<AvatarController> logger, 
        IAvatarService avatarService
    ) : Controller
    {
        private readonly ILogger<AvatarController> _logger = logger;
        private readonly IAvatarService _avatarService = avatarService;

        [HttpGet]
        public async Task<IActionResult> GetUserAvatar()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarByUserId(userId);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Аватар для {UserId} отправлен", userId);
                return File(result.Result!, MediaTypeNames.Image.Jpeg);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAvatarByUserId(Guid userId)
        {
            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarByUserId(userId);

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
