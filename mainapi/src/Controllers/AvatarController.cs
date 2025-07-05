using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvatarController(IAvatarService avatarService) : Controller
    {
        private readonly IAvatarService _avatarService = avatarService;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAvatarById(string userId)
        {
            try
            {
                var avatarArray = await _avatarService.GetUserAvatarById(Guid.Parse(userId));
                return File(avatarArray, MediaTypeNames.Image.Jpeg);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
