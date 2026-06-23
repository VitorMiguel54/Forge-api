namespace Forge.Application.DTOs.Sleep;

public record SleepRecordResponse(
    Guid Id,
    Guid UserProfileId,
    DateTime SleepDate,
    decimal HoursSlept,
    decimal GoalInHours,
    bool GoalAchieved);
