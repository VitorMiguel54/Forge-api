namespace Forge.Application.DTOs.Sleep;

public record CreateSleepRecordRequest(
    DateTime SleepDate,
    decimal HoursSlept);
