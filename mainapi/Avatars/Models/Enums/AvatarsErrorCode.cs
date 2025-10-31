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
        AvatarTruncated,

        [Description("Файл не предоставлен")]
        FileIsNull,

        [Description("Файл слишком большой. Максимальный размер: 5MB")]
        FileLengthLimit,

        [Description("Недопустимый формат файла. Разрешены: JPG, PNG, GIF, BMP")]
        FileFormatInvalid
    }
}
