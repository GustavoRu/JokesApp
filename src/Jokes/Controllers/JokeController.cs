using BackendApi.Jokes.DTOs;
using BackendApi.Jokes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Jokes.Controllers
{
    [ApiController]
    [Route("api/chistes")]
    [Authorize]
    public class JokeController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IExternalJokeService _externalJokeService;
        private readonly ILogger<JokeController> _logger;

        public JokeController(
            IJokeService jokeService,
            IExternalJokeService externalJokeService,
            ILogger<JokeController> logger)
        {
            _jokeService = jokeService;
            _externalJokeService = externalJokeService;
            _logger = logger;
        }

        [HttpGet("aleatorio")]
        public async Task<ActionResult<ExternalJokeDto>> GetRandomJoke([FromQuery] string? origen = null)
        {
            try
            {
                if (string.IsNullOrEmpty(origen))
                {
                    // Si no se especifica origen, elegir aleatoriamente entre las fuentes
                    var random = new Random();
                    origen = random.Next(3) switch
                    {
                        0 => "Chuck",
                        1 => "Dad",
                        _ => "Local"
                    };
                }

                switch (origen.ToLower())
                {
                    case "chuck":
                        return Ok(await _externalJokeService.GetRandomChuckNorrisJokeAsync());
                    case "dad":
                        return Ok(await _externalJokeService.GetRandomDadJokeAsync());
                    case "local":
                        return Ok(await _jokeService.GetRandomLocalJokeAsync());
                    default:
                        return BadRequest(new { error = "Origen no válido. Use 'Chuck', 'Dad' o 'Local'" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random joke");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("emparejados")]
        public async Task<ActionResult<List<JokePairDto>>> GetPairedJokes()
        {
            try
            {
                var pairs = await _externalJokeService.GetPairedJokesAsync();
                return Ok(pairs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paired jokes");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("combinado")]
        public async Task<ActionResult<string>> GetCombinedJoke()
        {
            try
            {
                var joke = await _externalJokeService.GetCombinedJokeAsync();
                return Ok(new { joke });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combined joke");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<JokeDto>> CreateJoke(CreateJokeDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var joke = await _jokeService.CreateJokeAsync(dto, userId);
                return CreatedAtAction(nameof(GetJokeById), new { id = joke.Id }, joke);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating joke");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JokeDto>> GetJokeById(int id)
        {
            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
                return NotFound();

            return Ok(joke);
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<List<JokeDto>>> FilterJokes(
            [FromQuery] int? minPalabras,
            [FromQuery] string? contiene,
            [FromQuery] int? autorId,
            [FromQuery] int? tematicaId)
        {
            try
            {
                var jokes = await _jokeService.FilterJokesAsync(minPalabras, contiene, autorId, tematicaId);
                return Ok(jokes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering jokes");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<JokeDto>> UpdateJoke(int id, UpdateJokeDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var isAdmin = User.IsInRole("admin");

                var joke = await _jokeService.UpdateJokeAsync(id, dto, userId, isAdmin);
                return Ok(joke);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating joke");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJoke(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var isAdmin = User.IsInRole("admin");

                var result = await _jokeService.DeleteJokeAsync(id, userId, isAdmin);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting joke");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}