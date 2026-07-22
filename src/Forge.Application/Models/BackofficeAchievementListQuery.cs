using Forge.Domain.Enums;

namespace Forge.Application.Models;

public record BackofficeAchievementListQuery(
    string? Search,
    AchievementCategory? Category,
    bool? IsActive,
    bool? IsSecret,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);