using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;

namespace LunkvayAPI.Friends.Services
{
    public interface IFriendshipsService
    {
        Task<ServiceResult<List<FriendshipDTO>>> GetFriends(Guid userId, int page = 1, int pageSize = 10, bool isCurrentUser = false);
        Task<ServiceResult<RandomFriendsResult>> GetRandomFriends(Guid userId, int count = 4);
        Task<ServiceResult<FriendshipDTO>> CreateFriendShip(Guid initiatorId, Guid friendId);
        Task<ServiceResult<FriendshipDTO>> UpdateFriendShipStatus(Guid userId, Guid friendshipId, FriendshipStatus status);
    }
}
