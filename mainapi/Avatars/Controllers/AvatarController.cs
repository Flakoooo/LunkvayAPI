using LunkvayAPI.Avatars.Models.Enums;
using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        public async Task<ActionResult<string>> GetCurrentUserImgDBAvatar()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<string> result = await _avatarService.GetUserImgDBAvatar(userId);
            if (!result.IsSuccess || result.Result is null)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар для {UserId} отправлен", userId);
            return Ok(result.Result);
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetUserImgDBAvatar(Guid userId)
        {
            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<string> result = await _avatarService.GetUserImgDBAvatar(userId);
            if (!result.IsSuccess || result.Result is null)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар для {UserId} отправлен", userId);
            return Ok(result.Result);
        }

        [HttpPost]
        public async Task<ActionResult<string>> UploadUserImgDBAvatar(IFormFile avatarFile)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest(AvatarsErrorCode.FileIsNull.GetDescription());

            if (avatarFile.Length > 5 * 1024 * 1024)
                return BadRequest(AvatarsErrorCode.FileLengthLimit.GetDescription());

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(AvatarsErrorCode.FileFormatInvalid.GetDescription());

            try
            {
                // чтение и конвертация файла
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await avatarFile.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var result = await _avatarService.UploadUserImgDBAvatar(userId, fileData);

                if (!result.IsSuccess || result.Result is null)
                {
                    _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                    return StatusCode((int)result.StatusCode, result.Error);
                }

                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)HttpStatusCode.InternalServerError, ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<ActionResult<string>> DeleteUserImgDBAvatar()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос на удаление аватара для пользователя {UserId}", userId);

            ServiceResult<string> result = await _avatarService.DeleteUserImgDBAvatar(userId);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка удаления аватара: {Error}", result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар удален для {UserId}", userId);
            return Ok(result.Result);
        }
    }
}
