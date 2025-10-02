using System.ComponentModel.DataAnnotations;

namespace LunkvayIdentityService.Models.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Имя пользователя не может быть пустым")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
