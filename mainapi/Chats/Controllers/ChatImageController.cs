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

        private const string DEFAULT_MEDIA_TYPE = MediaTypeNames.Image.Webp;


        [HttpGet("{userId}/{chatId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetChatImageByUserId(Guid userId, Guid chatId)
        {
            _logger.LogInformation("Запрос изображения чата {ChatId}", chatId);

            ServiceResult<byte[]> result = await _chatImageService.GetChatImageByChatId(userId, chatId);

            if (!result.IsSuccess || result.Result is null)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Изображение чата {ChatId} отправлен", chatId);
            return File(result.Result, DEFAULT_MEDIA_TYPE);
        }

        [HttpPost("{chatId}")]
        public async Task<ActionResult> UploadChatImage(Guid chatId, IFormFile avatarFile)
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

                var result = await _chatImageService.SetChatImage(userId, chatId, fileData);

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

        [HttpDelete("{chatId}")]
        public async Task<ActionResult> DeleteChatImage(Guid chatId)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос удаления изображения чата {ChatId}", chatId);

            ServiceResult<bool> result = await _chatImageService.RemoveChatImage(userId, chatId);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Изображение чата {ChatId} успешно удалено", chatId);
            return Ok();
        }
    }
}
