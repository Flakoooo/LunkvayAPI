using LunkvayAPI.src.Models.DTO;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IFriendsService
    {
        public Task<IEnumerable<UserListItemDTO>> GetUserFriends(Guid userId, int page = 1, int pageSize = 10);
        public Task<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)> GetRandomUserFriends(Guid userId, int count = 4);
    }
}
