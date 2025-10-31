using System.ComponentModel;

namespace LunkvayAPI.Avatars.Models.Enums
{
    public enum AvatarsErrorCode
    {
        [Description("Путь к изображениям пользователя не указан или отсуствует файл конфигурации")]
        UserAvatarsPathNotFound,

        [Description("Аватар не найден")]
        AvatarNotFound,

        [Description("Аватар поврежден")]
        AvatarTruncated
    }
}
