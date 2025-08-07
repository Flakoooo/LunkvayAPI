using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FriendsController(IFriendsService friendsService, ILogger<FriendsController> logger) : Controller
    {
        private readonly IFriendsService _friendsService = friendsService;
        private readonly ILogger<FriendsController> _logger = logger;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserFriends(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1?pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);

            ServiceResult<IEnumerable<UserListItemDTO>> result
                = await _friendsService.GetUserFriends(userId, page, pageSize);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Запрос друзей пользователя {Id}, страница {Page}", userId, page);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }

        [HttpGet("{userId}/random")]
        public async Task<IActionResult> GetRandomFriends(Guid userId, [FromQuery] int count = 6)
        {
            // /api/v1/friends/{userId}/random
            // /api/v1/friends/{userId}/random?count=4
            _logger.LogInformation("Запрос друзей пользователя {Id} для профиля", userId);

            ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)> result
                = await _friendsService.GetRandomUserFriends(userId, count);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Запрос случайных друзей пользователя {Id}", userId);
                return Ok(result.Result);
            }

            _logger.LogError("Ошибка: (Status: {StatusCode}) {Error}", (int)result.StatusCode, result.Error);
            return StatusCode((int)result.StatusCode, result.Error);
        }
    }
}
