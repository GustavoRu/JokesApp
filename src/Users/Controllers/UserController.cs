using Microsoft.AspNetCore.Mvc;
using BackendApi.Users.DTOs;
using BackendApi.Users.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Users.Controllers
{
    [ApiController]
    [Route("api/usuario")] // Cambiado para coincidir con el enunciado
    [Authorize] // Require authentication for all endpoints by default
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UserInsertDto> _userInsertValidator;
        private readonly IValidator<UserUpdateDto> _userUpdateValidator;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService, 
            IValidator<UserInsertDto> userInsertValidator, 
            IValidator<UserUpdateDto> userUpdateValidator,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _userInsertValidator = userInsertValidator;
            _userUpdateValidator = userUpdateValidator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "user,admin")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // Verificar si el usuario está autenticado
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Log all claims for debugging
            _logger.LogInformation("Claims in token:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Type: {claim.Type}, Value: {claim.Value}");
            }

            // Verificar si tiene el rol correcto
            if (!User.IsInRole("user") && !User.IsInRole("admin"))
            {
                return StatusCode(403, new { message = "Usuario no tiene los permisos necesarios" });
            }

            // Obtener el email del token
            var email = User.Claims.FirstOrDefault(c => 
                c.Type == System.Security.Claims.ClaimTypes.Email ||
                c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "El token no contiene el email del usuario" });
            }

            // Buscar el usuario en la base de datos
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado en la base de datos" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }
    }
}