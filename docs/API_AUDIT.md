# Forge API - Auditoria para Consumo Mobile

Atualizado em: 15/07/2026

## Estado Atual

A Forge API esta organizada em camadas (`Api`, `Application`, `Domain`, `Infrastructure`) e expoe CRUDs de perfil, exercicios, treinos, exercicios do treino, series, peso, agua, sono, XP, conquistas, agregadores mobile e contratos administrativos ja implementados para Grupos Musculares e Exercicios.

Status desta revisao de higiene:

- `dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\consolidation-bin\` passou com 0 avisos e 0 erros nesta revisao.
- `dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\consolidation-test-bin\` passou com 31 testes aprovados, 0 falhas e 0 ignorados nesta revisao.
- Esta versao documenta o estado real apos as estabilizacoes da API e os contratos administrativos ja adicionados para Backoffice.

## Endpoints Existentes

### UserProfile

- `POST /api/user-profiles`
- `GET /api/user-profiles`
- `GET /api/user-profiles/{id}`
- `PUT /api/user-profiles/{id}`
- `DELETE /api/user-profiles/{id}`

Observacoes:

- Ainda nao ha autenticacao; `UserProfile` e usado como identidade funcional enquanto auth fica fora do escopo.
- `GET /api/user-profiles` retorna todos os perfis sem paginacao.
- O perfil guarda `Level` e `TotalXp`, usados pela gamificacao.

### Exercise

- `POST /api/exercises`
- `GET /api/exercises`
- `GET /api/exercises/{id}`
- `PUT /api/exercises/{id}`
- `DELETE /api/exercises/{id}`

Observacoes:

- Suporta exercicios globais (`IsCustom = false`) e customizados por usuario (`IsCustom = true` com `UserProfileId`).
- `GET /api/exercises` ainda nao possui filtros, busca ou paginacao.
- O Mobile usa este endpoint para listar exercicios disponiveis na criacao de treino.

### Workout

- `POST /api/workouts`
- `GET /api/workouts`
- `GET /api/workouts/{id}`
- `PUT /api/workouts/{id}`
- `DELETE /api/workouts/{id}`
- `POST /api/workouts/{id}/start`
- `POST /api/workouts/{id}/finish`
- `POST /api/workouts/{id}/cancel`

Estado atual:

- `Workout.Status` e persistido com `Draft`, `InProgress`, `Completed` e `Cancelled`.
- `StartedAt` e `FinishedAt` sao persistidos.
- `WorkoutResponse` inclui `status`, `startedAt`, `finishedAt` e `durationMinutes`.
- Criacao inicia em `Draft`.
- Inicio define `StartedAt` quando ainda nao existir e muda para `InProgress`.
- Finalizacao define `FinishedAt`, muda para `Completed`, calcula volume, concede XP e avalia conquistas.
- Cancelamento muda para `Cancelled`.
- Finalizacao e transacional e idempotente: treino ja `Completed` retorna o estado atual sem repetir XP/conquistas.

Lacunas:

- `GET /api/workouts` ainda retorna treinos globais sem filtro obrigatorio por usuario.
- Nao ha paginacao no CRUD base.
- `WorkoutSet.Volume` ainda e recalculado no fechamento do treino, nao no create/update da serie.

### WorkoutExercise

- `POST /api/workouts/{workoutId}/exercises`
- `GET /api/workouts/{workoutId}/exercises`
- `DELETE /api/workouts/{workoutId}/exercises/{id}`

Observacoes:

- Valida existencia de treino e exercicio.
- Impede duplicidade do mesmo exercicio no mesmo treino.
- Response ainda e enxuto e nao traz todos os detalhes do exercicio base.

### WorkoutSet

- `POST /api/workout-exercises/{workoutExerciseId}/sets`
- `GET /api/workout-exercises/{workoutExerciseId}/sets`
- `PUT /api/workout-sets/{id}`
- `DELETE /api/workout-sets/{id}`

Observacoes:

- Permite criar, listar, editar e excluir series.
- Ainda nao ha campo de descanso.
- Nao ha endpoint batch para registrar varias series.

### WeightRecord

- `POST /api/user-profiles/{userProfileId}/weight-records`
- `GET /api/user-profiles/{userProfileId}/weight-records`
- `GET /api/user-profiles/{userProfileId}/weight-records/latest`
- `DELETE /api/weight-records/{id}`

Observacoes:

- Criacao atualiza `UserProfile.CurrentWeight`.
- Delete nao recalcula `CurrentWeight`.
- Ainda nao gera XP/conquistas.

### WaterIntake

- `POST /api/user-profiles/{userProfileId}/water-intakes`
- `GET /api/user-profiles/{userProfileId}/water-intakes`
- `GET /api/user-profiles/{userProfileId}/water-intakes/today`
- `DELETE /api/water-intakes/{id}`

Observacoes:

- Registra consumo de agua e calcula `GoalAchieved`.
- Ainda nao gera XP/conquistas.

### SleepRecord

- `POST /api/user-profiles/{userProfileId}/sleep-records`
- `GET /api/user-profiles/{userProfileId}/sleep-records`
- `GET /api/user-profiles/{userProfileId}/sleep-records/latest`
- `DELETE /api/sleep-records/{id}`

Observacoes:

- Registra sono e calcula `GoalAchieved`.
- Ainda nao gera XP/conquistas.

### Dashboard Legado

- `GET /api/dashboard/{userProfileId}`

Observacoes:

- Mantido por compatibilidade.
- Retorna gamificacao real de `UserProfile`.
- O Mobile atual prioriza o agregador mobile `/api/mobile/users/{userProfileId}/home`.

### Agregadores Mobile

- `GET /api/mobile/users/{userProfileId}/home`
- `GET /api/mobile/users/{userProfileId}/workouts`
- `GET /api/mobile/users/{userProfileId}/history?page=1&pageSize=20`

Estado atual:

- Todos filtram por `userProfileId`.
- Home agrega usuario, gamificacao, peso, agua, sono, progresso semanal, treino em andamento e metricas.
- Treinos agrega treino em andamento e treinos disponiveis/salvos.
- Historico agrega resumo e lista paginada de treinos concluidos.
- Treinos e Historico usam `Workout.Status` persistido e duracao real de `StartedAt`/`FinishedAt`.
- Historico possui paginacao e limita `pageSize`.

Lacunas:

- Ainda nao ha agregador mobile especifico para Perfil.
- Ainda nao ha agregador mobile especifico para Conquistas com progresso parcial.
- Ainda nao ha feed dedicado de atividade recente.
- Guardiao, streak e progresso parcial de conquistas ainda nao possuem contrato completo.

### XP e Conquistas

- `GET /api/user-profiles/{userProfileId}/xp`
- `GET /api/user-profiles/{userProfileId}/achievements`
- `GET /api/achievements`

Estado atual:

- `XpService` esta implementado.
- `AchievementService` esta implementado.
- Repositories de XP/conquistas estao implementados.
- Services estao registrados na DI.
- XP e concedido ao finalizar treino.
- Conquistas sao avaliadas automaticamente ao finalizar treino para categorias suportadas.
- Duplicidade e evitada por `Source` + `ReferenceId`.
- Catalogo oficial de conquistas tem seed idempotente com 11 conquistas iniciais.
- Novas conquistas oficiais devem ser adicionadas em `src/Forge.Infrastructure/Seeding/AchievementCatalog.cs`.

Lacunas:

- Eventos de agua, sono, peso, streak e metas semanais ainda nao geram XP/conquistas.
- O endpoint de conquistas do usuario ainda nao retorna progresso parcial por conquista.

## CORS e Pipeline

Policy de desenvolvimento:

- Nome: `ForgeMobileWebDevelopment`
- Origens permitidas: `http://localhost:8082`, `http://localhost:5173` e `http://localhost:5174`
- Permite headers e metodos necessarios para o Forge Mobile Web local e o Forge Backoffice local.
- Aplicada apenas em `Development`.
- Nao usa `AllowAnyOrigin` em producao.

