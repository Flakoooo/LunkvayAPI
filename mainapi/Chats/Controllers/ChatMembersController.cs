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
    [Route("api/v1/chats/members")]
    public class ChatMembersController(
        ILogger<ChatMembersController> logger,
        IChatMemberService chatMemberService
    ) : Controller
    {
        private readonly ILogger<ChatMembersController> _logger = logger;
        private readonly IChatMemberService _chatMemberService = chatMemberService;

        [HttpGet("{chatId}")]
        public async Task<ActionResult<IReadOnlyList<ChatMemberDTO>>> GetMembers(Guid chatId)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            ServiceResult<List<ChatMemberDTO>> result = await _chatMemberService.GetChatMembers(chatId);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Получение списка участников для чата {ChatId} от пользователя {UserId}", chatId, userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPost]
        public async Task<ActionResult<ChatMemberDTO>> CreateMember(
            [FromBody] CreateChatMemberRequest request
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMemberService.CreateMember(userId, request);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Создание участника пользователем {SenderId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpPatch]
        public async Task<ActionResult<ChatMemberDTO>> CreateMember(
            [FromBody] UpdateChatMemberRequest request
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMemberService.UpdateMember(userId, request);
            if (result.IsSuccess && result.Result is not null)
            {
                _logger.LogDebug("Обновление участника пользователем {SenderId}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpDelete]
        public async Task<ActionResult<ChatMemberDTO>> DeleteMember(
            [FromBody] DeleteChatMemberRequest request
        )
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            var result = await _chatMemberService.DeleteMember(userId, request);
            if (result.IsSuccess)
            {
                _logger.LogDebug("Обновление участника пользователем {SenderId}", userId);
                return Ok();
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
