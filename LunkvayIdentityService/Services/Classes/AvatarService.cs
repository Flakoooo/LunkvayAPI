using LunkvayIdentityService.Models.Entities;
using LunkvayIdentityService.Models.Utils;
using LunkvayIdentityService.Services.Interfaces;
using LunkvayIdentityService.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayIdentityService.Services.Classes
{
    public class AvatarService : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly string? _avatarsPath;

        private readonly string _defaultUserImageName = "default.jpg";

        public AvatarService(
            ILogger<AvatarService> logger, 
            LunkvayDBContext lunkvayDBContext, 
            IConfiguration configuration
        )
        {
            _logger = logger;
            _dbContext = lunkvayDBContext;

            string basePath = configuration["FileStorage:BasePath"] 
                ?? throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            string avatars = configuration["FileStorage:Avatars"] 
                ?? throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            _avatarsPath = Path.Combine(basePath, avatars);

            if (_defaultUserImageName is null or "")
            {
                string nameOfDefaultImage = nameof(_defaultUserImageName);
                throw new ArgumentNullException(nameOfDefaultImage);
            }

            Directory.CreateDirectory(_avatarsPath);
        }

        public async Task<ServiceResult<byte[]>> GetUserAvatarById(Guid userId)
        {
            if (_avatarsPath is null)
            {
                _logger.LogCritical("Путь к аватарам не задан!");
                return ServiceResult<byte[]>.Failure("Ошибка сервера", HttpStatusCode.InternalServerError);
            }

            _logger.LogDebug("Поиск аватара для {UserId}", userId);
            Avatar? avatar = await _dbContext.Avatars.FirstOrDefaultAsync(a => a.UserId == userId);

            string filePath = avatar is null
                ? Path.Combine(_avatarsPath, _defaultUserImageName) 
                : Path.Combine(_avatarsPath, avatar.FileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Файл не найден: {FilePath}", filePath);

                if (avatar is not null)
                {
                    string defaultFilePath = Path.Combine(_avatarsPath, _defaultUserImageName);
                    if (!File.Exists(defaultFilePath))
                    {
                        _logger.LogCritical("Дефолтный аватар {DefaultImage} отсутствует!", _defaultUserImageName);
                        return ServiceResult<byte[]>.Failure("Ошибка сервера", HttpStatusCode.InternalServerError);
                    }
                    filePath = defaultFilePath;
                }
                else return ServiceResult<byte[]>.Failure("Аватар не найден", HttpStatusCode.NotFound);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            if (fileBytes.Length == 0)
            {
                _logger.LogCritical("Аватар имеет пустое значение");
                return ServiceResult<byte[]>.Failure("Аватар не найден", HttpStatusCode.NotFound);
            }
            return ServiceResult<byte[]>.Success(fileBytes);
        }
    }
}
