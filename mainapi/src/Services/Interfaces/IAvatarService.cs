using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAvatarService
    {
        Task<ServiceResult<byte[]>> GetUserAvatarById(Guid userId);
    }
}
