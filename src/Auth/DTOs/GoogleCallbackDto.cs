using System.ComponentModel.DataAnnotations;

namespace BackendApi.Auth.DTOs
{
    public class GoogleCallbackDto
    {
        [Required(ErrorMessage = "El código de autorización es requerido")]
        public string Code { get; set; } = string.Empty;
    }
}