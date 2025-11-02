using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Chats.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/chats/messages")]
    public class ChatMessagesController(
        ILogger<ChatMessagesController> logger,
        IChatMessageService chatMessageService
    ) : Controller
    {
        private readonly ILogger<ChatMessagesController> _logger = logger;
        private readonly IChatMessageService _chatMessageService = chatMessageService;

        [HttpGet("{chatId}")]
        public async Task<ActionResult<IReadOnlyList<ChatMessageDTO>>> GetMessages(
            Guid chatId, [FromQuery] bool pinned, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            // /api/v1/messages/{chatId}
            // /api/v1/messages/{chatId}?page=1
            // /api/v1/messages/{chatId}?page=1&pageSize=10
            ServiceResult<List<ChatMessageDTO>> result = pinned
                ? await _chatMessageService.GetPinnedChatMessages(userId, chatId, page, pageSize)
                : await _chatMessageService.GetChatMessages(userId, chatId, page, pageSize);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Получение списка сообщение для чата {ChatId} от пользователя {UserId}", chatId, userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost]
        public async Task<ActionResult<ChatMessageDTO>> CreateMessage(
            [FromBody] CreateChatMessageRequest chatMessageRequest
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMessageService.CreateMessage(userId, chatMessageRequest);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Создание сообщения пользователем {SenderId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPatch("edit")]
        public async Task<ActionResult<ChatMessageDTO>> EditMessage(
            [FromBody] UpdateEditChatMessageRequest chatMessageRequest
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMessageService.EditChatMessage(userId, chatMessageRequest);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Обновление сообщения пользователем {SenderId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }


        [HttpPatch("pin")]
        public async Task<ActionResult<ChatMessageDTO>> PinMessage(
            [FromBody] UpdatePinChatMessageRequest chatMessageRequest
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMessageService.PinChatMessage(userId, chatMessageRequest);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Обновление сообщения пользователем {SenderId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteChat(
            [FromBody] DeleteChatMessageRequest chatMessageRequest
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<bool> result = await _chatMessageService.DeleteMessage(userId, chatMessageRequest);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Удаление сообщения пользователем {CreatorId}", userId);
                return Ok();
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
