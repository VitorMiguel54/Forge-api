# DATABASE.md

# Forge - Modelagem Inicial do Banco de Dados

## Visão Geral

Este documento descreve a modelagem inicial do banco de dados do Forge.

O banco deve armazenar informações relacionadas a:

* Usuário
* Exercícios
* Treinos
* Séries
* Peso corporal
* Consumo de água
* Sono
* XP
* Níveis
* Conquistas

## Entidades Principais

## UserProfile

Representa o perfil do usuário dentro da aplicação.

### Campos

* Id
* Name
* Email
* InitialWeight
* CurrentWeight
* Level
* TotalXp
* DailyWaterGoalInLiters
* DailySleepGoalInHours
* WeeklyWorkoutGoal
* CreatedAt
* UpdatedAt

### Relacionamentos

Um usuário pode ter muitos:

* Workouts
* WeightRecords
* WaterIntakes
* SleepRecords
* XpTransactions
* UserAchievements

---

## Exercise

Representa um exercício disponível para ser usado nos treinos.

### Campos

* Id
* Name
* Description
* MuscleGroup
* IsCustom
* UserProfileId
* CreatedAt
* UpdatedAt

### Observações

O sistema pode ter exercícios padrão e exercícios criados pelo próprio usuário.

Quando `IsCustom` for falso, o exercício pode ser considerado global.

Quando `IsCustom` for verdadeiro, o exercício pertence a um usuário.

---

## Workout

Representa tanto uma estrutura salva de treino quanto uma execução registrada.

Uso atual:

* Template salvo: `Status = Draft`, `TemplateWorkoutId = null` e `IsArchived = false`.
* Template removido da biblioteca: `Status = Draft` e `IsArchived = true`.
* Execução ativa: `Status = InProgress`, normalmente com `TemplateWorkoutId` apontando para o template de origem.
* Histórico: `Status = Completed`, preservado para consultas, XP, conquistas, séries e métricas.

### Campos

* Id
* UserProfileId
* Name
* WorkoutDate
* Location
* Notes
* TotalVolume
* Status
* TemplateWorkoutId
* IsArchived
* StartedAt
* FinishedAt
* CreatedAt
* UpdatedAt

### Regras

Um treino só deve contar como concluído se possuir pelo menos:

* Um exercício
* Uma série registrada

Iniciar um template salvo cria uma nova execução `InProgress` com ID próprio e copia a sequência de exercícios do template. O template permanece separado da execução.

Excluir um template salvo não remove fisicamente o registro; a aplicação marca `IsArchived = true` para preservar histórico e impedir início futuro. Treinos `InProgress` e `Completed` não podem ser excluídos pelo endpoint normal.

### Relacionamentos

Um treino pertence a um usuário.

Um treino possui muitos WorkoutExercises.

Uma execução pode apontar para um template por `TemplateWorkoutId`.

Um template pode possuir muitas execuções.

---

## WorkoutExercise

Representa um exercício dentro de um treino.

### Campos

* Id
* WorkoutId
* ExerciseId
* Order
* Notes
* CreatedAt
* UpdatedAt

### Relacionamentos

Um WorkoutExercise pertence a:

* Um Workout
* Um Exercise

Um WorkoutExercise possui muitas WorkoutSets.

---

## WorkoutSet

Representa uma série realizada em um exercício.

### Campos

* Id
* WorkoutExerciseId
* SetNumber
* Repetitions
* Weight
* Volume
* CreatedAt
* UpdatedAt

### Regras

Volume da série:

```txt
Volume = Weight * Repetitions
```

Exemplo:

```txt
20kg * 10 repetições = 200kg
```

---

## WeightRecord

Representa o registro de peso corporal do usuário.

### Campos

* Id
* UserProfileId
* Weight
* RecordDate
* CreatedAt

### Regras

O peso atual do usuário pode ser atualizado a partir do registro mais recente.

---

## WaterIntake

Representa o consumo de água do usuário em um dia.

### Campos

* Id
* UserProfileId
* IntakeDate
* Liters
* GoalInLiters
* GoalAchieved
* CreatedAt
* UpdatedAt

### Regras

`GoalAchieved` deve ser verdadeiro quando:

```txt
Liters >= GoalInLiters
```

---

## SleepRecord

Representa o registro de sono do usuário.

### Campos

* Id
* UserProfileId
* SleepDate
* HoursSlept
* GoalInHours
* GoalAchieved
* CreatedAt
* UpdatedAt

### Regras

`GoalAchieved` deve ser verdadeiro quando:

```txt
HoursSlept >= GoalInHours
```

---

## XpTransaction

