using Forge.Api.Controllers;
using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Forge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Forge.Api.Tests.Controllers;

public class MobileExercisesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsExercisesFilteredByMuscleGroupId()
    {
        var chestId = Guid.NewGuid();
        var backId = Guid.NewGuid();
        var controller = new MobileExercisesController(new FakeExerciseService(
            new ExerciseResponse(Guid.NewGuid(), "Supino", null, MuscleGroup.Chest, false, null, chestId, "Peito"),
            new ExerciseResponse(Guid.NewGuid(), "Remada", null, MuscleGroup.Back, false, null, backId, "Costas")));

        var actionResult = await controller.GetAll(chestId, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var exercises = Assert.IsAssignableFrom<IReadOnlyCollection<ExerciseResponse>>(okResult.Value);

        var exercise = Assert.Single(exercises);
        Assert.Equal("Supino", exercise.Name);
        Assert.Equal(chestId, exercise.MuscleGroupId);
    }

    private sealed class FakeExerciseService(params ExerciseResponse[] exercises) : IExerciseService
    {
        public Task<ExerciseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ExerciseResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<ExerciseResponse>>(exercises);
        }

        public Task<ExerciseResponse> CreateAsync(
            CreateExerciseRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ExerciseResponse?> UpdateAsync(
            Guid id,
            UpdateExerciseRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
