using BackendApi.Data;
using BackendApi.Jokes.DTOs;
using BackendApi.Jokes.Models;
using BackendApi.Topics.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Jokes.Services
{
    public interface IJokeService
    {
        Task<JokeDto> CreateJokeAsync(CreateJokeDto dto, int authorId);
        Task<JokeDto> UpdateJokeAsync(int id, UpdateJokeDto dto, int userId, bool isAdmin);
        Task<bool> DeleteJokeAsync(int id, int userId, bool isAdmin);
        Task<JokeDto?> GetJokeByIdAsync(int id);
        Task<List<JokeDto>> FilterJokesAsync(int? minWords = null, string? contains = null, int? authorId = null, int? topicId = null);
        Task<JokeDto> GetRandomLocalJokeAsync();
    }

    public class JokeService : IJokeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JokeService> _logger;

        public JokeService(ApplicationDbContext context, ILogger<JokeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<JokeDto> CreateJokeAsync(CreateJokeDto dto, int authorId)
        {
            var topics = await _context.Topics
                .Where(t => dto.TopicIds.Contains(t.Id))
                .ToListAsync();

            var joke = new JokeModel
            {
                Text = dto.Text,
                AuthorId = authorId,
                Topics = topics
            };

            _context.Jokes.Add(joke);
            await _context.SaveChangesAsync();

            return await GetJokeDtoAsync(joke);
        }

        public async Task<JokeDto> UpdateJokeAsync(int id, UpdateJokeDto dto, int userId, bool isAdmin)
        {
            var joke = await _context.Jokes
                .Include(j => j.Topics)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (joke == null)
                throw new KeyNotFoundException("Joke not found");

            if (joke.AuthorId != userId && !isAdmin)
                throw new UnauthorizedAccessException("You don't have permission to update this joke");

            joke.Text = dto.Text;

            // Update topics
            var topics = await _context.Topics
                .Where(t => dto.TopicIds.Contains(t.Id))
                .ToListAsync();

            joke.Topics.Clear();
            foreach (var topic in topics)
            {
                joke.Topics.Add(topic);
            }

            await _context.SaveChangesAsync();

            return await GetJokeDtoAsync(joke);
        }

        public async Task<bool> DeleteJokeAsync(int id, int userId, bool isAdmin)
        {
            var joke = await _context.Jokes.FindAsync(id);

            if (joke == null)
                return false;

            if (joke.AuthorId != userId && !isAdmin)
                throw new UnauthorizedAccessException("You don't have permission to delete this joke");

            _context.Jokes.Remove(joke);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<JokeDto?> GetJokeByIdAsync(int id)
        {
            var joke = await _context.Jokes
                .Include(j => j.Author)
                .Include(j => j.Topics)
                .FirstOrDefaultAsync(j => j.Id == id);

            return joke == null ? null : await GetJokeDtoAsync(joke);
        }

        public async Task<List<JokeDto>> FilterJokesAsync(int? minWords = null, string? contains = null, int? authorId = null, int? topicId = null)
        {
            var query = _context.Jokes
                .Include(j => j.Author)
                .Include(j => j.Topics)
                .AsQueryable();

            if (minWords.HasValue)
                query = query.Where(j => j.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= minWords.Value);

            if (!string.IsNullOrEmpty(contains))
                query = query.Where(j => j.Text.Contains(contains));

            if (authorId.HasValue)
                query = query.Where(j => j.AuthorId == authorId.Value);

            if (topicId.HasValue)
                query = query.Where(j => j.Topics.Any(t => t.Id == topicId.Value));

            var jokes = await query.ToListAsync();
            var jokeDtos = new List<JokeDto>();
            foreach (var joke in jokes)
            {
                jokeDtos.Add(await GetJokeDtoAsync(joke));
            }
            return jokeDtos;
        }

        public async Task<JokeDto> GetRandomLocalJokeAsync()
        {
            var count = await _context.Jokes.CountAsync();
            var random = new Random();
            var skip = random.Next(count);

            var joke = await _context.Jokes
                .Include(j => j.Author)
                .Include(j => j.Topics)
                .Skip(skip)
                .FirstOrDefaultAsync();

            if (joke == null)
                throw new InvalidOperationException("No jokes found in database");

            return await GetJokeDtoAsync(joke);
        }

        private Task<JokeDto> GetJokeDtoAsync(JokeModel joke)
        {
            return Task.FromResult(new JokeDto
            {
                Id = joke.Id,
                Text = joke.Text,
                CreatedAt = joke.CreatedAt,
                AuthorName = joke.Author?.Name ?? "Unknown",
                AuthorId = joke.AuthorId,
                Origin = joke.Origin,
                Topics = joke.Topics.Select(t => t.Name).ToList()
            });
        }
    }
}