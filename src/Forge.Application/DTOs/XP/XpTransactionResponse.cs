using Forge.Domain.Enums;

namespace Forge.Application.DTOs.XP;

public record XpTransactionResponse(
    Guid Id,
    Guid UserProfileId,
    int Amount,
    XpSource Source,
    string Description,
    Guid? ReferenceId,
    DateTime CreatedAt);
