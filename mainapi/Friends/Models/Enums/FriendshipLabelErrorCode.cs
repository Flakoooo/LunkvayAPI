using System.ComponentModel;

namespace LunkvayAPI.Friends.Models.Enums
{
    public enum FriendshipLabelErrorCode
    {
        [Description("Id дружеской метки не может быть пустым")]
        FriendshipLabelIdRequired,

        [Description("Дружеская метка не может быть пустой")]
        FriendshipLabelNameRequired,

        [Description("Дружеская метка не найдена")]
        FriendshipLabelNotFound,

        [Description("Не удалось удалить данные дружеские метки")]
        FriendshipLabelsNotFound
    }
}
