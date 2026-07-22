using Forge.Domain.Constants;

namespace Forge.Infrastructure.Seeding;

public static class LevelDefinitionCatalog
{
    public static IReadOnlyCollection<LevelDefinitionSeedDefinition> All { get; } =
    [
        new(OfficialLevelDefinitionIds.Forgotten, "Esquecido", "Guerreiro derrotado, cansado, quebrado e esquecido.", 0, 1, "Incomum"),
        new(OfficialLevelDefinitionIds.Forged, "Forjado", "Iniciou sua reconstrução e voltou a treinar.", 6000, 2, "Comum"),
        new(OfficialLevelDefinitionIds.Guard, "Guarda", "J? demonstra força, disciplina e respeito.", 16000, 3, "Raro"),
        new(OfficialLevelDefinitionIds.Sentinel, "Sentinela", "Guerreiro experiente, imponente e constante.", 30000, 4, "Épico"),
        new(OfficialLevelDefinitionIds.ForgeGuardian, "Guardião da Forja", "Estágio máximo, guerreiro lendário moldado pela disciplina.", 50000, 5, "Lendário")
    ];
}
