using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Chats.Services
{
    public class ChatImageService : IChatImageService
    {
        private readonly ILogger<ChatImageService> _logger;
        private readonly LunkvayDBContext _dbContext;
        private readonly string? _chatImagesPath;

        private readonly string _dafaultChatImageName = "default.jpg";

        public ChatImageService(
            ILogger<ChatImageService> logger, LunkvayDBContext lunkvayDBContext, IConfiguration configuration
        )
        {
            _logger = logger;
            _dbContext = lunkvayDBContext;

            string basePath = configuration["FileStorage:BasePath"] ??
                throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            string avatars = configuration["FileStorage:ChatImages"] ??
                throw new FileNotFoundException("Путь к изображениям пользователя не указан или отсуствует файл конфигурации");
            _chatImagesPath = Path.Combine(basePath, avatars);

            if (_dafaultChatImageName is null or "")
            {
                string nameOfDefaultImage = nameof(_dafaultChatImageName);
                throw new ArgumentNullException(nameOfDefaultImage);
            }

            Directory.CreateDirectory(_chatImagesPath);
        }

        public async Task<ServiceResult<byte[]>> GetChatImagesById(Guid chatId)
        {
            if (_chatImagesPath is null)
            {
                _logger.LogCritical("Путь к изображениям чата не задан!");
                return ServiceResult<byte[]>.Failure("Ошибка сервера", HttpStatusCode.InternalServerError);
            }

            _logger.LogDebug("Поиск изображения чата для {ChatId}", chatId);
            ChatImage? chatImage = await _dbContext.ChatImages.FirstOrDefaultAsync(a => a.ChatId == chatId);

            string filePath = chatImage is null
                ? Path.Combine(_chatImagesPath, _dafaultChatImageName)
                : Path.Combine(_chatImagesPath, chatImage.FileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Файл не найден: {FilePath}", filePath);

                if (chatImage is not null)
                {
                    string defaultFilePath = Path.Combine(_chatImagesPath, _dafaultChatImageName);
                    if (!File.Exists(defaultFilePath))
                    {
                        _logger.LogCritical("Дефолтное изображение чата {DefaultImage} отсутствует!", _dafaultChatImageName);
                        return ServiceResult<byte[]>.Failure("Ошибка сервера", HttpStatusCode.InternalServerError);
                    }
                    filePath = defaultFilePath;
                }
                else return ServiceResult<byte[]>.Failure("Изображение пользователя не найдено", HttpStatusCode.NotFound);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            if (fileBytes.Length == 0)
            {
                _logger.LogCritical("Изображение чата имеет пустое значение");
                return ServiceResult<byte[]>.Failure("Изображение чата не найдено", HttpStatusCode.NotFound);
            }
            return ServiceResult<byte[]>.Success(fileBytes);
        }
    }
}
