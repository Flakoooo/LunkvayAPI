using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IFriendsService
    {
        Task<ServiceResult<IEnumerable<UserListItemDTO>>> GetUserFriends(Guid userId, int page = 1, int pageSize = 10);
        Task<ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)>> GetRandomUserFriends(Guid userId, int count = 4);
    }
}
