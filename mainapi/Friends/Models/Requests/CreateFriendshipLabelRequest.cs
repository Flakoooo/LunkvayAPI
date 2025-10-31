using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Friends.Models.Requests
{
    public class CreateFriendshipLabelRequest
    {
        [Required]
        public Guid FriendshipId { get; init; }

        [Required]
        public string Label { get; init; } = string.Empty;
    }
}
