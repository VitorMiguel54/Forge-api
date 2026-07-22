using Forge.Application.Interfaces;
using Forge.Infrastructure.Data;
using Forge.Infrastructure.Repositories;
using Forge.Infrastructure.Seeding;
using Forge.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ForgeDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationTransaction, EfApplicationTransaction>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IMuscleGroupRepository, MuscleGroupRepository>();
        services.AddScoped<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
        services.AddScoped<IWorkoutMuscleGroupRepository, WorkoutMuscleGroupRepository>();
        services.AddScoped<IWorkoutSetRepository, WorkoutSetRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IWeightRecordRepository, WeightRecordRepository>();
        services.AddScoped<IWaterIntakeRepository, WaterIntakeRepository>();
        services.AddScoped<ISleepRecordRepository, SleepRecordRepository>();
        services.AddScoped<IXpRepository, XpRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        services.AddScoped<IRarityRepository, RarityRepository>();
        services.AddScoped<ILevelDefinitionRepository, LevelDefinitionRepository>();
        services.AddScoped<IAdminImageStorage, LocalAdminImageStorage>();
        services.AddScoped<AchievementSeeder>();
        services.AddScoped<RaritySeeder>();
        services.AddScoped<LevelDefinitionSeeder>();
        services.AddScoped<MuscleGroupSeeder>();
        services.AddScoped<ExerciseSeeder>();

        return services;
    }
}
