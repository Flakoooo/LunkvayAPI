using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Enums.ChatEnum;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.ChatAPI.Interfaces;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.src.Services.ChatAPI
{
    public class ChatMemberService(
        LunkvayDBContext lunkvayDBContext,
        IUserService userService,
        IChatNotificationService chatNotificationService
    ) : IChatMemberService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        public async Task<ServiceResult<IEnumerable<ChatMemberDTO>>> GetChatMembers(Guid chatId)
        {
            List<ChatMemberDTO> chatMembers = await _dbContext.ChatMembers
                .Where(cm => cm.ChatId == chatId)
                .Select(cm => new ChatMemberDTO
                {
                    Id = cm.Id,
                    Member = cm.Member != null ? new UserDTO
                    {
                        Id = cm.Member.Id,
                        UserName = cm.Member.UserName,
                        Email = cm.Member.Email,
                        FirstName = cm.Member.FirstName,
                        LastName = cm.Member.LastName,
                        CreatedAt = cm.Member.CreatedAt,
                        IsDeleted = cm.Member.IsDeleted,
                        LastLogin = cm.Member.LastLogin,
                        IsOnline = cm.Member.IsOnline,
                    } : null,
                    MemberName = cm.MemberName,
                    Role = cm.Role,
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<ChatMemberDTO>>.Success(chatMembers);
        }

        public async Task<ServiceResult<ChatMemberDTO>> CreateMember(ChatMemberRequest chatMemberRequest)
        {
            UserDTO member;
            ServiceResult<UserDTO> memberResult = await _userService.GetUserById(chatMemberRequest.MemberId);
            if (memberResult.IsSuccess && memberResult.Result is not null)
                member = memberResult.Result;
            else return ServiceResult<ChatMemberDTO>.Failure(
                memberResult.Error ?? "Непредвиденная ошибка",
                HttpStatusCode.InternalServerError
            );

            ChatMember chatMember = new()
            {
                ChatId = chatMemberRequest.ChatId,
                MemberId = chatMemberRequest.MemberId,
                MemberName = null,
                Role = ChatMemberRole.Member
            };
            await _dbContext.AddAsync(chatMember);
            await _dbContext.SaveChangesAsync();

            if (chatMember.Id != Guid.Empty)
                await _chatNotificationService.UserJoined(
                    chatMemberRequest.ChatId, $"{member.FirstName} {member.LastName}"
                );

            ChatMemberDTO chatMemberDTO = new()
            {
                Id = chatMember.Id,
                Member = member,
                MemberName = chatMember.MemberName,
                Role = chatMember.Role,
            };

            return ServiceResult<ChatMemberDTO>.Success(chatMemberDTO);
        }
    }
}
