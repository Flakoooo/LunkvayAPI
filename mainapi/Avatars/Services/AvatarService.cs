using LunkvayAPI.Avatars.Models.Enums;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Net;

namespace LunkvayAPI.Avatars.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly IUserService _userService;
        private readonly string? _avatarsPath;

        private const string DEFAULT_IMAGE_EXTENSION = "webp";
        private const string DEFAULT_USER_IMAGE_NAME = $"default.{DEFAULT_IMAGE_EXTENSION}";
        private const string CONFIGURATION_BASE_PATH = "FileStorage:BasePath";
        private const string CONFIGURATION_AVATARS = "FileStorage:Avatars";

        public AvatarService(
            IConfiguration configuration,
            ILogger<AvatarService> logger, 
            LunkvayDBContext lunkvayDBContext, 
            IUserService userService
        )
        {
            string basePath = configuration[CONFIGURATION_BASE_PATH] ??
                throw new FileNotFoundException(AvatarsErrorCode.UserAvatarsPathNotFound.GetDescription());
            string avatars = configuration[CONFIGURATION_AVATARS] ??
                throw new FileNotFoundException(AvatarsErrorCode.UserAvatarsPathNotFound.GetDescription());

            _logger = logger;
            _dbContext = lunkvayDBContext;
            _userService = userService;
            _avatarsPath = Path.Combine(basePath, avatars);

            if (string.IsNullOrEmpty(DEFAULT_USER_IMAGE_NAME))
            {
                string nameOfDefaultImage = nameof(DEFAULT_USER_IMAGE_NAME);
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

            string filePath = Path.Combine(_avatarsPath, fileName ?? DEFAULT_USER_IMAGE_NAME);

            if (File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                    return fileBytes.Length > 0
                        ? ServiceResult<byte[]>.Success(fileBytes)
                        : ServiceResult<byte[]>.Failure(
                            AvatarsErrorCode.AvatarTruncated.GetDescription(), 
                            HttpStatusCode.NotFound
                        );
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
                _logger.LogCritical("Дефолтный аватар {DefaultImage} отсутствует!", DEFAULT_USER_IMAGE_NAME);
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            // Это был поиск конкретного? Выдать дефолт
            string defaultPath = Path.Combine(_avatarsPath, DEFAULT_USER_IMAGE_NAME);
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

            return ServiceResult<byte[]>.Failure(
                AvatarsErrorCode.AvatarNotFound.GetDescription(), 
                HttpStatusCode.NotFound
            );
        }

        public async Task<ServiceResult<byte[]>> SetUserAvatar(Guid userId, byte[] avatarData)
        {
            if (userId == Guid.Empty)
                return ServiceResult<byte[]>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrEmpty(_avatarsPath))
            {
                _logger.LogCritical("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            ServiceResult<UserDTO> userResult = await _userService.GetUserById(userId);
            if (!userResult.IsSuccess || userResult.Result is null || userResult.Result.UserName is null)
                return ServiceResult<byte[]>.Failure(
                    userResult.Error ?? ErrorCode.InternalServerError.GetDescription(),
                    userResult.Error is null ? HttpStatusCode.InternalServerError : userResult.StatusCode
                );

            try
            {
                // конвертация в webp
                byte[] webpData;
                using (var image = Image.Load(avatarData))
                {
                    using var ms = new MemoryStream();
                    image.Save(ms, new WebpEncoder()
                    {
                        Quality = 80, //качество конвертации
                        Method = (WebpEncodingMethod)WebpFileFormatType.Lossy
                    });
                    webpData = ms.ToArray();
                }

                string fileName = $"{userId}.{DEFAULT_IMAGE_EXTENSION}";
                string filePath = Path.Combine(_avatarsPath, fileName);

                // создание файла
                await File.WriteAllBytesAsync(filePath, webpData);

                var existingAvatar = await _dbContext.Avatars.FirstOrDefaultAsync(a => a.UserId == userId);
                if (existingAvatar != null)
                {
                    var oldFilePath = Path.Combine(_avatarsPath, existingAvatar.FileName);
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);

                    existingAvatar.FileName = fileName;
                    existingAvatar.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    await _dbContext.Avatars.AddAsync(
                        new Avatar { FileName = fileName, UserId = userId, UpdatedAt = DateTime.UtcNow }
                    );
                }

                await _dbContext.SaveChangesAsync();

                return ServiceResult<byte[]>.Success(webpData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении аватара для пользователя {UserId}", userId);
                return ServiceResult<byte[]>.Failure(
                    "Ошибка при обработке изображения", HttpStatusCode.InternalServerError
                );
            }
        }
    }
}
