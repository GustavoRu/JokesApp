using System.Net.Http.Headers;
using System.Text.Json;
using BackendApi.Jokes.DTOs;

namespace BackendApi.Jokes.Services
{
    public interface IExternalJokeService
    {
        Task<ExternalJokeDto> GetRandomChuckNorrisJokeAsync();
        Task<ExternalJokeDto> GetRandomDadJokeAsync();
        Task<List<JokePairDto>> GetPairedJokesAsync(int count = 5);
        Task<string> GetCombinedJokeAsync();
    }

    public class ExternalJokeService : IExternalJokeService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ExternalJokeService> _logger;

        public ExternalJokeService(IHttpClientFactory clientFactory, ILogger<ExternalJokeService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<ExternalJokeDto> GetRandomChuckNorrisJokeAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("ChuckNorris");
                var response = await client.GetAsync("https://api.chucknorris.io/jokes/random");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Chuck Norris API response: {content}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var joke = JsonSerializer.Deserialize<ChuckNorrisApiResponse>(content, options);

                return new ExternalJokeDto
                {
                    Id = joke?.Id ?? "",
                    Text = joke?.Value ?? "No joke available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Chuck Norris joke");
                throw;
            }
        }

        public async Task<ExternalJokeDto> GetRandomDadJokeAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("DadJokes");
                var request = new HttpRequestMessage(HttpMethod.Get, "https://icanhazdadjoke.com/");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Dad Joke API response: {content}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var joke = JsonSerializer.Deserialize<DadJokeApiResponse>(content, options);

                return new ExternalJokeDto
                {
                    Id = joke?.Id ?? "",
                    Text = joke?.Joke ?? "No joke available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Dad joke");
                throw;
            }
        }

        public async Task<List<JokePairDto>> GetPairedJokesAsync(int count = 5)
        {
            try
            {
                // Crear las tareas para obtener los chistes
                var chuckTasks = Enumerable.Range(0, count)
                    .Select(_ => GetRandomChuckNorrisJokeAsync())
                    .ToList();

                var dadTasks = Enumerable.Range(0, count)
                    .Select(_ => GetRandomDadJokeAsync())
                    .ToList();

                // Esperar a que todas las tareas se completen
                await Task.WhenAll(chuckTasks.Concat(dadTasks));

                // Emparejar los chistes
                var pairs = new List<JokePairDto>();
                for (int i = 0; i < count; i++)
                {
                    var chuckJoke = await chuckTasks[i];
                    var dadJoke = await dadTasks[i];

                    pairs.Add(new JokePairDto
                    {
                        Chuck = chuckJoke.Text,
                        Dad = dadJoke.Text,
                        Combined = CombineJokes(chuckJoke.Text, dadJoke.Text)
                    });
                }

                return pairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paired jokes");
                throw;
            }
        }

        public async Task<string> GetCombinedJokeAsync()
        {
            var chuckJoke = await GetRandomChuckNorrisJokeAsync();
            var dadJoke = await GetRandomDadJokeAsync();
            return CombineJokes(chuckJoke.Text, dadJoke.Text);
        }

        private string CombineJokes(string chuckJoke, string dadJoke)
        {
            // Implementar lógica creativa para combinar chistes
            var random = new Random();
            return random.Next(2) == 0
                ? $"{chuckJoke} Speaking of which, {dadJoke.ToLower()}"
                : $"While {dadJoke.ToLower()}, {chuckJoke.ToLower()}";
        }

        private class ChuckNorrisApiResponse
        {
            public string[] Categories { get; set; } = Array.Empty<string>();
            public string Created_at { get; set; } = string.Empty;
            public string Icon_url { get; set; } = string.Empty;
            public string Id { get; set; } = string.Empty;
            public string Updated_at { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        private class DadJokeApiResponse
        {
            public string Id { get; set; } = string.Empty;
            public string Joke { get; set; } = string.Empty;
            public int Status { get; set; }
        }
    }
}