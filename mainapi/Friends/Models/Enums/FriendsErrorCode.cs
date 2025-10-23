using System.ComponentModel;

namespace LunkvayAPI.Friends.Models.Enums
{
    public enum FriendsErrorCode
    {
        [Description("Id дружбы не может быть пустым")]
        FriendshipIdRequired,

        [Description("Дружба не найдена")]
        FriendshipNotFound,

        [Description("Нельзя отправить запрос дружбы самому себе")]
        CannotAddSelfAsFriend,

        [Description("Запрос дружбы уже отправлен и ожидает ответа")]
        FriendRequestAlreadyExists,

        [Description("Вы уже друзья с этим пользователем")]
        AlreadyFriends,

        [Description("Заявка отправлена, но возникла ошибка при получении данных пользователя")]
        FriendDataRetrievalFailed,

        [Description("Статус дружбы уже имеет такое же значение")]
        StatusUnchanged,

        [Description("Инициатор может только отменить заявку")]
        InitiatorCanOnlyCancel,

        [Description("Вы можете только принять или отклонить заявку")]
        ReceiverCanOnlyAcceptOrReject,

        [Description("Можно изменять статус только у заявок в ожидании")]
        CanOnlyUpdatePendingRequests,

        [Description("Пользователь заблокирован")]
        UserBlocked,

        [Description("Недостаточно прав для выполнения операции")]
        InsufficientPermissions
    }
}
