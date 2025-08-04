using System.Security.Cryptography;
using System.Text;
using BackendApi.Auth.DTOs;
using BackendApi.Auth.Services;
using BackendApi.Users.Models;
using BackendApi.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserModel>> Register(RegisterDto registerDto)
        {
            if (await _userService.GetUserByEmailAsync(registerDto.Email) != null)
            {
                return BadRequest("Email already exists");
            }

            var user = new UserModel
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password)
            };

            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(createdUser);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserModel>> GetCurrentUser()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}