using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Forge.Application.Services;
using Forge.Application.Validators;
using Forge.Application.Validators.Exercise;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IValidator<CreateExerciseRequest>, CreateExerciseRequestValidator>();
        services.AddScoped<IValidator<UpdateExerciseRequest>, UpdateExerciseRequestValidator>();

        return services;
    }
}
