namespace Forge.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal InitialWeight { get; set; }
    public decimal CurrentWeight { get; set; }
    public int Level { get; set; }
    public int TotalXp { get; set; }
    public decimal DailyWaterGoalInLiters { get; set; }
    public decimal DailySleepGoalInHours { get; set; }
    public int WeeklyWorkoutGoal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<WeightRecord> WeightRecords { get; set; } = new List<WeightRecord>();
    public ICollection<WaterIntake> WaterIntakes { get; set; } = new List<WaterIntake>();
    public ICollection<SleepRecord> SleepRecords { get; set; } = new List<SleepRecord>();
    public ICollection<XpTransaction> XpTransactions { get; set; } = new List<XpTransaction>();
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
