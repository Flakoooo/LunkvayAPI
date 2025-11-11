using System.ComponentModel;

namespace LunkvayAPI.Common.Enums.ErrorCodes
{
    public enum ErrorCode
    {
        [Description("Id пользователя не может быть пустым")]
        UserIdRequired,

        [Description("Ошибка сервера")]
        InternalServerError
    }
}
