using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Auth.Models.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
    }
}
