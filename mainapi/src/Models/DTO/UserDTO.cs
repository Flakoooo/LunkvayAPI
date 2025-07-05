namespace LunkvayAPI.src.Models.DTO
{
    public record class UserDTO
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsOnline { get; set; }
    }
}
