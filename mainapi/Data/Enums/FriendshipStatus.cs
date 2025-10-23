using System.ComponentModel;
using System.Reflection;

namespace LunkvayAPI.Data.Enums
{
    public enum FriendshipStatus
    {
        [Description("Ожидание")]
        Pending, //Запрос отправлен

        [Description("Подтверждено")]
        Accepted, //Дружба подтверждена

        [Description("Отклонено")]
        Rejected, //Запрос отклонён

        [Description("Отменено")]
        Cancelled //Запрос отменили
    }
}
