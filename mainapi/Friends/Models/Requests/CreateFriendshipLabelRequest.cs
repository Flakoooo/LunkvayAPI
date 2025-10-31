using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Friends.Models.Requests
{
    public class CreateFriendshipLabelRequest
    {
        [Required]
        public required Guid FriendshipId { get; init; }

        [Required]
        public required string Label { get; init; }
    }
}
