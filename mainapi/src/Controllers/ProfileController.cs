using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProfileController(IProfileService profileService) : Controller
    {
        private readonly IProfileService _profileService = profileService;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfileByUserId(string userId)
        {
            try
            {
                var result = await _profileService.GetUserProfileById(Guid.Parse(userId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