Ordem relevante do pipeline:

1. `UseMiddleware<ProblemDetailsExceptionMiddleware>()`
2. `UseSwagger()` / `UseSwaggerUI()` em desenvolvimento
3. `UseHttpsRedirection()`
4. `UseCors("ForgeMobileWebDevelopment")` em desenvolvimento
5. `MapControllers()`

Observacao: autenticacao/autorizacao ainda esta fora do escopo atual, portanto nao ha `UseAuthentication()`/`UseAuthorization()` no pipeline.

## Banco, Migrations e Seed

DbSets principais:

- `UserProfiles`
- `Exercises`
- `Workouts`
- `WorkoutExercises`
- `WorkoutSets`
- `WeightRecords`
- `WaterIntakes`
- `SleepRecords`
- `XpTransactions`
- `Achievements`
- `UserAchievements`
- `MuscleGroups`
- `WorkoutMuscleGroups`

Migrations existentes:

- `20260623022316_InitialCreate`
- `20260708153000_AddWorkoutSetNotes`
- `20260714132221_AddWorkoutStatus`
- `20260714133601_AddWorkoutExecutionTime`
- `20260715143921_AddMuscleGroups`
- `20260715180304_AddBackofficeExerciseFields`

Seed oficial:

- `AchievementSeeder` roda no startup da API via `SeedAchievementsAsync`.
- O seed e idempotente, compara por `Id` estavel e por `Name`, insere apenas ausentes e preserva registros existentes.
- Quantidade atual do catalogo oficial: 11 conquistas.

