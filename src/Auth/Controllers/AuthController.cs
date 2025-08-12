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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly GoogleAuthService _googleAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IJwtService jwtService,
            GoogleAuthService googleAuthService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
            _logger = logger;
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

        [HttpGet("external/google-login")]
        public IActionResult GoogleLogin()
        {
            var authorizationUrl = _googleAuthService.GetAuthorizationUrl();
            _logger.LogInformation($"Redirecting to Google authorization URL: {authorizationUrl}");
            return Redirect(authorizationUrl);
        }

        [HttpGet("external/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? error)
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError($"Google OAuth error: {error}");
                    return BadRequest($"Google authentication error: {error}");
                }

                if (string.IsNullOrEmpty(code))
                {
                    _logger.LogError("No authorization code received from Google");
                    return BadRequest("No authorization code received");
                }

                // Obtener información del usuario de Google
                var googleUser = await _googleAuthService.GetUserInfoAsync(code);

                // Buscar o crear usuario en nuestra base de datos
                var user = await _userService.GetUserByEmailAsync(googleUser.Email);
                if (user == null)
                {
                    user = new UserModel
                    {
                        Email = googleUser.Email,
                        Name = googleUser.Name,
                        PasswordHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()), // Contraseña aleatoria
                        Role = "user",
                        IsActive = true
                    };

                    user = await _userService.CreateUserAsync(user);
                }

                // Generar JWT
                var token = _jwtService.GenerateToken(user);

                // Redirigir con el token
                var redirectUrl = $"/auth-success?token={token}";
                _logger.LogInformation($"Authentication successful, redirecting to: {redirectUrl}");
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                return BadRequest($"Authentication failed: {ex.Message}");
            }
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