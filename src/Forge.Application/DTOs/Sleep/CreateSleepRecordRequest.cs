namespace Forge.Application.DTOs.Sleep;

public record CreateSleepRecordRequest(
    Guid UserProfileId,
    DateTime SleepDate,
    decimal HoursSlept,
    decimal GoalInHours);
