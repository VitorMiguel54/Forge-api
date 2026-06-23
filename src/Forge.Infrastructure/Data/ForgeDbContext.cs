using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Data;

public class ForgeDbContext(DbContextOptions<ForgeDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<WeightRecord> WeightRecords => Set<WeightRecord>();
    public DbSet<WaterIntake> WaterIntakes => Set<WaterIntake>();
    public DbSet<SleepRecord> SleepRecords => Set<SleepRecord>();
    public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ForgeDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
