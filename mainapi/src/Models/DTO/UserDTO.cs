namespace LunkvayAPI.src.Models.DTO
{
    public record class UserDTO
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? IsOnline { get; set; }

        public static UserDTO Create(
            Guid id, string userName, string email,
            DateTime createdAt, bool isDeleted, DateTime lastLogin, bool isOnline,
            string? firstName = null, string? lastName = null, DateTime? deletedAt = null
        ) => new()
        {
            Id = id,
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = createdAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt,
            LastLogin = lastLogin,
            IsOnline = isOnline
        };
    }
}
