using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;
using LunkvayAPI.Friends.Models.Requests;
using LunkvayAPI.Friends.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.Friends.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class FriendsController(IFriendsService friendsService, ILogger<FriendsController> logger) : Controller
    {
        private readonly IFriendsService _friendsService = friendsService;
        private readonly ILogger<FriendsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FriendDTO>>> GetCurrentUserFriends(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1&pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);

            ServiceResult<List<FriendDTO>> result
                = await _friendsService.GetFriends(userId, page, pageSize, true);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Запрос друзей пользователя {Id}, страница {Page}", userId, page);
            return Ok(result.Result);
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<ActionResult<IReadOnlyList<FriendDTO>>> GetUserFriends(
            Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {
            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1&pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);

            ServiceResult<List<FriendDTO>> result
                = await _friendsService.GetFriends(userId, page, pageSize);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Запрос друзей пользователя {Id}, страница {Page}", userId, page);
            return Ok(result.Result);
        }

        [HttpPost("{friendId}")]
        public async Task<ActionResult<FriendDTO>> CreateFriendship(Guid friendId)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Создание дружбы между инициатором {UserId} и получателем {FriendId}",
                userId, friendId
            );

            ServiceResult<FriendDTO> result
                = await _friendsService.CreateFriendShip(userId, friendId);
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
        public async Task<ActionResult<FriendDTO>> UpdateFriendshipStatus(
            Guid friendshipId, [FromBody] UpdateFriendshipStatusRequest request
        )
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            _logger.LogInformation(
                "Изменение статуса дружбы {FriendshipId} на {Status} пользователем {UserId}",
                friendshipId, userId, request.Status
            );

            ServiceResult<FriendDTO> result 
                = await _friendsService.UpdateFriendShipStatus(userId, friendshipId, request.Status);
            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Успешное изменение статуса дружбы {FriendshipId}", friendshipId);
            return Ok(result.Result);
        }

        /*
        [HttpGet("random")]
        public async Task<ActionResult<IReadOnlyList<FriendDTO>>> GetCurrentUserRandomFriends([FromQuery] int count = 6)
        {
            Guid userId = (Guid)HttpContext.Items["UserId"]!;

            // /api/v1/friends/{userId}/random
            // /api/v1/friends/{userId}/random?count=4
            _logger.LogInformation("Запрос друзей пользователя {Id} для профиля", userId);

            ServiceResult<(List<FriendDTO> Friends, int FriendsCount)> result
                = await _friendsService.GetRandomUserFriends(userId, count);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Запрос случайных друзей пользователя {Id}", userId);
            return Ok(result.Result);
        }

        [AllowAnonymous]
        [HttpGet("{userId}/random")]
        public async Task<IActionResult> GetUserRandomFriends(Guid userId, [FromQuery] int count = 6)
        {
            // /api/v1/friends/{userId}/random
            // /api/v1/friends/{userId}/random?count=4
            _logger.LogInformation("Запрос друзей пользователя {Id} для профиля", userId);

            ServiceResult<(List<FriendDTO> Friends, int FriendsCount)> result
                = await _friendsService.GetRandomUserFriends(userId, count);

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
                return StatusCode((int)result.StatusCode, result.Error);
            }

            _logger.LogDebug("Запрос случайных друзей пользователя {Id}", userId);
            return Ok(result.Result);
        }
        */
    }
}
