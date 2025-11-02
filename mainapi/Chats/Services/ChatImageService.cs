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
using System.Text.Json.Nodes;

namespace LunkvayAPI.Chats.Services
{
    public class ChatImageService(
        ILogger<ChatImageService> logger,
        LunkvayDBContext lunkvayDBContext,
        HttpClient httpClient,
        IChatSystemService chatService,
        IChatMemberSystemService chatMemberService
    ) : IChatImageService
    {
        private readonly ILogger<ChatImageService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IChatSystemService _chatService = chatService;
        private readonly IChatMemberSystemService _chatMemberService = chatMemberService;

        private const string DEFAULT_IMAGE_PREFIX = "chat_image_";
        private const string IMGDB_API_KEY = "b2c5445f5bb24a4908ef0067129d6315";
        private const string DEFAULT_IMGDB_UPLOAD_URL = "https://api.imgbb.com/1/upload";
        private const string DEFAULT_IMGDB_UPLOAD_URL_KEY_PARAM = $"?key={IMGDB_API_KEY}";
        private const string DEFAULT_CHAT_IMAGE_URL = "https://i.ibb.co/hF66vz3v/default-chat.webp";

        public async Task<ServiceResult<string>> GetChatImgDBImage(Guid chatId)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<string>.Failure("Id чата не может быть пустым");

            string? imgDBUrl = await _dbContext.ChatImages
                .AsNoTracking()
                .Where(ci => ci.ChatId == chatId)
                .Select(ci => ci.ImgDBUrl)
                .FirstOrDefaultAsync();

            if (imgDBUrl is null)
                return ServiceResult<string>.Success(DEFAULT_CHAT_IMAGE_URL);

            return ServiceResult<string>.Success(imgDBUrl);
        }

        public async Task<ServiceResult<string>> UploadChatImgDBImage(Guid userId, Guid chatId, byte[] avatarData)
        {
            if (userId == Guid.Empty)
                return ServiceResult<string>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<string>.Failure("Id чата не может быть пустым");

            ServiceResult<Chat> chat = await _chatService.GetChatBySystem(chatId);
            if (!chat.IsSuccess || (chat.Result is not null && chat.Result.Type == ChatType.Personal))
                return ServiceResult<string>.Failure("Чат не найден");

            bool validation = await _chatMemberService.ExistAnyChatMembersBySystem(cm => 
                cm.ChatId == chatId 
                && cm.MemberId == userId
                && (cm.Role == ChatMemberRole.Owner || cm.Role == ChatMemberRole.Administrator)
            );
            if (!validation)
                return ServiceResult<string>.Failure("Отказано в доступе", HttpStatusCode.Forbidden);

            try
            {
                // конвертация в webp
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

                string uploadUrl = $"{DEFAULT_IMGDB_UPLOAD_URL}{DEFAULT_IMGDB_UPLOAD_URL_KEY_PARAM}";

                var form = new MultipartFormDataContent
                {
                    { new ByteArrayContent(webpData), "image" },
                    { new StringContent($"{DEFAULT_IMAGE_PREFIX}{chatId}"), "name" }
                };

                var response = await _httpClient.PostAsync(uploadUrl, form);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка при загрузке изображения чата {ChatId}. Status: {StatusCode}",
                        chatId, response.StatusCode);
                    return ServiceResult<string>.Failure(
                        "Ошибка при загрузке изображения", response.StatusCode
                    );
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(jsonResponse);

                if (json?["success"]?.GetValue<bool>() == true)
                {
                    string? id = json["data"]?["id"]?.GetValue<string>();
                    string? url = json["data"]?["url"]?.GetValue<string>();
                    string? deleteUrl = json["data"]?["delete_url"]?.GetValue<string>();

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(deleteUrl))
                    {
                        var chatImage = await _dbContext.ChatImages
                            .FirstOrDefaultAsync(ci => ci.ChatId == chatId);

                        string? oldDeleteUrl = null;

                        if (chatImage == null)
                        {
                            chatImage = new ChatImage
                            {
                                ChatId = chatId,
                                ImgDBId = id,
                                ImgDBUrl = url,
                                ImgDBDeleteUrl = deleteUrl,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _dbContext.AddAsync(chatImage);
                        }
                        else
                        {
                            oldDeleteUrl = chatImage.ImgDBDeleteUrl;

                            chatImage.ImgDBId = id;
                            chatImage.ImgDBUrl = url;
                            chatImage.ImgDBDeleteUrl = deleteUrl;
                            chatImage.UpdatedAt = DateTime.UtcNow;
                        }

                        await _dbContext.SaveChangesAsync();

                        if (!string.IsNullOrEmpty(oldDeleteUrl))
                        {
                            try
                            {
                                var deleteResponse = await _httpClient.DeleteAsync(oldDeleteUrl);
                                if (!deleteResponse.IsSuccessStatusCode)
                                    _logger.LogWarning(
                                        "Не удалось удалить старое изображение чата {ChatId}. Status: {StatusCode}",
                                        chatId, deleteResponse.StatusCode
                                    );
                                else
                                    _logger.LogInformation("Старое изображение удалено для чата {ChatId}", chatId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Ошибка при удалении старого изображения чата {ChatId}", chatId);
                            }
                        }

                        _logger.LogInformation("Изображение чата успешно обновлено для чата {ChatId}", chatId);
                        return ServiceResult<string>.Success(url);
                    }
                }

                throw new Exception("Upload failed: " + jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изображения чата {ChatId}", chatId);
                return ServiceResult<string>.Failure(
                    "Ошибка при обработке изображения", HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ServiceResult<string>> DeleteChatImgDBImage(Guid userId, Guid chatId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<string>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<string>.Failure("Id чата не может быть пустым");

            ServiceResult<Chat> chat = await _chatService.GetChatBySystem(chatId);
            if (!chat.IsSuccess || (chat.Result is not null && chat.Result.Type == ChatType.Personal))
                return ServiceResult<string>.Failure("Чат не найден");

            bool validation = await _chatMemberService.ExistAnyChatMembersBySystem(cm =>
                cm.ChatId == chatId
                && cm.MemberId == userId
                && (cm.Role == ChatMemberRole.Owner || cm.Role == ChatMemberRole.Administrator)
            );
            if (!validation)
                return ServiceResult<string>.Failure("Отказано в доступе", HttpStatusCode.Forbidden);

            try
            {
                var chatImage = await _dbContext.ChatImages
                    .FirstOrDefaultAsync(ci => ci.ChatId == chatId);

                if (chatImage == null)
                    return ServiceResult<string>.Success(DEFAULT_CHAT_IMAGE_URL);

                string? deleteUrl = chatImage.ImgDBDeleteUrl;

                _dbContext.ChatImages.Remove(chatImage);
                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(deleteUrl))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var deleteResponse = await _httpClient.DeleteAsync(deleteUrl);
                            if (deleteResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Изображение чата удалено из ImgBB для чата {ChatId}", chatId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Не удалось удалить изображение чата из ImgBB для чата {ChatId}. Status: {StatusCode}",
                                    chatId, deleteResponse.StatusCode
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка при удалении изображении чата из ImgBB для чата {ChatId}", chatId);
                        }
                    });
                }

                _logger.LogInformation("Изображение чата удалено для чата {ChatId}", chatId);
                return ServiceResult<string>.Success(DEFAULT_CHAT_IMAGE_URL);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображении чата для пользователя {ChatId}", chatId);
                return ServiceResult<string>.Failure(
                    "Ошибка при удалении аватара", HttpStatusCode.InternalServerError
                );
            }
        }
    }
}
