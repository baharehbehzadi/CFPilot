using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;

namespace CashflowPilot.Application.Interfaces;

public interface ICommentaryGenerator
{
    CommentaryContentDto Generate(VarianceAnalysis analysis, List<VarianceEntry> entries, OrganizationSettings settings);
}
