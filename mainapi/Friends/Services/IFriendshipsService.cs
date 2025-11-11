using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;

namespace LunkvayAPI.Friends.Services
{
    public interface IFriendshipsService
    {
        private const int START_PAGE = 1;
        private const int PAGE_SIZE = 10;

        Task<ServiceResult<List<FriendshipDTO>>> GetFriends(Guid userId, int page = START_PAGE, int pageSize = PAGE_SIZE, bool isCurrentUser = false);
        Task<ServiceResult<List<FriendshipDTO>>> GetIncomingFriendRequests(Guid userId, int page = START_PAGE, int pageSize = PAGE_SIZE);
        Task<ServiceResult<List<FriendshipDTO>>> GetOutgoingFriendRequests(Guid userId, int page = START_PAGE, int pageSize = PAGE_SIZE);
        Task<ServiceResult<List<UserListItemDTO>>> GetPossibleFriends(Guid userId, int page = START_PAGE, int pageSize = PAGE_SIZE);
        Task<ServiceResult<FriendshipDTO>> CreateFriendShip(Guid initiatorId, Guid friendId);
        Task<ServiceResult<FriendshipDTO>> UpdateFriendShipStatus(Guid userId, Guid friendshipId, FriendshipStatus status);
    }
}
