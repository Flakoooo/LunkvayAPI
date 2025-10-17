using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace LunkvayAPI.Chats.Services
{
    public class ChatService(
        ILogger<ChatService> logger, 
        LunkvayDBContext lunkvayDBContext
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<IEnumerable<ChatDTO>>> GetRooms(Guid userId)
        {
            _logger.LogInformation("({Date}) Запрос списка чатов для {UserId}", DateTime.Now, userId);

            var chatQuery = _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId))
                .Include(c => c.Members)
                    .ThenInclude(m => m.Member)
                .Include(c => c.LastMessage)
                .AsQueryable();

            var chats = await chatQuery.ToListAsync();

            foreach (var chat in chats.Where(c => c.LastMessage != null))
            {
                await _dbContext.Entry(chat.LastMessage!)
                    .Reference(m => m.Sender)
                    .LoadAsync();
            }

            var sortedChats = chats
                .OrderByDescending(c => c.LastMessage != null
                    ? c.LastMessage.CreatedAt
                    : c.UpdatedAt ?? c.CreatedAt)
                .ToList();

            List<ChatDTO> chatDtos = [.. sortedChats.Select(c => new ChatDTO
            {
                Id = c.Id,
                Name = c.Type == ChatType.Personal
                    ? c.Members
                        .Where(m => m.MemberId != userId)
                        .Select(m => m.Member?.FullName)
                        .FirstOrDefault()
                    : c.Name,
                LastMessage = c.LastMessage != null ? new ChatMessageDTO
                {
                    Message = c.LastMessage.Message,
                    CreatedAt = c.LastMessage.CreatedAt,
                    SystemMessageType = c.LastMessage.SystemMessageType,
                    Sender = c.LastMessage.Sender != null ? new UserDTO
                    {
                        Id = c.LastMessage.Sender.Id,
                        UserName = c.LastMessage.Sender.UserName,
                        FirstName = c.LastMessage.Sender.Id != userId
                            ? c.Type != ChatType.Personal
                                ? c.LastMessage.Sender.FirstName
                                : null
                            : "Вы",
                        LastName = c.Type != ChatType.Personal
                            ? !string.IsNullOrEmpty(c.LastMessage.Sender.LastName)
                                ? c.LastMessage.Sender.LastName[0].ToString()
                                : null
                            : null
                    } : null
                } : null
            })];

            _logger.LogInformation("({Date}) Получено {Count} чатов", DateTime.UtcNow, chatDtos.Count);

            return ServiceResult<IEnumerable<ChatDTO>>.Success(chatDtos);
        }

        public async Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId)
        {
            Chat chat = new()
            {
                CreatorId = creatorId, 
                LastMessageId = null, 
                Name = chatRequest.Name,
                Type = chatRequest.Type
            };
            await _dbContext.AddAsync(chat);
            await _dbContext.SaveChangesAsync();

            ChatDTO chatDTO = new()
            {
                Id = chat.Id,
                LastMessage = null,
                Name = chat.Name,
            };

            foreach (UserDTO member in chatRequest.Members)
            {
                if (member.Id is not null)
                {
                    ChatMember chatMember = new()
                    {
                        ChatId = chat.Id,
                        MemberId = (Guid)member.Id,
                        Role = ChatMemberRole.Member
                    };
                    await _dbContext.AddAsync(chatMember);
                }
            }

            await _dbContext.SaveChangesAsync();

            return ServiceResult<ChatDTO>.Success(chatDTO);
        }
    }
}
