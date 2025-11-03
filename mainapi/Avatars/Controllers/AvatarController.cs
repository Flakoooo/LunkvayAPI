using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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

        private const string DEFAULT_MEDIA_TYPE = MediaTypeNames.Image.Webp;

        [HttpGet]
        public async Task<ActionResult> GetUserAvatar()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarByUserId(userId);

            if (!result.IsSuccess || result.Result is null)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар для {UserId} отправлен", userId);
            return File(result.Result, DEFAULT_MEDIA_TYPE);
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetUserAvatarByUserId(Guid userId)
        {
            _logger.LogInformation("Запрос аватара для пользователя {UserId}", userId);

            ServiceResult<byte[]> result = await _avatarService.GetUserAvatarByUserId(userId);

            if (!result.IsSuccess || result.Result is null)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар для {UserId} отправлен", userId);
            return File(result.Result, DEFAULT_MEDIA_TYPE);
        }

        [HttpPost]
        public async Task<ActionResult> UploadAvatar(IFormFile avatarFile)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest("Файл не предоставлен");

            if (avatarFile.Length > 5 * 1024 * 1024)
                return BadRequest("Файл слишком большой. Максимальный размер: 5MB");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Недопустимый формат файла. Разрешены: JPG, PNG, GIF, BMP");

            try
            {
                // чтение и конвертация файла
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await avatarFile.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var result = await _avatarService.SetUserAvatar(userId, fileData);

                if (!result.IsSuccess || result.Result is null)
                {
                    _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                    return StatusCode((int)result.StatusCode, result.Error);
                }

                return File(result.Result, DEFAULT_MEDIA_TYPE);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)HttpStatusCode.InternalServerError, ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteUserAvatar()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос удаления аватара для пользователя {UserId}", userId);

            ServiceResult<bool> result = await _avatarService.RemoveUserAvatar(userId);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Аватар пользователя {UserId} успешно удален", userId);
            return Ok();
        }
    }
}
