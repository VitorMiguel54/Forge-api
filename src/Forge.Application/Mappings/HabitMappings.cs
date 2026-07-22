using Forge.Application.DTOs.Sleep;
using Forge.Application.DTOs.Water;
using Forge.Application.DTOs.Weight;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class HabitMappings
{
    public static WeightRecordResponse ToResponse(this WeightRecord weightRecord)
    {
        return new WeightRecordResponse(
            weightRecord.Id,
            weightRecord.UserProfileId,
            weightRecord.Weight,
            weightRecord.RecordDate);
    }

    public static WaterIntakeResponse ToResponse(this WaterIntake waterIntake)
    {
        return new WaterIntakeResponse(
            waterIntake.Id,
            waterIntake.UserProfileId,
            waterIntake.IntakeDate,
            waterIntake.Liters,
            waterIntake.GoalInLiters,
            waterIntake.GoalAchieved);
    }

    public static SleepRecordResponse ToResponse(this SleepRecord sleepRecord)
    {
        return new SleepRecordResponse(
            sleepRecord.Id,
            sleepRecord.UserProfileId,
            sleepRecord.SleepDate,
            sleepRecord.HoursSlept,
            sleepRecord.GoalInHours,
            sleepRecord.GoalAchieved);
    }
}
