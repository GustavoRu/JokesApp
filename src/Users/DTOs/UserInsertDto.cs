namespace BackendApi.Users.DTOs
{
    public class UserInsertDto
    {
        public string Username { get; set; } = string.Empty; // Mantenemos Username por compatibilidad con el front
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "user"; // Valor por defecto
    }
}