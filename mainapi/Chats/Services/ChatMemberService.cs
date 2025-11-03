using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LunkvayAPI.Chats.Services
{
    public class ChatMemberService(
        ILogger<ChatMemberService> logger,
        LunkvayDBContext lunkvayDBContext,
        IUserService userService,
        IChatMessageSystemService chatMessageService
    ) : IChatMemberService
    {
        private readonly ILogger<ChatMemberService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IChatMessageSystemService _chatMessageService = chatMessageService;
        

        private async Task<ServiceResult<bool>> ValidateMemberUpdateRights(
            Guid initiatorId, Guid chatId,
            ChatMember targetMember, UpdateChatMemberRequest request
        )
        {
            if (initiatorId == targetMember.MemberId)
            {
                if (request.NewRole.HasValue && request.NewRole.Value != targetMember.Role)
                    return ServiceResult<bool>.Failure("Вы не можете изменить свою роль");

                return ServiceResult<bool>.Success(true);
            }

            var initiator = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == chatId
                    && cm.MemberId == initiatorId
                    && !cm.IsDeleted
                );

            if (initiator == null)
                return ServiceResult<bool>.Failure("Недостаточно прав");

            if (initiator.Role == ChatMemberRole.Owner)
                return ServiceResult<bool>.Success(true);

            if (initiator.Role == ChatMemberRole.Administrator)
            {
                if (targetMember.Role == ChatMemberRole.Member)
                    return ServiceResult<bool>.Success(true);

                return ServiceResult<bool>.Failure("Вы не можете изменять администраторов и владельца");
            }

            return ServiceResult<bool>.Failure("Недостаточно прав");
        }

        private async Task<ServiceResult<bool>> ValidateMemberDeleteRights(Guid initiatorId, Guid chatId, ChatMember targetMember)
        {
            if (initiatorId == targetMember.MemberId)
                return ServiceResult<bool>.Success(true);

            var initiator = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == chatId
                    && cm.MemberId == initiatorId
                    && !cm.IsDeleted
                );

            if (initiator == null)
                return ServiceResult<bool>.Failure("Недостаточно прав");

            if (initiator.Role == ChatMemberRole.Owner)
                return ServiceResult<bool>.Success(true);

            if (initiator.Role == ChatMemberRole.Administrator)
            {
                if (targetMember.Role == ChatMemberRole.Member)
                    return ServiceResult<bool>.Success(true);

                return ServiceResult<bool>.Failure("Вы не можете исключать администраторов и владельца");
            }

            return ServiceResult<bool>.Failure("Недостаточно прав");
        }

        private async Task<ServiceResult<bool>> HandleOwnerLeaving(ChatMember ownerMember)
        {
            var newOwner = await _dbContext.ChatMembers
                .Where(cm =>
                    cm.ChatId == ownerMember.ChatId
                    && cm.MemberId != ownerMember.MemberId
                    && !cm.IsDeleted
                    && cm.Role == ChatMemberRole.Administrator)
                .OrderBy(cm => cm.CreatedAt)
                .FirstOrDefaultAsync();

            newOwner ??= await _dbContext.ChatMembers
                .Where(cm =>
                    cm.ChatId == ownerMember.ChatId
                    && cm.MemberId != ownerMember.MemberId
                    && !cm.IsDeleted
                    && cm.Role == ChatMemberRole.Member)
                .OrderBy(cm => cm.CreatedAt)
                .FirstOrDefaultAsync();

            if (newOwner != null)
            {
                newOwner.Role = ChatMemberRole.Owner;
                return ServiceResult<bool>.Success(true);
            }

            ownerMember.Chat!.IsDeleted = true;
            ownerMember.Chat.DeletedAt = DateTime.UtcNow;

            return ServiceResult<bool>.Success(true);
        }

        private async Task<ServiceResult<bool>> ValidateRoleChange(Guid initiatorId, Guid chatId,
            ChatMember targetMember, ChatMemberRole newRole)
        {
            var initiator = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == chatId
                    && cm.MemberId == initiatorId
                    && !cm.IsDeleted
                    && cm.Role == ChatMemberRole.Owner
                );

            if (initiator == null)
                return ServiceResult<bool>.Failure("Только владелец может изменять роли");

            if (targetMember.Role == ChatMemberRole.Owner && newRole != ChatMemberRole.Owner)
                return ServiceResult<bool>.Failure("Нельзя изменить роль владельца");

            return ServiceResult<bool>.Success(true);
        }

        private static ChatMemberDTO MapToDto(ChatMember chatMember) => new()
        {
            Id = chatMember.Id,
            UserId = chatMember.MemberId,
            UserName = chatMember.Member?.UserName != null ? chatMember.Member.UserName : "Удаленный пользователь",
            FirstName = chatMember.Member?.FirstName,
            LastName = chatMember.Member?.LastName,
            IsOnline = chatMember.Member != null && chatMember.Member.IsOnline,
            MemberName = chatMember.MemberName,
            Role = chatMember.Role
        };

        private static string FormatUserName(UserDTO? user)
        {
            if (user == null) return "Пользователь";

            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
                return $"{user.FirstName} {user.LastName}";

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                return user.FirstName;

            if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.LastName;

            return user.UserName ?? "Пользователь";
        }


        public async Task<ServiceResult<List<ChatMemberDTO>>> GetChatMembers(Guid chatId)
        {
            try
            {
                var chatExists = await _dbContext.Chats
                    .AnyAsync(c => c.Id == chatId && !c.IsDeleted);

                if (!chatExists)
                    return ServiceResult<List<ChatMemberDTO>>.Failure("Чат не найден");

                var chatMembers = await _dbContext.ChatMembers
                    .AsNoTracking()
                    .Where(cm => cm.ChatId == chatId && !cm.IsDeleted)
                    .Include(cm => cm.Member)
                    .OrderByDescending(cm => cm.Role)
                    .ThenBy(cm => cm.CreatedAt)
                    .Select(cm => MapToDto(cm))
                    .ToListAsync();

                return ServiceResult<List<ChatMemberDTO>>.Success(chatMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении участников чата {ChatId}", chatId);
                return ServiceResult<List<ChatMemberDTO>>.Failure("Ошибка при получении участников чата");
            }
        }

        public async Task<ServiceResult<ChatMemberDTO>> CreateMember(Guid initiatorId, CreateChatMemberRequest request)
        {
            if (initiatorId == Guid.Empty || request.MemberId == Guid.Empty)
                return ServiceResult<ChatMemberDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var initiatorMembership = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == request.ChatId
                    && cm.MemberId == initiatorId
                    && !cm.IsDeleted
                );

            if (initiatorMembership == null)
                return ServiceResult<ChatMemberDTO>.Failure("Вы не являетесь участником этого чата");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.Id == request.ChatId && !c.IsDeleted);

            if (chat?.Type == ChatType.Personal)
                return ServiceResult<ChatMemberDTO>.Failure("Нельзя добавить пользователя в личный чат");

            ServiceResult<UserDTO> userResult = await _userService.GetUserById(request.MemberId);
            var user = userResult.Result;

            // Проверяем, не является ли пользователь уже участником
            var existingMember = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == request.ChatId
                    && cm.MemberId == request.MemberId
                );

            if (existingMember != null)
            {
                if (!existingMember.IsDeleted)
                    return ServiceResult<ChatMemberDTO>.Failure("Пользователь уже является участником чата");

                existingMember.IsDeleted = false;
                existingMember.DeletedAt = null;
                existingMember.UpdatedAt = DateTime.UtcNow;
                existingMember.Role = ChatMemberRole.Member;

                await _dbContext.SaveChangesAsync();

                await _chatMessageService.CreateSystemChatMessage(
                    request.ChatId, $"{FormatUserName(user)} вернулся в чат", SystemMessageType.UserRejoined
                );

                var restoredMember = await _dbContext.ChatMembers
                    .Include(cm => cm.Member)
                    .FirstOrDefaultAsync(cm => cm.Id == existingMember.Id);

                return ServiceResult<ChatMemberDTO>.Success(MapToDto(restoredMember!));
            }

            var chatMember = new ChatMember
            {
                ChatId = request.ChatId,
                MemberId = request.MemberId,
                Role = ChatMemberRole.Member
            };

            await _dbContext.ChatMembers.AddAsync(chatMember);
            await _dbContext.SaveChangesAsync();

            await _chatMessageService.CreateSystemChatMessage(
                request.ChatId, $"{FormatUserName(user)} присоединился к чату", SystemMessageType.UserJoined
            );

            var newMember = await _dbContext.ChatMembers
                .Include(cm => cm.Member)
                .FirstOrDefaultAsync(cm => cm.Id == chatMember.Id);

            return ServiceResult<ChatMemberDTO>.Success(MapToDto(newMember!));
        }

        public async Task<ServiceResult<ChatMemberDTO>> UpdateMember(Guid initiatorId, UpdateChatMemberRequest request)
        {
            if (initiatorId == Guid.Empty || request.MemberId == Guid.Empty)
                return ServiceResult<ChatMemberDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (request.ChatId == Guid.Empty)
                return ServiceResult<ChatMemberDTO>.Failure("Id чата не может быть пустым");

            var chatMember = await _dbContext.ChatMembers
                .Include(cm => cm.Member)
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == request.ChatId
                    && cm.MemberId == request.MemberId
                    && !cm.IsDeleted
                );

            if (chatMember == null)
                return ServiceResult<ChatMemberDTO>.Failure("Участник не найден");

            var validationResult = await ValidateMemberUpdateRights(initiatorId, request.ChatId, chatMember, request);
            if (!validationResult.IsSuccess)
                return ServiceResult<ChatMemberDTO>.Failure(validationResult.Error!);

            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(request.NewMemberName) && request.NewMemberName != chatMember.MemberName)
            {
                chatMember.MemberName = request.NewMemberName.Trim();
                hasChanges = true;
            }

            if (request.NewRole.HasValue && request.NewRole.Value != chatMember.Role)
            {
                var roleValidation = await ValidateRoleChange(initiatorId, request.ChatId, chatMember, request.NewRole.Value);
                if (!roleValidation.IsSuccess)
                    return ServiceResult<ChatMemberDTO>.Failure(roleValidation.Error!);

                chatMember.Role = request.NewRole.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                chatMember.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return ServiceResult<ChatMemberDTO>.Success(MapToDto(chatMember));
        }

        public async Task<ServiceResult<bool>> DeleteMember(Guid initiatorId, DeleteChatMemberRequest request)
        {
            if (initiatorId == Guid.Empty || request.MemberId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var chatMember = await _dbContext.ChatMembers
                .Include(cm => cm.Chat)
                .FirstOrDefaultAsync(cm =>
                    cm.ChatId == request.ChatId
                    && cm.MemberId == request.MemberId
                    && !cm.IsDeleted
                );

            if (chatMember == null)
                return ServiceResult<bool>.Failure("Участник не найден");

            var validationResult = await ValidateMemberDeleteRights(initiatorId, request.ChatId, chatMember);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Failure(validationResult.Error!);

            if (chatMember.Role == ChatMemberRole.Owner)
            {
                var transferResult = await HandleOwnerLeaving(chatMember);
                if (!transferResult.IsSuccess)
                    return ServiceResult<bool>.Failure(transferResult.Error!);
            }

            chatMember.IsDeleted = true;
            chatMember.MemberName = null;
            chatMember.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            ServiceResult<UserDTO> userResult = await _userService.GetUserById(request.MemberId);
            var user = userResult.Result;

            await _chatMessageService.CreateSystemChatMessage(
                request.ChatId, $"{FormatUserName(user)} покинул чат", SystemMessageType.UserLeft
            );

            return ServiceResult<bool>.Success(true);
        }
    }
}
