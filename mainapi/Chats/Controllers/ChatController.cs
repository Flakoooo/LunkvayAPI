using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Chats.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController(
        ILogger<ChatController> logger, 
        IChatNotificationService chatHubService,
        IChatService chatService,
        IChatMemberService chatMemberService,
        IChatMessageService chatMessageService
    ) : Controller
    {
        private readonly ILogger<ChatController> _logger = logger;
        private readonly IChatNotificationService _chatHubService = chatHubService;
        private readonly IChatService _chatService = chatService;
        private readonly IChatMemberService _chatMemberService = chatMemberService;
        private readonly IChatMessageService _chatMessageService = chatMessageService;

        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetRooms(Guid userId)
        {
            ServiceResult<List<ChatDTO>> result = await _chatService.GetRooms(userId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Запрос списка чатов для {UserId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpGet("messages/{userId}/{chatId}")]
        public async Task<IActionResult> GetMessages(
            Guid userId, Guid chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            // /api/v1/messages/{userId}/{chatId}
            // /api/v1/messages/{userId}/{chatId}?page=1
            // /api/v1/messages/{userId}/{chatId}?page=1&pageSize=10
            ServiceResult<IEnumerable<ChatMessageDTO>> result 
                = await _chatMessageService.GetChatMessages(userId, chatId, page, pageSize);
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

        [HttpPost("message/send/{senderId}")]
        public async Task<IActionResult> CreateMessage(
            [FromBody] ChatMessageRequest chatMessageRequest, Guid senderId
        )
        {
            var result = await _chatMessageService.CreateMessage(chatMessageRequest, senderId);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Создание сообщения пользователем {SenderId}", senderId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }


        /*
        //сделать получение Id из токена
        [HttpPost("{roomId}/invite/{senderId}/{newMemberId}")]
        public async Task<IActionResult> InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId)
        {
            await _chatHubService.InviteInRoom(roomId, senderId, newMemberId);

            return Ok();
        }

        [HttpPost("{roomId}/leave/{userId}")]
        public async Task<IActionResult> LeaveFromRoom(Guid roomId, Guid userId)
        {
            await _chatHubService.LeaveFromRoom(roomId, userId);

            return Ok();
        }

        [HttpPost("{roomId}/join/{userId}")]
        public async Task<IActionResult> JoinInRoom(Guid roomId, Guid userId)
        {
            await _chatHubService.JoinInRoom(roomId, userId);

            return Ok();
        }
        */
    }
}
