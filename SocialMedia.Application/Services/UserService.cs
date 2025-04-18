using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using SocialMedia.Application.DTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IJwtTokenService jwtTokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive
            } : null;
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            var user = new User
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                IsActive = userDto.IsActive
            };
            user.SetPassword("default_password"); // Mật khẩu mặc định

            var createdUser = await _userRepository.CreateAsync(user);
            return new UserDto
            {
                Id = createdUser.Id,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                IsActive = createdUser.IsActive
            };
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user != null)
            {
                user.FullName = userDto.FullName;
                user.Email = userDto.Email;
                user.IsActive = userDto.IsActive;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task DeleteUserAsync(Guid id) => await _userRepository.DeleteAsync(id);

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.UserExistsAsync(email);
        }
        public async Task<AlertDto?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.UserExistsAsync(registerDto.Email);
            if (existingUser)
                return new AlertDto
                {
                    Message = "Email already exists.",
                    AlertType = "error"  
                };
            
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = registerDto.FullName,
                Image = registerDto.Image,
                DoB = registerDto.DoB,
                Gender = registerDto.Gender,
                Biography = registerDto.Biography,
                Email = registerDto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            newUser.SetPassword(registerDto.Password);

            await _userRepository.CreateAsync(newUser);

            return new AlertDto
            {
                Message = "Registration successful.",
                AlertType = "success"
            };
        }
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == loginDto.Email);

            if (user == null || !user.VerifyPassword(loginDto.Password))
                return null; 

            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(30)
            };
        }
    }
}

