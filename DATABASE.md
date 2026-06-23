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

Representa uma sessão de treino registrada pelo usuário.

### Campos

* Id
* UserProfileId
* Name
* WorkoutDate
* Location
* Notes
* TotalVolume
* CreatedAt
* UpdatedAt

### Regras

Um treino só deve contar como concluído se possuir pelo menos:

* Um exercício
* Uma série registrada

### Relacionamentos

Um treino pertence a um usuário.

Um treino possui muitos WorkoutExercises.

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
