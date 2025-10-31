using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Auth.Models.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Имя пользователя не может быть пустым")]
        public string UserName { get; init; } = string.Empty;

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
