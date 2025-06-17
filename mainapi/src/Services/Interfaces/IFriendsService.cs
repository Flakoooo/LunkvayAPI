using LunkvayAPI.src.Models.DTO;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IFriendsService
    {
        public Task<IEnumerable<UserDTO>> GetUserFriends(string userId);
    }
}
