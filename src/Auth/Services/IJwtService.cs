using BackendApi.Users.Models;

namespace BackendApi.Auth.Services
{
    public interface IJwtService
    {
        string GenerateToken(UserModel user);
        bool ValidateToken(string token);
    }
}