using LunkvayAPI.Avatars.Models.Enums;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Avatars.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly string? _avatarsPath;

        private readonly string _defaultUserImageName = "default.jpg";
        private readonly string _configurationBasepathName = "FileStorage:BasePath";
        private readonly string _configurationAvatarsName = "FileStorage:BasePath";

        public AvatarService(
            ILogger<AvatarService> logger, LunkvayDBContext lunkvayDBContext, IConfiguration configuration
        )
        {
            _logger = logger;
            _dbContext = lunkvayDBContext;

            string basePath = configuration[_configurationBasepathName] ??
                throw new FileNotFoundException(AvatarsErrorCode.UserAvatarsPathNotFound.GetDescription());
            string avatars = configuration[_configurationAvatarsName] ??
                throw new FileNotFoundException(AvatarsErrorCode.UserAvatarsPathNotFound.GetDescription());
            _avatarsPath = Path.Combine(basePath, avatars);

            if (string.IsNullOrEmpty(_defaultUserImageName))
            {
                string nameOfDefaultImage = nameof(_defaultUserImageName);
                throw new ArgumentNullException(nameOfDefaultImage);
            }

            Directory.CreateDirectory(_avatarsPath);
        }

        public async Task<ServiceResult<byte[]>> GetUserAvatarByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<byte[]>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrEmpty(_avatarsPath))
            {
                _logger.LogCritical("Путь к аватарам не задан!");
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            string? fileName = await _dbContext.Avatars
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => a.FileName)
                .FirstOrDefaultAsync();

            string filePath = Path.Combine(_avatarsPath, fileName ?? _defaultUserImageName);

            if (File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                    return fileBytes.Length > 0
                        ? ServiceResult<byte[]>.Success(fileBytes)
                        : ServiceResult<byte[]>.Failure("Аватар поврежден", HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка чтения файла {FilePath}", filePath);
                }
            }
            // === Если аватар не найден ===
            _logger.LogWarning("Файл не найден или недоступен: {FilePath}", filePath);

            // Это был поиск дефолтного? Ну что, фатальная ошибка
            if (fileName is null)
            {
                _logger.LogCritical("Дефолтный аватар {DefaultImage} отсутствует!", _defaultUserImageName);
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            // Это был поиск конкретного? Выдать дефолт
            string defaultPath = Path.Combine(_avatarsPath, _defaultUserImageName);
            if (File.Exists(defaultPath))
            {
                try
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(defaultPath);
                    return ServiceResult<byte[]>.Success(fileBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка чтения дефолтного аватара {FilePath}", defaultPath);
                }
            }

            return ServiceResult<byte[]>.Failure("Аватар не найден", HttpStatusCode.NotFound);
        }
    }
}
