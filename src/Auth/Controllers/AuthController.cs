using System.Security.Cryptography;
using System.Text;
using System.Web;
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

        /// <summary>
        /// Inicia el flujo de autenticación con Google
        /// </summary>
        /// <returns>URL de autorización de Google y estado para validación</returns>
        /// <summary>
        /// Inicia el flujo de autenticación con Google
        /// </summary>
        [HttpGet("external/google-login")]
        public ActionResult<object> GoogleLogin()
        {
            var authorizationUrl = _googleAuthService.GetAuthorizationUrl();
            _logger.LogInformation($"Generated Google authorization URL: {authorizationUrl}");
            return Ok(new { 
                url = authorizationUrl,
                message = @"1. Abre esta URL en una nueva pestaña
2. Completa el login con Google
3. Cuando te redirija, copia SOLO el parámetro 'code' de la URL (está entre 'code=' y '&scope=')
4. Usa ese código en el endpoint POST /api/auth/external/google-callback"
            });
        }

        /// <summary>
        /// Completa el flujo de autenticación con Google y devuelve el token JWT
        /// </summary>
        [HttpPost("external/google-callback")]
        public async Task<ActionResult<object>> GoogleCallback([FromBody] GoogleCallbackDto callbackDto)
        {
            try
            {
                if (string.IsNullOrEmpty(callbackDto.Code))
                {
                    _logger.LogError("No authorization code received from Google");
                    return BadRequest(new { error = "No authorization code received" });
                }

                // Decodificar el código si viene con codificación URL
                var decodedCode = HttpUtility.UrlDecode(callbackDto.Code.Trim());

                // Obtener información del usuario de Google
                var googleUser = await _googleAuthService.GetUserInfoAsync(decodedCode);
                _logger.LogInformation($"Received Google user info - Email: {googleUser.Email}, Name: {googleUser.Name}");

                if (string.IsNullOrEmpty(googleUser.Email))
                {
                    _logger.LogError("Google user info does not contain email");
                    return BadRequest(new { error = "No se pudo obtener el email del usuario de Google" });
                }

                // Buscar o crear usuario en nuestra base de datos
                var user = await _userService.GetUserByEmailAsync(googleUser.Email);
                if (user == null)
                {
                    _logger.LogInformation($"Creating new user with email: {googleUser.Email}");
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
                else
                {
                    _logger.LogInformation($"Found existing user with email: {user.Email}");
                }

                // Generar JWT
                var token = _jwtService.GenerateToken(user);

                return Ok(new { 
                    token = token,
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        name = user.Name,
                        role = user.Role
                    },
                    message = @"Autenticación exitosa. Sigue estos pasos:
1. Copia el token que aparece arriba
2. Haz click en el botón 'Authorize' en la parte superior
3. En el campo que aparece, escribe: Bearer [espacio] y pega el token
4. Ejemplo: Bearer eyJhbGci...
5. Click en 'Authorize' y luego en 'Close'
6. ¡Listo! Ahora puedes usar los endpoints protegidos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                return BadRequest(new { error = $"Authentication failed: {ex.Message}" });
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