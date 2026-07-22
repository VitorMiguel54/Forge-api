using Forge.Application.DTOs.Achievement;
using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Application.DTOs.XP;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.Achievements;
using Forge.Domain.Constants;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Xunit;

namespace Forge.Api.Tests.Services;

public class BackofficeAchievementServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsPagedAdministrativeList_WithFilters()
    {
        var repository = new FakeAchievementRepository(
            CreateAchievement(Guid.NewGuid(), "Primeiro treino", AchievementCategory.Workout, isActive: true, isSecret: false, requiredValue: 1),
            CreateAchievement(Guid.NewGuid(), "Treino secreto", AchievementCategory.Secret, isActive: false, isSecret: true, requiredValue: 1),
            CreateAchievement(Guid.NewGuid(), "Dez treinos", AchievementCategory.Workout, isActive: true, isSecret: false, requiredValue: 10));
        var service = CreateService(repository);

        var response = await service.GetAsync(
            search: "treino",
            category: AchievementCategory.Workout,
            isActive: true,
            isSecret: false,
            sortBy: "requiredValue",
            sortDirection: "desc",
            page: 1,
            pageSize: 1);

        Assert.Equal(2, response.TotalItems);
        Assert.Equal(2, response.TotalPages);
        var achievement = Assert.Single(response.Items);
        Assert.Equal("Dez treinos", achievement.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WhenAchievementExists()
    {
        var id = Guid.NewGuid();
        var repository = new FakeAchievementRepository(CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout));
        repository.UserAchievements.Add(new UserAchievement { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid(), AchievementId = id });
        var service = CreateService(repository);

        var response = await service.GetByIdAsync(id);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(1, response.UnlockedCount);
        Assert.False(response.IsOfficial);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenAchievementDoesNotExist()
    {
        var service = CreateService(new FakeAchievementRepository());

        var response = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(response);
    }

    [Fact]
    public async Task CreateAsync_CreatesValidAchievement()
    {
        var repository = new FakeAchievementRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateBackofficeAchievementRequest(
            " Nova conquista ",
            " Descricao valida ",
            AchievementCategory.Workout,
            RequiredValue: 3,
            XpReward: 100,
            IsSecret: false,
            IsActive: true));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Nova conquista", response.Name);
        Assert.Equal("Descricao valida", response.Description);
        Assert.True(response.IsActive);
        Assert.False(response.IsOfficial);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNameAlreadyExists()
    {
        var repository = new FakeAchievementRepository(CreateAchievement(Guid.NewGuid(), "Primeiro treino", AchievementCategory.Workout));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBackofficeAchievementRequest(
                "primeiro TREINO",
                "Outra descricao",
                AchievementCategory.Workout,
                1,
                50,
                false)));

        Assert.Equal("Achievement name already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationError_WhenCategoryIsInvalid()
    {
        var service = CreateService(new FakeAchievementRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBackofficeAchievementRequest(
                "Conquista",
                "Descricao",
                (AchievementCategory)999,
                1,
                50,
                false)));

        Assert.Contains("Achievement category is invalid.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationError_WhenRequiredValueIsInvalid()
    {
        var service = CreateService(new FakeAchievementRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBackofficeAchievementRequest(
                "Conquista",
                "Descricao",
                AchievementCategory.Workout,
                0,
                50,
                false)));

        Assert.Contains("Achievement required value must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingAchievement()
    {
        var id = Guid.NewGuid();
        var service = CreateService(new FakeAchievementRepository(CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout)));

        var response = await service.UpdateAsync(
            id,
            new UpdateBackofficeAchievementRequest(
                "Primeiro treino atualizado",
                "Descricao atualizada",
                AchievementCategory.Progression,
                1000,
                200,
                true));

        Assert.NotNull(response);
        Assert.Equal("Primeiro treino atualizado", response.Name);
        Assert.Equal(AchievementCategory.Progression, response.Category);
        Assert.Equal(200, response.XpReward);
        Assert.True(response.IsSecret);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflict_WhenNameBelongsToAnotherAchievement()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var service = CreateService(new FakeAchievementRepository(
            CreateAchievement(firstId, "Primeiro treino", AchievementCategory.Workout),
            CreateAchievement(secondId, "Dez treinos", AchievementCategory.Workout)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(
                secondId,
                new UpdateBackofficeAchievementRequest(
                    "Primeiro treino",
                    "Descricao",
                    AchievementCategory.Workout,
                    10,
                    100,
                    false)));

        Assert.Equal("Achievement name already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeactivatesAchievement()
    {
        var id = Guid.NewGuid();
        var service = CreateService(new FakeAchievementRepository(CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout)));

        var response = await service.UpdateStatusAsync(id, new UpdateBackofficeAchievementStatusRequest(IsActive: false));

        Assert.NotNull(response);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task UploadBadgeImageAsync_UpdatesAchievementBadgeUrl()
    {
        var id = Guid.NewGuid();
        var repository = new FakeAchievementRepository(CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout));
        var storage = new FakeAdminImageStorage();
        var service = CreateService(repository, storage);

        await using var stream = CreatePngStream();
        var response = await service.UploadBadgeImageAsync(id, stream, "badge.png", "image/png", stream.Length);

        Assert.NotNull(response);
        Assert.Equal(nameof(Achievement.BadgeImageUrl), response.FieldName);
        Assert.Equal("/uploads/backoffice/achievements/" + id + "/badge/badge.png", response.Url);
        Assert.Equal(response.Url, repository.Achievements.Single().BadgeImageUrl);
        Assert.Equal("achievements/" + id + "/badge/badge.png", repository.Achievements.Single().BadgeImageStorageKey);
    }

    [Fact]
    public async Task UploadBadgeImageAsync_ReplacesPreviousBadgeFile()
    {
        var id = Guid.NewGuid();
        var achievement = CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout);
        achievement.BadgeImageUrl = "/uploads/backoffice/old.png";
        achievement.BadgeImageStorageKey = "achievements/old.png";
        var storage = new FakeAdminImageStorage();
        var service = CreateService(new FakeAchievementRepository(achievement), storage);

        await using var stream = CreatePngStream();
        await service.UploadBadgeImageAsync(id, stream, "badge.png", "image/png", stream.Length);

        Assert.Contains("achievements/old.png", storage.DeletedKeys);
    }

    [Fact]
    public async Task DeleteBadgeImageAsync_ClearsBadgeImageAndDeletesFile()
    {
        var id = Guid.NewGuid();
        var achievement = CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout);
        achievement.BadgeImageUrl = "/uploads/backoffice/old.png";
        achievement.BadgeImageStorageKey = "achievements/old.png";
        var storage = new FakeAdminImageStorage();
        var service = CreateService(new FakeAchievementRepository(achievement), storage);

        var deleted = await service.DeleteBadgeImageAsync(id);

        Assert.True(deleted);
        Assert.Null(achievement.BadgeImageUrl);
        Assert.Null(achievement.BadgeImageStorageKey);
        Assert.Contains("achievements/old.png", storage.DeletedKeys);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAchievement_WhenItHasNoLinksAndIsNotOfficial()
    {
        var id = Guid.NewGuid();
        var repository = new FakeAchievementRepository(CreateAchievement(id, "Administrativa", AchievementCategory.Workout));
        var service = CreateService(repository);

        var deleted = await service.DeleteAsync(id);

        Assert.True(deleted);
        Assert.Empty(repository.Achievements);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenAchievementHasUserAchievements()
    {
        var id = Guid.NewGuid();
        var repository = new FakeAchievementRepository(CreateAchievement(id, "Administrativa", AchievementCategory.Workout));
        repository.UserAchievements.Add(new UserAchievement { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid(), AchievementId = id });
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

        Assert.Equal("Achievement cannot be deleted because it has already been unlocked by users. Deactivate it instead.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenAchievementIsOfficial()
    {
        var id = OfficialAchievementIds.FirstWorkout;
        var service = CreateService(new FakeAchievementRepository(CreateAchievement(id, "Primeiro treino", AchievementCategory.Workout)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

        Assert.Equal("Official achievements cannot be deleted. Deactivate them instead.", exception.Message);
    }

    [Fact]
    public async Task PublicCatalog_ReturnsOnlyActiveAchievements()
    {
        var repository = new FakeAchievementRepository(
            CreateAchievement(Guid.NewGuid(), "Ativa", AchievementCategory.Workout, isActive: true),
            CreateAchievement(Guid.NewGuid(), "Inativa", AchievementCategory.Workout, isActive: false));
        var service = CreatePublicService(repository, completedWorkouts: 0);

        var response = await service.GetAllAsync();

        var achievement = Assert.Single(response);
        Assert.Equal("Ativa", achievement.Name);
    }

    [Fact]
    public async Task EvaluateWorkoutCompletedAsync_IgnoresInactiveAchievements()
    {
        var userProfileId = Guid.NewGuid();
        var inactive = CreateAchievement(Guid.NewGuid(), "Inativa", AchievementCategory.Workout, isActive: false, requiredValue: 1, xpReward: 50);
        var repository = new FakeAchievementRepository(inactive);
        var xpService = new FakeXpService();
        var service = CreatePublicService(repository, completedWorkouts: 1, userProfileId, xpService);

        var response = await service.EvaluateWorkoutCompletedAsync(userProfileId);

        Assert.Empty(response);
        Assert.Empty(repository.UserAchievements);
        Assert.Equal(0, xpService.AwardAchievementUnlockedCount);
    }

    [Fact]
    public async Task EvaluateWorkoutCompletedAsync_UnlocksActiveAchievementAndDoesNotDuplicateXp()
    {
        var userProfileId = Guid.NewGuid();
        var active = CreateAchievement(Guid.NewGuid(), "Ativa", AchievementCategory.Workout, isActive: true, requiredValue: 1, xpReward: 50);
        var repository = new FakeAchievementRepository(active);
        var xpService = new FakeXpService();
        var service = CreatePublicService(repository, completedWorkouts: 1, userProfileId, xpService);

        var firstResponse = await service.EvaluateWorkoutCompletedAsync(userProfileId);
        var secondResponse = await service.EvaluateWorkoutCompletedAsync(userProfileId);

        Assert.Single(firstResponse);
        Assert.Empty(secondResponse);
        Assert.Single(repository.UserAchievements);
        Assert.Equal(1, xpService.AwardAchievementUnlockedCount);
    }

    [Fact]
    public async Task GetUnlockedByUserAsync_ReturnsHistoricalAchievement_EvenWhenInactive()
    {
        var userProfileId = Guid.NewGuid();
        var achievement = CreateAchievement(Guid.NewGuid(), "Historica", AchievementCategory.Workout, isActive: false);
        var repository = new FakeAchievementRepository(achievement);
        repository.UserAchievements.Add(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            AchievementId = achievement.Id,
            Achievement = achievement,
            UnlockedAt = DateTime.UtcNow
        });
        var service = CreatePublicService(repository, completedWorkouts: 0, userProfileId);

        var response = await service.GetUnlockedByUserAsync(userProfileId);

        var unlocked = Assert.Single(response);
        Assert.Equal(achievement.Id, unlocked.AchievementId);
    }

    private static BackofficeAchievementService CreateService(FakeAchievementRepository repository, FakeAdminImageStorage? storage = null)
    {
        return new BackofficeAchievementService(
            repository,
            storage ?? new FakeAdminImageStorage(),
            new CreateBackofficeAchievementRequestValidator(),
            new UpdateBackofficeAchievementRequestValidator());
    }

    private static MemoryStream CreatePngStream()
    {
        return new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]);
    }

    private static AchievementService CreatePublicService(
        FakeAchievementRepository repository,
        int completedWorkouts,
        Guid? existingUserProfileId = null,
        FakeXpService? xpService = null)
    {
        return new AchievementService(
            repository,
            new FakeWorkoutRepository(completedWorkouts),
            new FakeUserProfileRepository(existingUserProfileId),
            xpService ?? new FakeXpService());
    }

    private static Achievement CreateAchievement(
        Guid id,
        string name,
        AchievementCategory category,
        bool isActive = true,
        bool isSecret = false,
        int requiredValue = 1,
        int xpReward = 50)
    {
        var now = DateTime.UtcNow;

        return new Achievement
        {
            Id = id,
            Name = name,
            Description = $"Descricao de {name}",
            Category = category,
            RequiredValue = requiredValue,
            IsSecret = isSecret,
            IsActive = isActive,
            XpReward = xpReward,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private sealed class FakeAchievementRepository(params Achievement[] achievements) : IAchievementRepository
    {
        public List<Achievement> Achievements { get; } = achievements.ToList();
        public List<UserAchievement> UserAchievements { get; } = [];
        public HashSet<Guid> XpTransactionAchievementIds { get; } = [];

        public Task<IReadOnlyCollection<Achievement>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Achievement>>(Achievements.ToArray());
        }

        public Task<IReadOnlyCollection<Achievement>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Achievement>>(Achievements.Where(achievement => achievement.IsActive).ToArray());
        }

        public Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Achievements.FirstOrDefault(achievement => achievement.Id == id));
        }

        public Task<BackofficeAchievementListData> GetBackofficeAsync(
            BackofficeAchievementListQuery query,
            CancellationToken cancellationToken = default)
        {
            var items = Achievements.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                items = items.Where(achievement => achievement.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (query.Category is not null)
            {
                items = items.Where(achievement => achievement.Category == query.Category);
            }

            if (query.IsActive is not null)
            {
                items = items.Where(achievement => achievement.IsActive == query.IsActive);
            }

            if (query.IsSecret is not null)
            {
                items = items.Where(achievement => achievement.IsSecret == query.IsSecret);
            }

            items = query.SortBy?.ToLowerInvariant() == "requiredvalue"
                ? query.SortDirection?.ToLowerInvariant() == "desc"
                    ? items.OrderByDescending(achievement => achievement.RequiredValue)
                    : items.OrderBy(achievement => achievement.RequiredValue)
                : items.OrderBy(achievement => achievement.Name);

            var filtered = items.ToArray();
            var pageItems = filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ToData)
                .ToArray();

            return Task.FromResult(new BackofficeAchievementListData(pageItems, filtered.Length));
        }

        public Task<BackofficeAchievementData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Achievements.Where(achievement => achievement.Id == id).Select(ToData).FirstOrDefault());
        }

        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Achievements.Any(achievement =>
                string.Equals(achievement.Name, name, StringComparison.OrdinalIgnoreCase)
                && (ignoredId == null || achievement.Id != ignoredId)));
        }

        public Task<bool> HasUserAchievementsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserAchievements.Any(userAchievement => userAchievement.AchievementId == id));
        }

        public Task<bool> HasXpTransactionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(XpTransactionAchievementIds.Contains(id));
        }

        public Task<IReadOnlyCollection<UserAchievement>> GetUnlockedByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<UserAchievement>>(
                UserAchievements
                    .Where(userAchievement => userAchievement.UserProfileId == userProfileId)
                    .OrderByDescending(userAchievement => userAchievement.UnlockedAt)
                    .ToArray());
        }

        public Task<bool> IsUnlockedAsync(Guid userProfileId, Guid achievementId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserAchievements.Any(userAchievement =>
                userAchievement.UserProfileId == userProfileId
                && userAchievement.AchievementId == achievementId));
        }

        public Task AddAsync(Achievement achievement, CancellationToken cancellationToken = default)
        {
            Achievements.Add(achievement);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Achievement achievement, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Achievement achievement, CancellationToken cancellationToken = default)
        {
            Achievements.Remove(achievement);
            return Task.CompletedTask;
        }

        public Task AddUnlockedAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default)
        {
            var achievement = Achievements.First(item => item.Id == userAchievement.AchievementId);
            userAchievement.Achievement = achievement;
            UserAchievements.Add(userAchievement);
            return Task.CompletedTask;
        }

        private BackofficeAchievementData ToData(Achievement achievement)
        {
            return new BackofficeAchievementData(
                achievement.Id,
                achievement.Name,
                achievement.Description,
                achievement.Category,
                achievement.RequiredValue,
                achievement.XpReward,
                achievement.IsSecret,
                achievement.IsActive,
                achievement.BadgeImageUrl,
                UserAchievements.Count(userAchievement => userAchievement.AchievementId == achievement.Id),
                achievement.CreatedAt,
                achievement.UpdatedAt);
        }
    }

    private sealed class FakeAdminImageStorage : IAdminImageStorage
    {
        public List<string> DeletedKeys { get; } = [];

        public Task<StoredFileResult> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default)
        {
            var storageKey = folder + "/" + fileName;
            return Task.FromResult(new StoredFileResult(
                storageKey,
                "/uploads/backoffice/" + storageKey,
                fileName,
                contentType,
                stream.Length));
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            DeletedKeys.Add(storageKey);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeWorkoutRepository(int completedWorkouts) : IWorkoutRepository
    {
        public Task<int> CountCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(completedWorkouts);
        }

        public Task<int> CountCompletedByUserProfileSinceAsync(Guid userProfileId, DateTime utcStartDate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(completedWorkouts);
        }

        public Task<decimal> SumCompletedVolumeByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0m);
        }

        public Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetByIdWithExercisesAndSetsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetLatestCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetLatestInProgressByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MobileWorkoutSummaryData>> GetMobileSummariesByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MobileHistoryWorkoutData>> GetMobileHistoryWorkoutsByUserProfileAsync(Guid userProfileId, int skip, int take, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<decimal> SumCompletedVolumeByUserProfileSinceAsync(Guid userProfileId, DateTime utcStartDate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> SumCompletedExerciseCountByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> SumCompletedDurationMinutesByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(Workout workout, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeUserProfileRepository(Guid? existingUserProfileId) : IUserProfileRepository
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(existingUserProfileId is null || existingUserProfileId == id);
        }

        public Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> EmailExistsAsync(string email, Guid? ignoredUserProfileId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeXpService : IXpService
    {
        public int AwardAchievementUnlockedCount { get; private set; }

        public Task<XpTransactionResponse?> AwardAchievementUnlockedAsync(Guid userProfileId, Achievement achievement, CancellationToken cancellationToken = default)
        {
            AwardAchievementUnlockedCount++;
            return Task.FromResult<XpTransactionResponse?>(null);
        }

        public Task<XpSummaryResponse?> GetSummaryByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<XpTransactionResponse?> AwardWorkoutCompletedAsync(Workout workout, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
