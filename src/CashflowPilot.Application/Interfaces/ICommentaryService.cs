using CashflowPilot.Domain.Entities;

namespace CashflowPilot.Application.Interfaces;

public interface ICommentaryService
{
    Task<CommentaryPack> GenerateAsync(int analysisId, string userId, int organizationId);
}
