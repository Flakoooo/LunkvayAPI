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
    [Route("api/v1/friends")]
    public class FriendshipsController(
        IFriendshipsService friendshipsService, ILogger<FriendshipsController> logger
    ) : Controller
    {
        private readonly IFriendshipsService _friendshipsService = friendshipsService;
        private readonly ILogger<FriendshipsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FriendshipDTO>>> GetCurrentUserFriends(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1&pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);

            ServiceResult<List<FriendshipDTO>> result
                = await _friendshipsService.GetFriends(userId, page, pageSize, true);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Вывод друзей пользователя {Id}, страница {Page}", userId, page);
            return Ok(result.Result);
        }

        //TODO: Реализовать вывод запросов в друзья
        //TODO: Реализовать вывод исходящих запросов в друзья

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<ActionResult<IReadOnlyList<FriendshipDTO>>> GetUserFriends(
            Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1&pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);

            ServiceResult<List<FriendshipDTO>> result
                = await _friendshipsService.GetFriends(userId, page, pageSize);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Запрос друзей пользователя {Id}, страница {Page}", userId, page);
            return Ok(result.Result);
        }

        [HttpPost("{friendId}")]
        public async Task<ActionResult<FriendshipDTO>> CreateFriendship(Guid friendId)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Создание дружбы между инициатором {UserId} и получателем {FriendId}",
                userId, friendId
            );

            ServiceResult<FriendshipDTO> result
                = await _friendshipsService.CreateFriendShip(userId, friendId);
            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug(
                "Успешное создание между инициатором {UserId} и получателем {FriendId}",
                userId, friendId
            );
            return Ok(result.Result);
        }

        [HttpPatch("status/{friendshipId}")]
        public async Task<ActionResult<FriendshipDTO>> UpdateFriendshipStatus(
            Guid friendshipId, [FromBody] UpdateFriendshipStatusRequest request
        )
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Изменение статуса дружбы {FriendshipId} на {Status} пользователем {UserId}",
                friendshipId, userId, request.Status
            );

            ServiceResult<FriendshipDTO> result 
                = await _friendshipsService.UpdateFriendShipStatus(userId, friendshipId, request.Status);
            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Успешное изменение статуса дружбы {FriendshipId}", friendshipId);
            return Ok(result.Result);
        }
    }
}
