using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LunkvayAPI.src.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly string? _avatarsPath;

        private readonly string _defaultUserImageName = "default.jpg";

        public AvatarService(ILogger<AvatarService> logger, LunkvayDBContext lunkvayDBContext, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = lunkvayDBContext;

            string basePath = configuration["FileStorage:BasePath"] ??
                throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            string avatars = configuration["FileStorage:Avatars"] ??
                throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            _avatarsPath = Path.Combine(basePath, avatars);

            if (string.IsNullOrEmpty(_defaultUserImageName)) throw new ArgumentNullException(nameof(_defaultUserImageName));

            _ = Directory.CreateDirectory(_avatarsPath);
        }

        public async Task<ServiceResult<byte[]>> GetUserAvatarById(Guid userId)
        {
            if (_avatarsPath == null)
            {
                _logger.LogCritical("Путь к аватарам не задан!");
                return ServiceResult<byte[]>.Failure("Ошибка сервера", 500);
            }

            _logger.LogDebug("Поиск аватара для {UserId}", userId);
            Avatar? avatar = await _dbContext.Avatars.Where(a => a.UserId == userId).FirstOrDefaultAsync();

            string filePath = avatar == null 
                ? Path.Combine(_avatarsPath, _defaultUserImageName) 
                : Path.Combine(_avatarsPath, avatar.FileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Файл не найден: {FilePath}", filePath);

                if (avatar != null)
                {
                    string defaultFilePath = Path.Combine(_avatarsPath, _defaultUserImageName);
                    if (!File.Exists(defaultFilePath))
                    {
                        _logger.LogCritical("Дефолтный аватар {DefaultImage} отсутствует!", _defaultUserImageName);
                        return ServiceResult<byte[]>.Failure("Ошибка сервера", 500);
                    }
                    filePath = defaultFilePath;
                }
                else return ServiceResult<byte[]>.Failure("Аватар не найден", 404);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            if (fileBytes.Length == 0)
            {
                _logger.LogCritical("Аватар имеет пустое значение");
                return ServiceResult<byte[]>.Failure("Аватар не найден", 404);
            }
            return ServiceResult<byte[]>.Success(fileBytes);
        }
    }
}
