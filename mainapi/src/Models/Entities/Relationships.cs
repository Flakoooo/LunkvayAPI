using LunkvayAPI.src.Models.Enums;

namespace LunkvayAPI.src.Models.Entities
{
    public class Relationships
    {
        public required string UserId1 { get; set; }
        public required string UserId2 { get; set; }
        public required RelationshipsStatus Status { get; set; }
        public required string InitiatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
