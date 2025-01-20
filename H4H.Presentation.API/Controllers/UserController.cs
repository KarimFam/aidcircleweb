using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Application.DTOs;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
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
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User is null.");
            }

            var user = _mapper.Map<User>(userDto);
            await _userService.AddUserAsync(user);
            var createdUserDto = _mapper.Map<UserDto>(user);
            return CreatedAtAction(nameof(GetUserById), new { UserId = user.UserId }, createdUserDto);
        }

        // PUT: api/user/{UserId}
        [HttpPut("{UserId}")]
        public async Task<IActionResult> UpdateUser(Guid UserId, [FromBody] UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User is null.");
            }

            var existingUser = await _userService.GetUserByIdAsync(UserId);
            if (existingUser == null)
            {
                return NotFound($"User with ID {UserId} not found.");
            }

            _mapper.Map(userDto, existingUser);
            await _userService.UpdateUserAsync(existingUser);
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
