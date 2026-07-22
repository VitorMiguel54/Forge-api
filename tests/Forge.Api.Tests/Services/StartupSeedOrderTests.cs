using System.Runtime.CompilerServices;
using Xunit;

namespace Forge.Api.Tests.Services;

public class StartupSeedOrderTests
{
    [Fact]
    public void Program_SeedsOfficialRaritiesBeforeLevelDefinitions()
    {
        var programPath = Path.GetFullPath(Path.Combine(GetSourceDirectory(), "..", "..", "..", "src", "Forge.Api", "Program.cs"));
        var program = File.ReadAllText(programPath);

        var muscleGroupSeedIndex = program.IndexOf("SeedMuscleGroupsAsync", StringComparison.Ordinal);
        var exerciseSeedIndex = program.IndexOf("SeedExercisesAsync", StringComparison.Ordinal);
        var raritySeedIndex = program.IndexOf("SeedRaritiesAsync", StringComparison.Ordinal);
        var levelSeedIndex = program.IndexOf("SeedLevelDefinitionsAsync", StringComparison.Ordinal);

        Assert.True(muscleGroupSeedIndex >= 0, "Program.cs must seed official muscle groups.");
        Assert.True(exerciseSeedIndex >= 0, "Program.cs must seed official exercises.");
        Assert.True(raritySeedIndex >= 0, "Program.cs must seed official rarities.");
        Assert.True(levelSeedIndex >= 0, "Program.cs must seed official level definitions.");
        Assert.True(muscleGroupSeedIndex < exerciseSeedIndex, "Official muscle groups must be seeded before exercises.");
        Assert.True(raritySeedIndex < levelSeedIndex, "Official rarities must be seeded before level definitions.");
    }

    private static string GetSourceDirectory([CallerFilePath] string sourcePath = "")
    {
        return Path.GetDirectoryName(sourcePath) ?? throw new InvalidOperationException("Could not resolve test source path.");
    }
}
