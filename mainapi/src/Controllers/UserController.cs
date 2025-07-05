using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LunkvayAPI.src.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await _userService.GetUserById(Guid.Parse(userId));
                return Ok(user);
            }
            catch (FormatException)
            {
                return BadRequest("Неверный формат GUID");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var user = await _userService.GetUsers();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
