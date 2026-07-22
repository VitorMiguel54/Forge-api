using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.DTOs.Backoffice.MuscleGroups;
using Forge.Application.DTOs.Backoffice.Rarities;
using Forge.Application.DTOs.Backoffice.Exercises;
using Forge.Application.DTOs.Exercise;
using Forge.Application.DTOs.Sleep;
using Forge.Application.DTOs.UserProfile;
using Forge.Application.DTOs.Weight;
using Forge.Application.DTOs.Water;
using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Forge.Application.Services;
using Forge.Application.Validators;
using Forge.Application.Validators.Achievements;
using Forge.Application.Validators.Exercise;
using Forge.Application.Validators.Levels;
using Forge.Application.Validators.MuscleGroups;
using Forge.Application.Validators.Rarities;
using Forge.Application.Validators.Sleep;
using Forge.Application.Validators.UserProfile;
using Forge.Application.Validators.Weight;
using Forge.Application.Validators.Water;
using Forge.Application.Validators.Workout;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IWorkoutExerciseService, WorkoutExerciseService>();
        services.AddScoped<IWorkoutSetService, WorkoutSetService>();
        services.AddScoped<IWeightService, WeightService>();
        services.AddScoped<IWaterService, WaterService>();
        services.AddScoped<ISleepService, SleepService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IMobileHomeService, MobileHomeService>();
        services.AddScoped<IMobileHistoryService, MobileHistoryService>();
        services.AddScoped<IMobileWorkoutService, MobileWorkoutService>();
        services.AddScoped<IMobileMuscleGroupService, MobileMuscleGroupService>();
        services.AddScoped<IBackofficeMuscleGroupService, BackofficeMuscleGroupService>();
        services.AddScoped<IBackofficeExerciseService, BackofficeExerciseService>();
        services.AddScoped<IBackofficeAchievementService, BackofficeAchievementService>();
        services.AddScoped<IBackofficeRarityService, BackofficeRarityService>();
        services.AddScoped<IBackofficeLevelDefinitionService, BackofficeLevelDefinitionService>();
        services.AddScoped<ILevelDefinitionService, LevelDefinitionService>();
        services.AddScoped<ILevelProgressionService, LevelProgressionService>();
        services.AddScoped<IXpService, XpService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IValidator<CreateBackofficeMuscleGroupRequest>, CreateBackofficeMuscleGroupRequestValidator>();
        services.AddScoped<IValidator<UpdateBackofficeMuscleGroupRequest>, UpdateBackofficeMuscleGroupRequestValidator>();
        services.AddScoped<IValidator<CreateBackofficeAchievementRequest>, CreateBackofficeAchievementRequestValidator>();
        services.AddScoped<IValidator<CreateBackofficeRarityRequest>, CreateBackofficeRarityRequestValidator>();
        services.AddScoped<IValidator<CreateBackofficeLevelDefinitionRequest>, CreateBackofficeLevelDefinitionRequestValidator>();
        services.AddScoped<IValidator<UpdateBackofficeLevelDefinitionRequest>, UpdateBackofficeLevelDefinitionRequestValidator>();
        services.AddScoped<IValidator<UpdateBackofficeRarityRequest>, UpdateBackofficeRarityRequestValidator>();
        services.AddScoped<IValidator<UpdateBackofficeAchievementRequest>, UpdateBackofficeAchievementRequestValidator>();
        services.AddScoped<IValidator<CreateBackofficeExerciseRequest>, CreateBackofficeExerciseRequestValidator>();
        services.AddScoped<IValidator<UpdateBackofficeExerciseRequest>, UpdateBackofficeExerciseRequestValidator>();
        services.AddScoped<IValidator<CreateExerciseRequest>, CreateExerciseRequestValidator>();
        services.AddScoped<IValidator<CreateSleepRecordRequest>, CreateSleepRecordRequestValidator>();
        services.AddScoped<IValidator<UpdateExerciseRequest>, UpdateExerciseRequestValidator>();
        services.AddScoped<IValidator<CreateUserProfileRequest>, CreateUserProfileRequestValidator>();
        services.AddScoped<IValidator<UpdateUserProfileRequest>, UpdateUserProfileRequestValidator>();
        services.AddScoped<IValidator<CreateWeightRecordRequest>, CreateWeightRecordRequestValidator>();
        services.AddScoped<IValidator<CreateWaterIntakeRequest>, CreateWaterIntakeRequestValidator>();
        services.AddScoped<IValidator<CreateWorkoutExerciseRequest>, CreateWorkoutExerciseRequestValidator>();
        services.AddScoped<IValidator<CreateWorkoutSetRequest>, CreateWorkoutSetRequestValidator>();
        services.AddScoped<IValidator<UpdateWorkoutSetRequest>, UpdateWorkoutSetRequestValidator>();
        services.AddScoped<IValidator<CreateWorkoutRequest>, CreateWorkoutRequestValidator>();
        services.AddScoped<IValidator<UpdateWorkoutRequest>, UpdateWorkoutRequestValidator>();

        return services;
    }
}
