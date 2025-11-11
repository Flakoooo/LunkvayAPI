using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_name")]
        public required string UserName { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = "";

        [Column("last_name")]
        public string LastName { get; set; } = "";

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("is_online")]
        public bool IsOnline { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = null;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; } = null;

        [Column("last_login")]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;


        public string FullName => $"{FirstName} {LastName}";

        public static User Create(
            string userName, string email, string password,
            string firstName = "", string lastName = "",
            bool isDeleted = false, bool isOnline = true
        ) => new()
        {
            //Id в базе данных
            UserName = userName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            //CreatedAt в базе данных по UTC
            IsDeleted = isDeleted,
            //DeletedAt nullable
            //LastLogin в базе данных по UTC
            IsOnline = isOnline
        };
    }
}
