using LunkvayAPI.Avatars.Models.Enums;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

namespace LunkvayAPI.Chats.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/chat-image")]
    public class ChatImageController(
        ILogger<ChatImageController> logger,
        IChatImageService chatImageService
    ) : Controller
    {
        private readonly ILogger<ChatImageController> _logger = logger;
        private readonly IChatImageService _chatImageService = chatImageService;

        [HttpGet("{chatId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChatImageById(Guid chatId)
        {
            _logger.LogInformation("Запрос изображения чата для пользователя {ChatId}", chatId);

            ServiceResult<string> result = await _chatImageService.GetChatImgDBImage(chatId);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Изображение чата для {ChatId} отправлено", chatId);
                return File(result.Result!, MediaTypeNames.Image.Jpeg);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("{chatId}")]
        public async Task<ActionResult<string>> SetChatImage(Guid chatId, IFormFile avatarFile)
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

                var result = await _chatImageService.UploadChatImgDBImage(userId, chatId, fileData);

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

        [HttpDelete("{chatId}")]
        public async Task<ActionResult<string>> DeleteChatImage(Guid chatId)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос на удаление аватара для пользователя {UserId}", userId);

            ServiceResult<string> result = await _chatImageService.DeleteChatImgDBImage(userId, chatId);

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
