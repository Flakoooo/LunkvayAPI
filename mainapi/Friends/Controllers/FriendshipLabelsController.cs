using LunkvayAPI.Common.Results;
using LunkvayAPI.Friends.Models.DTO;
using LunkvayAPI.Friends.Models.Requests;
using LunkvayAPI.Friends.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Friends.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/friends/labels")]
    public class FriendshipLabelsController(
        IFriendshipLabelsService friendshipLabelsService,
        ILogger<FriendshipLabelsController> logger
    ) : Controller
    {
        private readonly IFriendshipLabelsService _friendshipLabelsService = friendshipLabelsService;
        private readonly ILogger<FriendshipLabelsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FriendshipLabelDTO>>> GetLabels()
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос списка дружеских меток друзей {UserId}", userId);

            ServiceResult<List<FriendshipLabelDTO>> result
                = await _friendshipLabelsService.GetLabels(userId);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Вывод списка дружеских меток друзей {UserId}", userId);
            return Ok(result.Result);
        }

        [HttpPost]
        public async Task<ActionResult<FriendshipLabelDTO>> CreateLabel(
            [FromBody] CreateFriendshipLabelRequest request        
        )
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation("Запрос создания дружеской метки пользователем {UserId}", userId);

            ServiceResult<FriendshipLabelDTO> result
                = await _friendshipLabelsService.CreateLabel(userId, request);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Успешное создание дружеской метки пользователем {UserId}", userId);
            return Ok(result.Result);
        }

        [HttpDelete("{friendshipLabelId}")]
        public async Task<ActionResult> DeleteLabel(Guid friendshipLabelId)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Запрос удаления дружеской метки {LabelId} пользователем {UserId}", 
                friendshipLabelId, userId
            );

            ServiceResult<bool> result
                = await _friendshipLabelsService.DeleteLabel(userId, friendshipLabelId);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug(
                "Успешное удаление дружеской метки {LabelId} пользователем {UserId}",
                friendshipLabelId, userId
            );
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteSpecificLabel([FromQuery] string label)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Запрос удаления дружеских меток '{LabelName}' пользователем {UserId}",
                label, userId
            );

            ServiceResult<int> result
                = await _friendshipLabelsService.DeleteSpecificLabel(userId, label);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug(
                "Успешное удаление дружеских меток '{LabelName}' пользователем {UserId}",
                label, userId
            );
            return Ok();
        }
    }
}
