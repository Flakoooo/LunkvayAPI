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
        public async Task<IActionResult> GetUserFriends(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // /api/v1/friends/{userId}
            // /api/v1/friends/{userId}?page=1
            // /api/v1/friends/{userId}?page=1?pageSize=10
            _logger.LogInformation("Запрос друзей пользователя {Id}, страница {Page}", userId, page);
            var result = await _friendsService.GetUserFriends(Guid.Parse(userId), page, pageSize);
            return Ok(result);
        }

        [HttpGet("{userId}/random")]
        public async Task<IActionResult> GetRandomFriends(string userId, [FromQuery] int count = 6)
        {
            // /api/v1/friends/{userId}/random
            // /api/v1/friends/{userId}/random?count=4
            _logger.LogInformation("Запрос друзей пользователя {Id} для профиля", userId);
            var (result, _) = await _friendsService.GetRandomUserFriends(Guid.Parse(userId), count);
            return Ok(result);
        }
    }
}
