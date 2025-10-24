using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;
using LunkvayAPI.Friends.Models.Enums;
using LunkvayAPI.Friends.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Friends.Services
{
    public class FriendshipLabelsService(
        LunkvayDBContext dbContext,
        ILogger<FriendshipLabelsService> logger
    ) : IFriendshipLabelsService
    {
        private readonly LunkvayDBContext _dbContext = dbContext;
        private readonly ILogger<FriendshipLabelsService> _logger = logger;

        public async Task<ServiceResult<List<FriendshipLabelDTO>>> GetLabels(Guid creatorId)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<List<FriendshipLabelDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var labels = await _dbContext.FriendshipLabels
                .AsNoTracking()
                .Where(fl => fl.CreatorId == creatorId)
                .Select(fl => new FriendshipLabelDTO { Id = fl.Id, Label = fl.Label })
                .Distinct()
                .ToListAsync();

            return ServiceResult<List<FriendshipLabelDTO>>.Success(labels);
        }

        public async Task<ServiceResult<FriendshipLabelDTO>> CreateLabel(Guid creatorId, CreateFriendshipLabelRequest request)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<FriendshipLabelDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var friendshipLabel = new FriendshipLabel
            {
                FriendshipId = request.FriendshipId,
                CreatorId = creatorId,
                Label = request.Label
            };

            await _dbContext.FriendshipLabels.AddAsync(friendshipLabel);

            await _dbContext.SaveChangesAsync();

            var friendshipLabelDTO = new FriendshipLabelDTO 
            { 
                Id = friendshipLabel.Id, 
                Label = friendshipLabel.Label 
            };

            return ServiceResult<FriendshipLabelDTO>.Success(friendshipLabelDTO);
        }

        public async Task<ServiceResult<bool>> DeleteLabel(Guid creatorId, Guid friendshipLabelId)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (friendshipLabelId == Guid.Empty)
                return ServiceResult<bool>.Failure(
                    FriendshipLabelErrorCode.FriendshipLabelIdRequired.GetDescription()
                );

            var friendshipLabel = await _dbContext.FriendshipLabels
                .FirstOrDefaultAsync(fl => fl.CreatorId == creatorId && fl.Id == friendshipLabelId);

            if (friendshipLabel is null)
                return ServiceResult<bool>.Failure(
                    FriendshipLabelErrorCode.FriendshipLabelNotFound.GetDescription()
                );

            _dbContext.FriendshipLabels.Remove(friendshipLabel);

            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<int>> DeleteSpecificLabel(Guid creatorId, string label)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<int>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrEmpty(label))
                return ServiceResult<int>.Failure(
                    FriendshipLabelErrorCode.FriendshipLabelNameRequired.GetDescription()
                );

            var specificLabels = await _dbContext.FriendshipLabels
                .Where(fl => fl.CreatorId == creatorId && fl.Label == label)
                .ToListAsync();

            int count = specificLabels.Count;

            if (specificLabels.Count == 0)
                return ServiceResult<int>.Failure(
                    FriendshipLabelErrorCode.FriendshipLabelsNotFound.GetDescription()
                );

            _dbContext.FriendshipLabels.RemoveRange(specificLabels);

            await _dbContext.SaveChangesAsync();

            return ServiceResult<int>.Success(count);
        }
    } 
}
