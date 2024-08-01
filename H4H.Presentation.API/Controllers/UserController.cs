using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null)
            {
                return NotFound("Users not found.");
            }
            return Ok(users);
        }

        // GET: api/user/{UserId}
        [HttpGet("{UserId}")]
        public async Task<IActionResult> GetUserById(Guid UserId)
        {
            var user = await _userService.GetUserByIdAsync(UserId);
            if (user == null)
            {
                return NotFound($"User with ID {UserId} not found.");
            }
            return Ok(user);

        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User is null.");
            }

            // Add additional validation here if needed
            await _userService.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { UserId = user.UserId }, user);
        }

        // PUT: api/user/{UserId}
        [HttpPut("{UserId}")]
        public async Task<IActionResult> UpdateUser(Guid UserId, [FromBody] User user)
        {
            if (user == null || UserId != user.UserId)
            {
                return BadRequest("User is null or ID mismatch.");
            }

            // Additional validation and checks here

            await _userService.UpdateUserAsync(user);
            return NoContent();
        }

        // DELETE: api/user/{UserId}
        [HttpDelete("{UserId}")]
        public async Task<IActionResult> DeleteUser(Guid UserId)
        {
            var user = await _userService.GetUserByIdAsync(UserId);
            if (user == null)
            {
                return NotFound($"User with ID {UserId} not found.");
            }

            await _userService.DeleteUserAsync(UserId);
            return NoContent();
        }
    }
}
