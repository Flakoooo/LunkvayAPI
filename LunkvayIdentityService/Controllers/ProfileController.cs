using LunkvayIdentityService.Models.DTO;
using LunkvayIdentityService.Models.Utils;
using LunkvayIdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayIdentityService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProfileController(IProfileService profileService, ILogger<ProfileController> logger) : Controller
    {
        private readonly IProfileService _profileService = profileService;
        private readonly ILogger<ProfileController> _logger = logger;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfileByUserId(Guid userId)
        {
            _logger.LogInformation("Запрос профиля пользователя {UserId}", userId);
            ServiceResult<UserProfileDTO> result = await _profileService.GetUserProfileById(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешное получение профиля для пользователя {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return BadRequest(result.Error);
        }
    }
}
