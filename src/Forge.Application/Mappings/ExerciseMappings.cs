using Forge.Application.DTOs.Exercise;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class ExerciseMappings
{
    public static ExerciseResponse ToResponse(this Exercise exercise)
    {
        return new ExerciseResponse(
            exercise.Id,
            exercise.Name,
            exercise.Description,
            exercise.MuscleGroup,
            exercise.IsCustom,
            exercise.UserProfileId,
            exercise.MuscleGroupId,
            exercise.MuscleGroupEntity?.DisplayName);
    }
}
