using LunkvayFriendsService.Models.DTO;
using LunkvayFriendsService.Models.Utils;

namespace LunkvayFriendsService.Services
{
    public interface IFriendsService
    {
        Task<ServiceResult<IEnumerable<UserListItemDTO>>> GetUserFriends(Guid userId, int page = 1, int pageSize = 10);
        Task<ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)>> GetRandomUserFriends(Guid userId, int count = 4);
    }
}
