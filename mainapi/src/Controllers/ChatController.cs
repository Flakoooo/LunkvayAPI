using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayApp.src.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController(ILogger<ChatController> logger, IChatService chatService) : Controller
    {
        private readonly ILogger<ChatController> _logger = logger;
        private readonly IChatService _chatService = chatService;

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] ChatRequest chatRequest)
        {
            //перенести данную реализацию в сервис
            //проверка на существующий личный чат
            Chat chat = new()
            {
                Name = chatRequest.Name,
                Type = chatRequest.Type
            };

            return Ok();
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
