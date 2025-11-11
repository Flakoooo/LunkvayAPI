using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Friends.Services
{
    public interface IFriendshipsSystemService
    {
        private const int RANDOM_FRIENDS_COUNT = 6;

        Task<RandomFriendsResult?> GetRandomFriends(Guid userId, int count = RANDOM_FRIENDS_COUNT);
    }
}
