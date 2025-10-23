using System.ComponentModel;

namespace LunkvayAPI.Data.Enums
{
    public enum ErrorCode
    {
        [Description("Id пользователя не может быть пустым")]
        UserIdRequired,

        [Description("Ошибка сервера")]
        InternalServerError
    }
}
