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
        private IValidator<UserInsertDto> _userInsertValidator;
        private IValidator<UserUpdateDto> _userUpdateValidator;

        public UserController(IUserService userService, IValidator<UserInsertDto> userInsertValidator, IValidator<UserUpdateDto> userUpdateValidator)
        {
            _userService = userService;
            _userInsertValidator = userInsertValidator;
            _userUpdateValidator = userUpdateValidator;
        }

        [HttpGet]
        [Authorize(Roles = "user,admin")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
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