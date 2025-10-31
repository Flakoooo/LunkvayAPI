using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Friends.Models.Requests
{
    public class UpdateFriendshipStatusRequest
    {
        [Required]
        public required FriendshipStatus Status { get; init; }
    }
}
