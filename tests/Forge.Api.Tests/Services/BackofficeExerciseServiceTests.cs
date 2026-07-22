using Forge.Application.DTOs.Backoffice.Exercises;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.Exercise;
using Forge.Domain.Entities;
using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;
using Xunit;

namespace Forge.Api.Tests.Services;

public class BackofficeExerciseServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsPagedAdministrativeList()
    {
        var chestId = Guid.NewGuid();
        var backId = Guid.NewGuid();
        var repository = new FakeExerciseRepository(
            CreateExercise(Guid.NewGuid(), "Supino", chestId, isActive: true, displayOrder: 2),
            CreateExercise(Guid.NewGuid(), "Remada", backId, isActive: false, displayOrder: 1),
            CreateExercise(Guid.NewGuid(), "Crucifixo", chestId, isActive: true, displayOrder: 3));
        var service = CreateService(repository, CreateMuscleGroupRepository(chestId, backId));

        var response = await service.GetAsync(
            search: "i",
            muscleGroupId: chestId,
            isActive: true,
            sortBy: "displayOrder",
            sortDirection: "desc",
            page: 1,
            pageSize: 1);

        Assert.Equal(2, response.TotalItems);
        Assert.Equal(2, response.TotalPages);
        var exercise = Assert.Single(response.Items);
        Assert.Equal("Crucifixo", exercise.Name);
    }

    [Fact]
    public async Task CreateAsync_CreatesValidExercise()
    {
        var muscleGroupId = Guid.NewGuid();
        var repository = new FakeExerciseRepository();
        var service = CreateService(repository, CreateMuscleGroupRepository(muscleGroupId));

        var response = await service.CreateAsync(new CreateBackofficeExerciseRequest(
            " Supino Reto ",
            "Exercicio para peito",
            muscleGroupId,
            "Intermediario",
            "Barra",
            IsCustom: false,
            DisplayOrder: 10,
            ImageUrl: null,
            GifUrl: null,
            VideoUrl: null,
            ThumbnailUrl: null));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Supino Reto", response.Name);
        Assert.Equal(muscleGroupId, response.MuscleGroupId);
        Assert.True(response.IsActive);
        Assert.Equal("Intermediario", response.Difficulty);
        Assert.Equal("Barra", response.Equipment);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNameAlreadyExists()
    {
        var muscleGroupId = Guid.NewGuid();
        var service = CreateService(
            new FakeExerciseRepository(CreateExercise(Guid.NewGuid(), "Supino Reto", muscleGroupId)),
            CreateMuscleGroupRepository(muscleGroupId));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBackofficeExerciseRequest(
                "supino reto",
                null,
                muscleGroupId,
                null,
                null,
                false,
                null,
                null,
                null,
                null,
                null)));

        Assert.Equal("Exercise name already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationError_WhenMuscleGroupDoesNotExist()
    {
        var service = CreateService(new FakeExerciseRepository(), CreateMuscleGroupRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBackofficeExerciseRequest(
                "Supino",
                null,
                Guid.NewGuid(),
                null,
                null,
                false,
                null,
                null,
                null,
                null,
                null)));

        Assert.Equal("Muscle group does not exist.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingExercise()
    {
        var oldGroupId = Guid.NewGuid();
        var newGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var service = CreateService(
            new FakeExerciseRepository(CreateExercise(exerciseId, "Supino", oldGroupId)),
            CreateMuscleGroupRepository(oldGroupId, newGroupId));

        var response = await service.UpdateAsync(
            exerciseId,
            new UpdateBackofficeExerciseRequest(
                "Supino Inclinado",
                "Atualizado",
                newGroupId,
                "Avancado",
                "Halteres",
                false,
                20,
                "/image.png",
                "/demo.gif",
                "/video.mp4",
                "/thumb.png"));

        Assert.NotNull(response);
        Assert.Equal("Supino Inclinado", response.Name);
        Assert.Equal(newGroupId, response.MuscleGroupId);
        Assert.Equal("Atualizado", response.Description);
        Assert.Equal("/image.png", response.ImageUrl);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeactivatesExercise()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var service = CreateService(
            new FakeExerciseRepository(CreateExercise(exerciseId, "Supino", muscleGroupId)),
            CreateMuscleGroupRepository(muscleGroupId));

        var response = await service.UpdateStatusAsync(
            exerciseId,
            new UpdateBackofficeExerciseStatusRequest(IsActive: false));

        Assert.NotNull(response);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenExerciseIsLinkedToWorkout()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var service = CreateService(
            new FakeExerciseRepository(
                inUseIds: new HashSet<Guid> { exerciseId },
                CreateExercise(exerciseId, "Supino", muscleGroupId)),
            CreateMuscleGroupRepository(muscleGroupId));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(exerciseId));

        Assert.Equal("Exercise cannot be deleted because it is linked to a workout.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExercise_WhenExerciseIsNotLinkedToWorkout()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var repository = new FakeExerciseRepository(CreateExercise(exerciseId, "Supino", muscleGroupId));
        var service = CreateService(repository, CreateMuscleGroupRepository(muscleGroupId));

        var deleted = await service.DeleteAsync(exerciseId);

        Assert.True(deleted);
        Assert.Empty(repository.Exercises);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenExerciseDoesNotExist()
    {
        var service = CreateService(new FakeExerciseRepository(), CreateMuscleGroupRepository());

        var response = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(response);
    }

    [Fact]
    public async Task UploadMediaAsync_StoresImageUrl()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var repository = new FakeExerciseRepository(CreateExercise(exerciseId, "Supino", muscleGroupId));
        var storage = new FakeAdminImageStorage("/uploads/backoffice/exercises/supino.webp");
        var service = CreateService(repository, CreateMuscleGroupRepository(muscleGroupId), storage);
        await using var stream = new MemoryStream([0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50]);

        var response = await service.UploadMediaAsync(
            exerciseId,
            "image",
            stream,
            "supino.webp",
            "image/webp",
            stream.Length);

        Assert.NotNull(response);
        Assert.Equal(exerciseId, response.ExerciseId);
        Assert.Equal("image", response.MediaType);
        Assert.Equal("/uploads/backoffice/exercises/supino.webp", response.Url);
        Assert.Equal("/uploads/backoffice/exercises/supino.webp", repository.Exercises.Single().ImageUrl);
    }

    [Fact]
    public async Task DeleteMediaAsync_RemovesThumbnailUrl()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var exercise = CreateExercise(exerciseId, "Supino", muscleGroupId);
        exercise.ThumbnailUrl = "/uploads/backoffice/exercises/supino-thumb.webp";
        var repository = new FakeExerciseRepository(exercise);
        var storage = new FakeAdminImageStorage();
        var service = CreateService(repository, CreateMuscleGroupRepository(muscleGroupId), storage);

        var deleted = await service.DeleteMediaAsync(exerciseId, "thumbnail");

        Assert.True(deleted);
        Assert.Null(repository.Exercises.Single().ThumbnailUrl);
        Assert.Contains("exercises/supino-thumb.webp", storage.DeletedKeys);
    }

    [Fact]
    public async Task UploadMediaAsync_RejectsUnsupportedMediaType()
    {
        var muscleGroupId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var service = CreateService(
            new FakeExerciseRepository(CreateExercise(exerciseId, "Supino", muscleGroupId)),
            CreateMuscleGroupRepository(muscleGroupId));
        await using var stream = new MemoryStream([0x47, 0x49, 0x46, 0x38, 0x39, 0x61]);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadMediaAsync(
                exerciseId,
                "audio",
                stream,
                "demo.gif",
                "image/gif",
                stream.Length));

        Assert.Equal("Exercise media type is not supported.", exception.Message);
    }

    private static BackofficeExerciseService CreateService(
        FakeExerciseRepository exerciseRepository,
        FakeMuscleGroupRepository muscleGroupRepository,
        FakeAdminImageStorage? mediaStorage = null)
    {
        return new BackofficeExerciseService(
            exerciseRepository,
            muscleGroupRepository,
            mediaStorage ?? new FakeAdminImageStorage(),
            new CreateBackofficeExerciseRequestValidator(),
            new UpdateBackofficeExerciseRequestValidator());
    }

    private static FakeMuscleGroupRepository CreateMuscleGroupRepository(params Guid[] activeIds)
    {
        return new FakeMuscleGroupRepository(activeIds);
    }

    private static Exercise CreateExercise(
        Guid id,
        string name,
        Guid muscleGroupId,
        bool isActive = true,
        int? displayOrder = null)
    {
        return new Exercise
        {
            Id = id,
            Name = name,
            Description = null,
            MuscleGroup = MuscleGroupEnum.Chest,
            MuscleGroupId = muscleGroupId,
            Difficulty = null,
            Equipment = null,
            IsCustom = false,
            IsActive = isActive,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakeExerciseRepository : IExerciseRepository
    {
        private readonly HashSet<Guid> inUseIds;

        public FakeExerciseRepository(params Exercise[] exercises)
            : this(new HashSet<Guid>(), exercises)
        {
        }

        public FakeExerciseRepository(HashSet<Guid> inUseIds, params Exercise[] exercises)
        {
            this.inUseIds = inUseIds;
            Exercises = exercises.ToList();
        }

        public List<Exercise> Exercises { get; }

        public Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Exercise>>(Exercises.ToArray());
        }

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exercises.FirstOrDefault(exercise => exercise.Id == id));
        }

        public Task<BackofficeExerciseListData> GetBackofficeAsync(
            BackofficeExerciseListQuery query,
            CancellationToken cancellationToken = default)
        {
            var exercises = Exercises.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                exercises = exercises.Where(exercise => exercise.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (query.MuscleGroupId is not null)
            {
                exercises = exercises.Where(exercise => exercise.MuscleGroupId == query.MuscleGroupId);
            }

            if (query.IsActive is not null)
            {
                exercises = exercises.Where(exercise => exercise.IsActive == query.IsActive);
            }

            exercises = query.SortBy?.ToLowerInvariant() == "displayorder"
                ? query.SortDirection?.ToLowerInvariant() == "desc"
                    ? exercises.OrderByDescending(exercise => exercise.DisplayOrder)
                    : exercises.OrderBy(exercise => exercise.DisplayOrder)
                : exercises.OrderBy(exercise => exercise.Name);

            var filtered = exercises.ToArray();
            var items = filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ToBackofficeData)
                .ToArray();

            return Task.FromResult(new BackofficeExerciseListData(items, filtered.Length));
        }

        public Task<BackofficeExerciseData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exercises
                .Where(exercise => exercise.Id == id)
                .Select(ToBackofficeData)
                .FirstOrDefault());
        }

        public Task<bool> NameExistsAsync(
            string name,
            Guid? ignoredId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exercises.Any(exercise =>
                string.Equals(exercise.Name, name, StringComparison.OrdinalIgnoreCase)
                && (ignoredId == null || exercise.Id != ignoredId)));
        }

        public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(inUseIds.Contains(id));
        }

        public Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            Exercises.Add(exercise);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            Exercises.Remove(exercise);
            return Task.CompletedTask;
        }

        private static BackofficeExerciseData ToBackofficeData(Exercise exercise)
        {
            return new BackofficeExerciseData(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.MuscleGroupId,
                exercise.MuscleGroupId is null ? null : "Grupo",
                exercise.Difficulty,
                exercise.Equipment,
                exercise.IsCustom,
                exercise.IsActive,
                exercise.DisplayOrder,
                exercise.ImageUrl,
                exercise.GifUrl,
                exercise.VideoUrl,
                exercise.ThumbnailUrl);
        }
    }

    private sealed class FakeMuscleGroupRepository(params Guid[] activeIds) : IMuscleGroupRepository
    {
        public Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MuscleGroup?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(activeIds.Contains(id));
        }

        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeAdminImageStorage(string publicUrl = "/uploads/backoffice/exercises/media.webp") : IAdminImageStorage
    {
        public List<string> DeletedKeys { get; } = [];

        public Task<StoredFileResult> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new StoredFileResult(
                $"{folder}/media{Path.GetExtension(fileName)}",
                publicUrl,
                fileName,
                contentType,
                stream.Length));
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            DeletedKeys.Add(storageKey);
            return Task.CompletedTask;
        }

        public string? GetStorageKeyFromPublicUrl(string? publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl) || !publicUrl.StartsWith("/uploads/backoffice/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return publicUrl["/uploads/backoffice/".Length..];
        }
    }
}
