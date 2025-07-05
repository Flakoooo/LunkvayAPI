namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAvatarService
    {
        public Task<byte[]> GetUserAvatarById(Guid userId);
    }
}
