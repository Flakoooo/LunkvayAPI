using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.ChatAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace LunkvayAPI.src.Controllers.ChatAPI
{
    [ApiController]
    [Route("api/v1/chat-image")]
    public class ChatImageController(
        ILogger<ChatImageController> logger,
        IChatImageService chatImageService
    ) : Controller
    {
        private readonly ILogger<ChatImageController> _logger = logger;
        private readonly IChatImageService _chatImageService = chatImageService;

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChatImageById(Guid chatId)
        {
            _logger.LogInformation("Запрос изображения чата для пользователя {ChatId}", chatId);

            ServiceResult<byte[]> result = await _chatImageService.GetChatImagesById(chatId);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Изображение чата для {ChatId} отправлено", chatId);
                return File(result.Result!, MediaTypeNames.Image.Jpeg);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("{chatId}")]
        public async Task<IActionResult> SetChatImage(Guid chatId, [FromBody] byte[] image)
        {
            return Ok();
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChatImage(Guid chatId)
        {
            return Ok();
        }
    }
}
