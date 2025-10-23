using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;

namespace LunkvayAPI.Friends.Services
{
    public interface IFriendsService
    {
        Task<ServiceResult<List<FriendDTO>>> GetFriends(Guid userId, int page = 1, int pageSize = 10, bool isCurrentUser = false);
        Task<ServiceResult<RandomFriendsResult>> GetRandomFriends(Guid userId, int count = 4);
        Task<ServiceResult<FriendDTO>> CreateFriendShip(Guid initiatorId, Guid friendId);
        Task<ServiceResult<FriendDTO>> UpdateFriendShipStatus(Guid userId, Guid friendshipId, FriendshipStatus status);
    }
}
