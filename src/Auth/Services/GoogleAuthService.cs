using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using BackendApi.Users.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace BackendApi.Auth.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(
            IConfiguration configuration,
            ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public string GetAuthorizationUrl()
        {
            var clientId = _configuration["Google:ClientId"];
            var redirectUri = _configuration["Google:RedirectUri"];
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["response_type"] = "code",
                ["scope"] = "email profile",
                ["redirect_uri"] = redirectUri!,
                ["access_type"] = "offline",
                ["state"] = GenerateState()
            };

            var url = QueryHelpers.AddQueryString(
                "https://accounts.google.com/o/oauth2/v2/auth",
                parameters);

            _logger.LogInformation($"Generated Google authorization URL: {url}");
            _logger.LogInformation($"Redirect URI configured: {redirectUri}");

            return url;
        }

        public async Task<GoogleUserInfo> GetUserInfoAsync(string code)
        {
            try
            {
                _logger.LogInformation("Starting token exchange for code");
                var tokenResponse = await GetAccessTokenAsync(code);
                
                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    _logger.LogError("Received empty access token");
                    throw new Exception("Failed to get access token");
                }

                _logger.LogInformation("Successfully obtained access token, fetching user info");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                var userInfoResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get user info. Status: {userInfoResponse.StatusCode}, Error: {errorContent}");
                    throw new Exception($"Failed to get user info. Status: {userInfoResponse.StatusCode}");
                }

                var content = await userInfoResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Google user info response: {content}");
                
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (userInfo == null)
                {
                    throw new Exception("Failed to deserialize user info");
                }

                _logger.LogInformation($"Parsed user info - Email: {userInfo.Email}, Name: {userInfo.Name}");
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserInfoAsync");
                throw;
            }
        }

        private async Task<GoogleTokenResponse> GetAccessTokenAsync(string code)
        {
            try
            {
                var redirectUri = _configuration["Google:RedirectUri"];
                _logger.LogInformation($"Using redirect URI for token exchange: {redirectUri}");

                var tokenRequest = new Dictionary<string, string>
                {
                    ["client_id"] = _configuration["Google:ClientId"]!,
                    ["client_secret"] = _configuration["Google:ClientSecret"]!,
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri!,
                    ["grant_type"] = "authorization_code"
                };

                var tokenResponse = await _httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequest));

                var responseContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Token response status: {tokenResponse.StatusCode}");
                
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Token request failed. Status: {tokenResponse.StatusCode}, Error: {responseContent}");
                    throw new Exception($"Token request failed. Status: {tokenResponse.StatusCode}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent, options) 
                    ?? throw new Exception("Failed to deserialize token response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessTokenAsync");
                throw;
            }
        }

        private string GenerateState()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }

    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;

        // Nombres de propiedades como vienen de Google
        public string access_token { get => AccessToken; set => AccessToken = value; }
        public string token_type { get => TokenType; set => TokenType = value; }
        public int expires_in { get => ExpiresIn; set => ExpiresIn = value; }
        public string refresh_token { get => RefreshToken; set => RefreshToken = value; }
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
    }
}