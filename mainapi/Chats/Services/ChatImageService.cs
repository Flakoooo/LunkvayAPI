using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Net;

namespace LunkvayAPI.Chats.Services
{
    public class ChatImageService : IChatImageService
    {
        private readonly ILogger<ChatImageService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly IChatSystemService _chatService;
        private readonly IChatMemberSystemService _chatMemberService;
        private readonly IAvatarService _avatarService;
        private readonly string? _chatImagesPath;

        private const string DEFAULT_IMAGE_EXTENSION = "webp";
        private const string DEFAULT_CHAT_IMAGE_NAME = $"default.{DEFAULT_IMAGE_EXTENSION}";
        private const string CONFIGURATION_BASE_PATH = "FileStorage:BasePath";
        private const string CONFIGURATION_CHAT_IMAGES = "FileStorage:ChatImages";

        public ChatImageService(
            IConfiguration configuration,
            ILogger<ChatImageService> logger,
            LunkvayDBContext lunkvayDBContext,
            IChatSystemService chatService,
            IChatMemberSystemService chatMemberService,
            IAvatarService avatarService
        )
        {
            string error = "Путь к изображениям чата не указан или отсуствует файл конфигурации";
            string basePath = configuration[CONFIGURATION_BASE_PATH] ??
                throw new FileNotFoundException(error);
            string chatImages = configuration[CONFIGURATION_CHAT_IMAGES] ??
                throw new FileNotFoundException(error);

            _logger = logger;
            _dbContext = lunkvayDBContext;
            _chatService = chatService;
            _chatMemberService = chatMemberService;
            _avatarService = avatarService;
            _chatImagesPath = Path.Combine(basePath, chatImages);

            if (string.IsNullOrEmpty(DEFAULT_CHAT_IMAGE_NAME))
            {
                string nameOfDefaultImage = nameof(DEFAULT_CHAT_IMAGE_NAME);
                throw new ArgumentNullException(nameOfDefaultImage);
            }

            Directory.CreateDirectory(_chatImagesPath);
        }

