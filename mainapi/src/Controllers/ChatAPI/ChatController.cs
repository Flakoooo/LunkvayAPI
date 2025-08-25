using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.ChatAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers.ChatAPI
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController(ILogger<ChatController> logger, IChatService chatService) : Controller
    {
        private readonly ILogger<ChatController> _logger = logger;
        private readonly IChatService _chatService = chatService;

        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetRooms(Guid userId)
        {
            ServiceResult<IEnumerable<ChatDTO>> result = await _chatService.GetRooms(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Запрос списка чатов для {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpGet("messages/{userId}/{chatId}")]
        public async Task<IActionResult> GetMessages(Guid userId, Guid chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // /api/v1/messages/{userId}/{chatId}
            // /api/v1/messages/{userId}/{chatId}?page=1
            // /api/v1/messages/{userId}/{chatId}?page=1&pageSize=10
            ServiceResult<IEnumerable<ChatMessageDTO>> result = await _chatService.GetChatMessages(userId, chatId, page, pageSize);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Получение списка сообщение для чата {ChatId} от пользователя {UserId}", chatId, userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("create")]
        // /api/v1/chat/create?creatorId=id
        public async Task<IActionResult> CreateRoom([FromBody] ChatRequest chatRequest, [FromQuery] Guid creatorId)
        {
            ServiceResult<ChatDTO> result = await _chatService.CreateRoom(chatRequest, creatorId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Создание чата пользователем {CreatorId}", creatorId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost("{roomId}/join/{userId}")]
        public async Task<IActionResult> JoinInRoom(Guid roomId, Guid userId)
        {
            await _chatService.JoinInRoom(roomId, userId);

            return Ok();
        }

        //сделать получение Id из токена
        [HttpPost("{roomId}/invite/{senderId}/{newMemberId}")]
        public async Task<IActionResult> InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId)
        {
            await _chatService.InviteInRoom(roomId, senderId, newMemberId);

            return Ok();
        }

        [HttpPost("{roomId}/leave/{userId}")]
        public async Task<IActionResult> LeaveFromRoom(Guid roomId, Guid userId)
        {
            await _chatService.LeaveFromRoom(roomId, userId);

            return Ok();
        }
    }
}
