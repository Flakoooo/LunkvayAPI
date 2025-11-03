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

            // Проверяем пользователя
            ServiceResult<UserDTO> userResult = await _userService.GetUserById(userId);
            if (!userResult.IsSuccess || userResult.Result is null || userResult.Result.UserName is null)
                return ServiceResult<byte[]>.Failure(
                    userResult.Error ?? ErrorCode.InternalServerError.GetDescription(),
                    userResult.Error is null ? HttpStatusCode.InternalServerError : userResult.StatusCode
                );

            try
            {
                // Конвертация в webp
                byte[] webpData;
                using (var image = Image.Load(avatarData))
                {
                    using var ms = new MemoryStream();
                    image.Save(ms, new WebpEncoder()
                    {
                        Quality = 80,
                        Method = (WebpEncodingMethod)WebpFileFormatType.Lossy
                    });
                    webpData = ms.ToArray();
                }

                string fileName = $"{userId}.{DEFAULT_IMAGE_EXTENSION}";
                string filePath = Path.Combine(_avatarsPath, fileName);
                string tempFilePath = Path.Combine(_avatarsPath, $"{userId}.temp.{DEFAULT_IMAGE_EXTENSION}");

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // пременный файл
                    await File.WriteAllBytesAsync(tempFilePath, webpData);

                    var existingAvatar = await _dbContext.Avatars
                        .FirstOrDefaultAsync(a => a.UserId == userId);
                    string? oldFileName = existingAvatar?.FileName;

                    if (existingAvatar != null)
                    {
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

                    // удаляем старый и на его место перемещаем временный
                    if (File.Exists(filePath)) File.Delete(filePath); 

                    File.Move(tempFilePath, filePath);

                    // удаляем старый файл если он каким то образом назван по другому
                    if (oldFileName != null && oldFileName != fileName)
                    {
                        var oldFilePath = Path.Combine(_avatarsPath, oldFileName);
                        if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<byte[]>.Success(webpData);
                }
                catch
                {
                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении аватара для пользователя {UserId}", userId);
                return ServiceResult<byte[]>.Failure(
                    "Ошибка при обработке изображения", HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ServiceResult<bool>> RemoveUserAvatar(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrEmpty(_avatarsPath))
            {
                _logger.LogCritical("Путь к изображениям пользователя не указан или отсутствует файл конфигурации");
                return ServiceResult<bool>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingAvatar = await _dbContext.Avatars
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (existingAvatar == null)
                {
                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true);
                }

                string filePath = Path.Combine(_avatarsPath, existingAvatar.FileName);
                if (File.Exists(filePath)) File.Delete(filePath);

                _dbContext.Avatars.Remove(existingAvatar);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Аватар пользователя {UserId} удален", userId);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при удалении аватара пользователя {UserId}", userId);
                return ServiceResult<bool>.Failure(
                    "Ошибка при удалении аватара", HttpStatusCode.InternalServerError
                );
            }
        }
    }
}