## Problemas Corrigidos Desde a Auditoria Anterior

- Warning `NU1903` do `Microsoft.OpenApi` corrigido.
- Middleware global de `ProblemDetails` implementado.
- Projeto de testes `tests/Forge.Api.Tests` criado.
- CRUDs base de `UserProfile`, `Exercise`, `Workout`, `WorkoutExercise`, `WorkoutSet`, `WeightRecord`, `WaterIntake` e `SleepRecord` implementados.
- Agregadores mobile de Home, Treinos e Historico implementados.
- CORS de desenvolvimento configurado para o Forge Mobile Web.
- `Workout.Status` persistido.
- `StartedAt` e `FinishedAt` persistidos.
- Duracao real de treino substituiu estimativas fixas nos agregadores mobile.
- Finalizacao de treino tornou-se transacional e idempotente.
- XP e conquistas foram implementados.
- Catalogo oficial de conquistas passou a ser semeado automaticamente.

## Pendencias Reais

### P0 - Higiene/Confiabilidade

- Confirmar aplicacao de migrations e seed em ambiente limpo sempre que a connection string mudar.
- Remover artefatos temporarios de validacao, como `preflight-options.txt`, quando aparecerem no worktree.
- Revisar fakes de testes com `NotImplementedException` em metodos nao exercitados.

### P1 - Contratos Mobile de Polimento

- Criar agregador mobile de Perfil.
- Criar agregador mobile de Conquistas com resumo, catalogo, desbloqueios e progresso parcial.
- Expor streak real.
- Definir contrato real do Guardiao.
- Criar feed real de atividade recente.
- Expor descanso por serie se fizer parte do fluxo de treino.

### P2 - Produto/Arquitetura

- Adicionar filtros/paginacao nos endpoints CRUD que ainda retornam colecoes completas.
- Definir autenticacao/autorizacao.
- Centralizar politica configuravel de XP por evento.
- Calcular volume da serie em create/update, se a UI precisar desse valor durante a execucao.
- Ampliar testes diretos de `XpService`, `AchievementService` e controllers de gamificacao.

## Riscos Atuais

- Mobile ainda depende de `EXPO_PUBLIC_USER_PROFILE_ID` enquanto nao ha autenticacao.
- CRUDs globais sem paginacao/filtro podem crescer mal.
- Perfil e Conquistas ainda precisam compor multiplos endpoints no cliente.
- Progresso de XP/level no Mobile deve continuar alinhado ao contrato do backend.
- Ambientes novos precisam executar migrations e startup da API para receber o seed de conquistas.

## Conclusao

A API deixou de estar em fase de esqueleto e ja sustenta os fluxos principais do Forge Mobile. Os maiores bloqueios tecnicos anteriores foram resolvidos: status explicito, duracao real, finalizacao transacional/idempotente, XP/conquistas reais e seed oficial. A proxima frente deve ser higiene continua, contratos mobile de polimento e expansao de testes, sem reabrir mocks no Mobile.
