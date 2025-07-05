using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LunkvayAPI.src.Services
{
    public class AvatarService(ILogger<AvatarService> logger, LunkvayDBContext lunkvayDBContext, IConfiguration configuration) : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly string? _avatarsPath = configuration["AvatarSettings:StoragePath"];

        public async Task<byte[]> GetUserAvatarById(Guid userId)
        {
            _logger.LogInformation("Получение изображения пользователя для {UserId}", userId.ToString());
            var avatar = await _dbContext.Avatars.Where(a => a.UserId == userId).FirstOrDefaultAsync();
            if (avatar == null)
            {
                _logger.LogInformation("[ОШИБКА] Изображение для {UserId} не найдено", userId.ToString());
                throw new Exception("Изображение пользователя не найдено");
            }

            if (_avatarsPath == null)
            {
                _logger.LogInformation("[ОШИБКА] Путь до папки с изображениями пользователя не указан");
                throw new Exception("Серверная ошибка при получении изображения пользователя");
            }
                

            var filePath = Path.Combine(_avatarsPath, avatar.FileName);
            _logger.LogInformation("Полный путь: {Path}", Path.GetFullPath(filePath));
            if (!File.Exists(filePath))
            {
                _logger.LogInformation("[ОШИБКА] Файл изображения для {UserId} не найден", userId.ToString());
                throw new Exception("Файл изображения пользователя не найден");
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogInformation("[ОШИБКА] Файл изображения для {UserId} пуст", userId.ToString());
                throw new Exception("Файл изображения пользователя пуст");
            }

            _logger.LogInformation("Изображение для {UserId} было получено", userId.ToString());
            return fileBytes;
        }
    }
}
