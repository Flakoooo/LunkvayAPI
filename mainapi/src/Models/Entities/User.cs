using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_name")]
        [Required(ErrorMessage = "Имя пользователя не может быть пустым")]
        public required string UserName { get; set; }

        [Column("email")]
        [Required(ErrorMessage = "Почта не может быть пустой")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public required string Email { get; set; }

        [Column("password_hash")]
        [Required(ErrorMessage = "Пароль не может быть пустым")]
        public required string PasswordHash { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = "";

        [Column("last_name")]
        public string LastName { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
