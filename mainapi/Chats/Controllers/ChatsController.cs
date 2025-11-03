using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Chats.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class ChatsController(
        ILogger<ChatsController> logger,
        IChatService chatService
    ) : Controller
    {
        private readonly ILogger<ChatsController> _logger = logger;
        private readonly IChatService _chatService = chatService;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ChatDTO>>> GetRooms()
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<List<ChatDTO>> result = await _chatService.GetRooms(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Запрос списка чатов для {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("group")]
        public async Task<ActionResult<ChatDTO>> CreateGroupRoom([FromBody] CreateGroupChatRequest chatRequest)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<ChatDTO> result = await _chatService.CreateGroupChat(userId, chatRequest);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Создание группового чата пользователем {CreatorId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPatch("{chatId}")]
        public async Task<ActionResult<ChatDTO>> UpdateChat(Guid chatId, [FromBody] UpdateChatRequest chatRequest)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<ChatDTO> result = await _chatService.UpdateChat(userId, chatId, chatRequest);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Обновление чата пользователем {CreatorId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpDelete("{chatId}")]
        public async Task<ActionResult> DeleteChat(Guid chatId)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<bool> result = await _chatService.DeleteChat(userId, chatId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Удаление чата пользователем {CreatorId}", userId);
                return Ok();
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
