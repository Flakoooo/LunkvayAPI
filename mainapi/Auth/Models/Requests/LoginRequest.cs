using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Auth.Models.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
