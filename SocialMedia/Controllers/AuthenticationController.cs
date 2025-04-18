using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs;
using SocialMedia.Application.Interfaces;

namespace SocialMedia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly Application.Interfaces.IEmailService _emailService;
        private readonly IUserService _userService;

        public AuthenticationController(Application.Interfaces.IEmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        // Gửi OTP qua email
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] UserEmail request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid email address.");
            }

            var userExists = await _userService.UserExistsAsync(request.Email);
            if (userExists)
            {
                return BadRequest("Email đã tồn tại.");
            }

            var otp = await _emailService.GenerateOtpAsync(request.Email);
            var message = $"Your OTP code is: {otp}";
            await _emailService.SendEmailAsync(request.Email, "Your OTP Code", message);

            return Ok("OTP has been sent to your email.");
        }
        

        // Xác thực OTP và đăng ký người dùng
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromQuery] string otp, [FromForm] RegisterDto registerDto)
        {
            var isOtpValid = await _emailService.VerifyOtpAsync(registerDto.Email, otp);
            if (!isOtpValid)
            {
                return BadRequest("Invalid, expired OTP or exceeded attempts.");
            }

            var userExists = await _userService.UserExistsAsync(registerDto.Email);
            if (userExists)
            {
                return BadRequest("Email đã tồn tại.");
            }

            var authResponse = await _userService.RegisterAsync(registerDto);
            if (authResponse == null)
            {
                return BadRequest("Có lỗi khi đăng ký.");
            }

            return Ok(authResponse);
        }

    }
}
