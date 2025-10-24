using LunkvayAPI.Common.Results;
using LunkvayAPI.Friends.Models.DTO;
using LunkvayAPI.Friends.Models.Requests;

namespace LunkvayAPI.Friends.Services
{
    public interface IFriendshipLabelsService
    {
        Task<ServiceResult<List<FriendshipLabelDTO>>> GetLabels(Guid creatorId);
        Task<ServiceResult<FriendshipLabelDTO>> CreateLabel(Guid creatorId, CreateFriendshipLabelRequest request);
        Task<ServiceResult<bool>> DeleteLabel(Guid creatorId, Guid friendshipLabelId);
        Task<ServiceResult<int>> DeleteSpecificLabel(Guid creatorId, string label);
    }
}
