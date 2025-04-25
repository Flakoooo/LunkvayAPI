using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.src.Models.Entities
{
    public class User
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Пароль не может быть пустым")]
        public required string PasswordHash { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
