using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Auth.Models.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Имя пользователя не может быть пустым")]
        public required string UserName { get; init; }

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