        private async Task<ServiceResult<byte[]>> GetDefaultChatImage()
        {
            if (string.IsNullOrEmpty(_chatImagesPath))
            {
                _logger.LogCritical("Путь к изображениям чата не задан!");
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            string defaultPath = Path.Combine(_chatImagesPath, DEFAULT_CHAT_IMAGE_NAME);
            if (File.Exists(defaultPath))
            {
                try
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(defaultPath);
                    return ServiceResult<byte[]>.Success(fileBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка чтения дефолтного изображения чата {FilePath}", defaultPath);
                }
            }

            _logger.LogCritical("Дефолтное изображение чата {DefaultImage} отсутствует!", DEFAULT_CHAT_IMAGE_NAME);
            return ServiceResult<byte[]>.Failure(
                ErrorCode.InternalServerError.GetDescription(),
                HttpStatusCode.InternalServerError
            );
        }

        public async Task<ServiceResult<byte[]>> GetChatImageByChatId(Guid userId, Guid chatId)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<byte[]>.Failure("Id чата не может быть пустым");

            if (string.IsNullOrEmpty(_chatImagesPath))
            {
                _logger.LogCritical("Путь к изображениям чата не задан!");
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            ServiceResult<Chat> chatResult =  await _chatService.GetChatBySystem(chatId);
            if (!chatResult.IsSuccess || chatResult.Result is null)
                return ServiceResult<byte[]>.Failure(
                    chatResult.Error ?? ErrorCode.InternalServerError.GetDescription(),
                    chatResult.Error is null ? HttpStatusCode.InternalServerError : chatResult.StatusCode
                );

            var chat = chatResult.Result;

            bool isChatMember = await _chatMemberService.ExistAnyChatMembersBySystem(
                cm => cm.ChatId == chatId && cm.MemberId == userId && !cm.IsDeleted
            );

            if (!isChatMember)
                return ServiceResult<byte[]>.Failure("Вы не являетесь участником этого чата", HttpStatusCode.Forbidden);

            if (chat.Type == ChatType.Personal)
            {
                List<ChatMember> members = await _chatMemberService.GetChatMembersByChatIdBySystem(chatId);
                var member = members.FirstOrDefault(cm => cm.MemberId != userId && !cm.IsDeleted);
                if (member == null)
                    return await GetDefaultChatImage();

                return await _avatarService.GetUserAvatarByUserId(member.MemberId);
            }
            else if (chat.Type == ChatType.Group)
            {
                string? fileName = await _dbContext.ChatImages
                .AsNoTracking()
                .Where(ci => ci.ChatId == chatId)
                .Select(ci => ci.FileName)
                .FirstOrDefaultAsync();

                string filePath = Path.Combine(_chatImagesPath, fileName ?? DEFAULT_CHAT_IMAGE_NAME);

                if (File.Exists(filePath))
                {
                    try
                    {
                        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                        return fileBytes.Length > 0
                            ? ServiceResult<byte[]>.Success(fileBytes)
                            : ServiceResult<byte[]>.Failure(
                                "Изображение вата повреждено",
                                HttpStatusCode.NotFound
                            );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка чтения файла {FilePath}", filePath);
                    }
                }

                return await GetDefaultChatImage();
            }

            return ServiceResult<byte[]>.Failure(
                "Изображение чата не найдено",
                HttpStatusCode.NotFound
            );
        }

        public async Task<ServiceResult<byte[]>> SetChatImage(Guid userId, Guid chatId, byte[] imageData)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<byte[]>.Failure("Id чата не может быть пустым");

            if (string.IsNullOrEmpty(_chatImagesPath))
            {
                _logger.LogCritical("Путь к изображениям чата не указан или отсуствует файл конфигурации");
                return ServiceResult<byte[]>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            // Проверяем чат
            ServiceResult<Chat> chatResult = await _chatService.GetChatBySystem(chatId);
            if (!chatResult.IsSuccess)
                return ServiceResult<byte[]>.Failure("Чат не найден");

            bool validation = await _chatMemberService.ExistAnyChatMembersBySystem(cm =>
                cm.ChatId == chatId && cm.MemberId == userId && cm.Role != ChatMemberRole.Member
            );
            if (!validation)
                return ServiceResult<byte[]>.Failure("Отказано в доступе");

            try
            {
                // Конвертация в webp
                byte[] webpData;
                using (var image = Image.Load(imageData))
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
                string filePath = Path.Combine(_chatImagesPath, fileName);
                string tempFilePath = Path.Combine(_chatImagesPath, $"{chatId}.temp.{DEFAULT_IMAGE_EXTENSION}");

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // пременный файл
                    await File.WriteAllBytesAsync(tempFilePath, webpData);

                    var existingChatImage = await _dbContext.ChatImages
                        .FirstOrDefaultAsync(ci => ci.ChatId == chatId);
                    string? oldFileName = existingChatImage?.FileName;

                    if (existingChatImage != null)
                    {
                        existingChatImage.FileName = fileName;
                        existingChatImage.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        await _dbContext.ChatImages.AddAsync(
                            new ChatImage { FileName = fileName, ChatId = chatId, UpdatedAt = DateTime.UtcNow }
                        );
                    }

                    await _dbContext.SaveChangesAsync();

                    // удаляем старый и на его место перемещаем временный
                    if (File.Exists(filePath)) File.Delete(filePath);

                    File.Move(tempFilePath, filePath);

                    // удаляем старый файл если он каким то образом назван по другому
                    if (oldFileName != null && oldFileName != fileName)
                    {
                        var oldFilePath = Path.Combine(_chatImagesPath, oldFileName);
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
                _logger.LogError(ex, "Ошибка при сохранении изобрадении чата {ChatId}", chatId);
                return ServiceResult<byte[]>.Failure(
                    "Ошибка при обработке изображения", HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ServiceResult<bool>> RemoveChatImage(Guid userId, Guid chatId)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<bool>.Failure("Id чата не может быть пустым");

            if (string.IsNullOrEmpty(_chatImagesPath))
            {
                _logger.LogCritical("Путь к изображениям чата не указан или отсуствует файл конфигурации");
                return ServiceResult<bool>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), HttpStatusCode.InternalServerError
                );
            }

            // Проверяем чат
            ServiceResult<Chat> chatResult = await _chatService.GetChatBySystem(chatId);
            if (!chatResult.IsSuccess)
                return ServiceResult<bool>.Failure("Чат не найден");

            bool validation = await _chatMemberService.ExistAnyChatMembersBySystem(cm =>
                cm.ChatId == chatId && cm.MemberId == userId && cm.Role != ChatMemberRole.Member
            );
            if (!validation)
                return ServiceResult<bool>.Failure("Отказано в доступе");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingChatImage = await _dbContext.ChatImages
                    .FirstOrDefaultAsync(ci => ci.ChatId == chatId);

                if (existingChatImage == null)
                {
                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true);
                }

                string filePath = Path.Combine(_chatImagesPath, existingChatImage.FileName);
                if (File.Exists(filePath)) File.Delete(filePath);

                _dbContext.ChatImages.Remove(existingChatImage);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Изображение чата {ChatId} удалено", chatId);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при удалении изображения чата {ChatId}", chatId);
                return ServiceResult<bool>.Failure(
                    "Ошибка при удалении изображения чата", HttpStatusCode.InternalServerError
                );
            }
        }
    }
}
