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
        public DateTime? LastLogin { get; set; }
        public bool? IsOnline { get; set; }
    }
}
