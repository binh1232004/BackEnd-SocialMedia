using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Services;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService; 
        private readonly IUserRepository _userRepository;

        public UserController(IUserService userService, IUserRepository userRepository) 
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _userService.GetAllUsersAsync());
        }

        // ✅ API này yêu cầu Token JWT
        [Authorize] // 🔥 Thêm dòng này để bắt buộc Authentication
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }


        // ✅ API Đăng nhập - Trả về JWT Token
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var authResponse = await _userService.LoginAsync(loginDto);
            if (authResponse == null) return Unauthorized("Email hoặc mật khẩu không đúng.");

            return Ok(authResponse);
        }[AllowAnonymous]

        [HttpDelete]
        public async Task<ActionResult<AuthResponseDto>> Login([FromQuery]Guid id)
        {
            await _userRepository.DeleteAsync(id);

            return Ok();
        }


    }
}
