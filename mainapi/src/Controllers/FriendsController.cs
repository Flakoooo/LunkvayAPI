using LunkvayAPI.src.Services;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult> GetUserFriends(string userId)
        {
            _logger.LogInformation("api/v1/friends/{Id}", userId);
            var result = await _friendsService.GetUserFriends(Guid.Parse(userId));
            return Ok(result);
        }
    }
}
