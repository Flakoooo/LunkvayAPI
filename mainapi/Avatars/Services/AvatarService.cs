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
using System.Text.Json.Nodes;

namespace LunkvayAPI.Avatars.Services
{
    public class AvatarService(
        ILogger<AvatarService> logger,
        LunkvayDBContext lunkvayDBContext,
        HttpClient httpClient,
        IUserService userService
    ) : IAvatarService
    {
        private readonly ILogger<AvatarService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IUserService _userService = userService;

        private const string DEFAULT_IMAGE_PREFIX = "user_avatar_";
        private const string IMGDB_API_KEY = "b2c5445f5bb24a4908ef0067129d6315";
        private const string DEFAULT_IMGDB_UPLOAD_URL = "https://api.imgbb.com/1/upload";
        private const string DEFAULT_IMGDB_UPLOAD_URL_KEY_PARAM = $"?key={IMGDB_API_KEY}";
        private const string DEFAULT_AVATAR_URL = "https://i.ibb.co/67xWgZSb/default-user.webp";

        public async Task<ServiceResult<string>> GetUserImgDBAvatar(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<string>.Failure(ErrorCode.UserIdRequired.GetDescription());

            string? imgDBUrl = await _dbContext.Avatars
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => a.ImgDBUrl)
                .FirstOrDefaultAsync();

            if (imgDBUrl is null)
                return ServiceResult<string>.Success(DEFAULT_AVATAR_URL);

            return ServiceResult<string>.Success(imgDBUrl);
        }

        public async Task<ServiceResult<string>> UploadUserImgDBAvatar(Guid userId, byte[] avatarData)
        {
            if (userId == Guid.Empty)
                return ServiceResult<string>.Failure(ErrorCode.UserIdRequired.GetDescription());

            ServiceResult<UserDTO> userResult = await _userService.GetUserById(userId);
            if (!userResult.IsSuccess || userResult.Result is null || userResult.Result.UserName is null)
                return ServiceResult<string>.Failure(
                    userResult.Error ?? ErrorCode.InternalServerError.GetDescription(),
                    userResult.Error is null ? HttpStatusCode.InternalServerError : userResult.StatusCode
                );

            var user = userResult.Result;

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
                    { new StringContent($"{DEFAULT_IMAGE_PREFIX}{user.Id}"), "name" }
                };

                var response = await _httpClient.PostAsync(uploadUrl, form);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка при загрузке аватара для пользователя {UserId}. Status: {StatusCode}",
                        userId, response.StatusCode);
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
                        var avatar = await _dbContext.Avatars
                            .FirstOrDefaultAsync(a => a.UserId == userId);

                        string? oldDeleteUrl = null;

                        if (avatar == null)
                        {
                            avatar = new Avatar
                            {
                                UserId = userId,
                                ImgDBId = id,
                                ImgDBUrl = url,
                                ImgDBDeleteUrl = deleteUrl,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _dbContext.AddAsync(avatar);
                        }
                        else
                        {
                            oldDeleteUrl = avatar.ImgDBDeleteUrl;

                            avatar.ImgDBId = id;
                            avatar.ImgDBUrl = url;
                            avatar.ImgDBDeleteUrl = deleteUrl;
                            avatar.UpdatedAt = DateTime.UtcNow;
                        }

                        await _dbContext.SaveChangesAsync();

                        if (!string.IsNullOrEmpty(oldDeleteUrl))
                        {
                            try
                            {
                                var deleteResponse = await _httpClient.DeleteAsync(oldDeleteUrl);
                                if (!deleteResponse.IsSuccessStatusCode)
                                    _logger.LogWarning(
                                        "Не удалось удалить старый аватар пользователя {UserId}. Status: {StatusCode}",
                                        userId, deleteResponse.StatusCode
                                    );
                                else
                                    _logger.LogInformation("Старый аватар удален для пользователя {UserId}", userId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Ошибка при удалении старого аватара пользователя {UserId}", userId);
                            }
                        }

                        _logger.LogInformation("Аватар успешно обновлен для пользователя {UserId}", userId);
                        return ServiceResult<string>.Success(url);
                    }
                }

                throw new Exception("Upload failed: " + jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении аватара для пользователя {UserId}", userId);
                return ServiceResult<string>.Failure(
                    "Ошибка при обработке изображения", HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ServiceResult<string>> DeleteUserImgDBAvatar(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<string>.Failure(ErrorCode.UserIdRequired.GetDescription());

            try
            {
                var avatar = await _dbContext.Avatars
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (avatar == null)
                    return ServiceResult<string>.Success(DEFAULT_AVATAR_URL);

                string? deleteUrl = avatar.ImgDBDeleteUrl;

                _dbContext.Avatars.Remove(avatar);
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
                                _logger.LogInformation("Аватар удален из ImgBB для пользователя {UserId}", userId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Не удалось удалить аватар из ImgBB для пользователя {UserId}. Status: {StatusCode}",
                                    userId, deleteResponse.StatusCode
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка при удалении аватара из ImgBB для пользователя {UserId}", userId);
                        }
                    });
                }

                _logger.LogInformation("Аватар удален для пользователя {UserId}", userId);
                return ServiceResult<string>.Success(DEFAULT_AVATAR_URL);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении аватара для пользователя {UserId}", userId);
                return ServiceResult<string>.Failure(
                    "Ошибка при удалении аватара", HttpStatusCode.InternalServerError
                );
            }
        }
    }
}
