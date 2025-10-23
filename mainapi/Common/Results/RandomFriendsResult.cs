using LunkvayAPI.Common.DTO;

namespace LunkvayAPI.Common.Results
{
    public class RandomFriendsResult
    {
        public List<UserListItemDTO> Friends { get; set; } = [];
        public int FriendsCount { get; set; } = 0;
    }
}
