using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAvatarService
    {
        public Task<ServiceResult<byte[]>> GetUserAvatarById(Guid userId);
    }
}
