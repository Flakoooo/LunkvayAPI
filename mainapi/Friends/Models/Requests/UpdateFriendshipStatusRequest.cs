using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Friends.Models.Requests
{
    public class UpdateFriendshipStatusRequest
    {
        [Required]
        public FriendshipStatus Status { get; init; }
    }
}
