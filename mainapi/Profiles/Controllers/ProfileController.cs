using LunkvayAPI.Common.Results;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Requests;
using LunkvayAPI.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Profiles.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class ProfileController(
        IProfileService profileService, 
        ILogger<ProfileController> logger
    ) : Controller
    {
        private readonly IProfileService _profileService = profileService;
        private readonly ILogger<ProfileController> _logger = logger;

        [HttpGet("current-user-profile")]
        public async Task<IActionResult> GetProfile()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос профиля пользователя {UserId}", userId);
            ServiceResult<ProfileDTO> result = await _profileService.GetUserProfileById(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешное получение профиля для пользователя {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return BadRequest(result.Error);
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfileByUserId(Guid userId)
        {
            _logger.LogInformation("Запрос профиля пользователя {UserId}", userId);
            ServiceResult<ProfileDTO> result = await _profileService.GetUserProfileById(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешное получение профиля для пользователя {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return BadRequest(result.Error);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Обновление профиля пользователя {UserId}", userId);
            ServiceResult<ProfileDTO> result = await _profileService.UpdateProfile(userId, request);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Успешное обновление профиля для пользователя {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return BadRequest(result.Error);
        }
    }
}