Representa um ganho de XP dentro do sistema.

### Campos

* Id
* UserProfileId
* Amount
* Source
* Description
* ReferenceId
* CreatedAt

### Exemplos de Source

* WorkoutCompleted
* SetRegistered
* WaterGoalAchieved
* SleepGoalAchieved
* WeightRegistered
* WeeklyWorkoutGoalAchieved
* AchievementUnlocked

### Observações

Toda alteração de XP deve gerar uma transação.

O `TotalXp` do usuário deve ser calculado ou atualizado a partir das transações.

---

## Achievement

Representa uma conquista disponível no sistema.

### Campos

* Id
* Name
* Description
* Category
* RequiredValue
* IsSecret
* XpReward
* CreatedAt
* UpdatedAt

### Categorias

* Workout
* Hydration
* Sleep
* Progression
* Consistency
* Secret

---

## UserAchievement

Representa uma conquista desbloqueada por um usuário.

### Campos

* Id
* UserProfileId
* AchievementId
* UnlockedAt

### Regras

Um usuário não pode desbloquear a mesma conquista mais de uma vez.

---

## Relacionamentos Gerais

```txt
UserProfile 1:N Workout
Workout 1:N Workout (TemplateWorkoutId)
Workout 1:N WorkoutExercise
Exercise 1:N WorkoutExercise
WorkoutExercise 1:N WorkoutSet

UserProfile 1:N WeightRecord
UserProfile 1:N WaterIntake
UserProfile 1:N SleepRecord
UserProfile 1:N XpTransaction
UserProfile 1:N UserAchievement

Achievement 1:N UserAchievement
```

## Índices Recomendados

### UserProfile

* Email único

### Exercise

* Name
* MuscleGroup

### Workout

* UserProfileId
* WorkoutDate

### WorkoutSet

* WorkoutExerciseId

### WeightRecord

* UserProfileId
* RecordDate

### WaterIntake

* UserProfileId
* IntakeDate

### SleepRecord

* UserProfileId
* SleepDate

### XpTransaction

* UserProfileId
* CreatedAt
* Source

### UserAchievement

* UserProfileId
* AchievementId único em conjunto

---

## Observações Técnicas

* Usar `Guid` como identificador principal das entidades.
* Usar `decimal` para pesos, cargas, litros e horas.
* Usar `DateTime` ou `DateOnly` para datas, conforme suporte e necessidade do projeto.
* Utilizar Entity Framework Core.
* Criar migrations organizadas.
* Evitar lógica de negócio diretamente no DbContext.
* Não retornar entidades diretamente na API.
* Utilizar DTOs para entrada e saída.

---

## Reset local de exercícios e treinos de desenvolvimento

O banco local pode conter treinos, séries e exercícios usados apenas durante o desenvolvimento. Para substituir esses dados por um catálogo oficial, foi criado o script manual:

```txt
docs/sql/reset-development-exercises.sql
```

O script remove somente dados de treino e exercícios atuais, nesta ordem:

* `WorkoutSets`;
* `WorkoutMuscleGroups`;
* `WorkoutExercises`;
* `Workouts`;
* `Exercises`.

Ele preserva `UserProfiles`, `MuscleGroups`, `Rarities`, `Achievements`, `UserAchievements`, `LevelDefinitions`, `XpTransactions` e demais tabelas não relacionadas diretamente ao catálogo de exercícios/treinos.

Execução esperada em SQLCMD mode:

```sql
:setvar Environment "Development"
:r docs/sql/reset-development-exercises.sql
```

Após o reset, iniciar a API em `Development` recria o catálogo oficial de exercícios pelo seed idempotente. Essa limpeza não roda em migrations, não roda no startup da aplicação e não altera a regra normal de exclusão dos endpoints.

## Catálogo oficial de exercícios

O seed oficial de exercícios fica em `src/Forge.Infrastructure/Seeding/ExerciseCatalog.cs` e é aplicado por `ExerciseSeeder`.

Características:

* usa os IDs estáveis dos grupos em `MuscleGroupIds`;
* não cria grupos musculares duplicados;
* gera IDs estáveis de exercícios a partir de chaves oficiais versionadas;
* insere somente exercícios ausentes por ID e por nome;
* não remove exercícios administrativos adicionados depois;
* não sobrescreve alterações administrativas;
* cria os exercícios como ativos, globais e ordenados por `DisplayOrder`;
* mantém o endpoint comum de exclusão protegido por `WorkoutExercises`, retornando `409 Conflict` quando o exercício está em uso.

Quantidade inicial: 150 exercícios oficiais, 10 para cada um dos 15 grupos musculares oficiais.
