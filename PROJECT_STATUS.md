# PROJECT_STATUS.md

# Forge - Estado Atual do Projeto

Atualizado em: 22/07/2026

## Midias Administrativas de Exercicios - 22/07/2026

Implementado o contrato esperado pelo Forge.Backoffice para upload e remocao de midias de exercicios.

Endpoints:

* `POST /api/backoffice/exercises/{id}/media/{mediaType}`
* `DELETE /api/backoffice/exercises/{id}/media/{mediaType}`
* `mediaType` aceito: `image`, `thumbnail`, `gif`, `video`.
* Upload usa `multipart/form-data` com campo `file`.
* Retorno do upload: `exerciseId`, `mediaType`, `url`, `contentType`, `fileSize` e `updatedAt`.

Persistencia e storage:

* As URLs publicas sao persistidas nos campos ja existentes de `Exercise`: `ImageUrl`, `ThumbnailUrl`, `GifUrl` e `VideoUrl`.
* Nao foi criada migration porque o modelo ja possuia os campos necessarios.
* A API reutiliza a abstracao existente `IAdminImageStorage` e a implementacao atual `LocalAdminImageStorage`.
* A remocao/substituicao deriva a chave de storage a partir da URL publica quando a URL pertence ao storage local configurado.
* Produção futura: substituir `IAdminImageStorage` por provedor externo, como R2/Azure Blob/S3, e manter o mesmo contrato HTTP.

Validacoes:

* Imagem principal: PNG, JPG/JPEG e WebP ate 5 MB.
* Thumbnail: PNG, JPG/JPEG e WebP ate 2 MB.
* GIF: GIF ate 15 MB.
* Video: MP4 ou WebM ate 50 MB.
* Validacao inclui extensao, content-type e assinatura basica do arquivo.

Arquivos principais:

* `BackofficeExercisesController`
* `BackofficeExerciseService`
* `IBackofficeExerciseService`
* `BackofficeExerciseMediaUploadResponse`
* `AdminExerciseMediaUploadValidator`
* `IAdminImageStorage`
* `LocalAdminImageStorage`

Observacao:

* O contrato foi implementado sem adicionar Cloudflare R2, Azure Blob ou outro provedor externo, pois ja havia abstracao de storage local configurada na API.

## Ordenacao Persistente de Treinos Salvos - 20/07/2026

Implementada e validada a ordenacao manual persistente dos templates de treino salvos.

Modelo e banco:

* `Workout` possui `DisplayOrder` persistido.
* Migration criada: `20260717143000_AddWorkoutDisplayOrder`.
* A migration preenche treinos existentes com ordem sequencial estavel por usuario usando `ROW_NUMBER()` ordenado por `CreatedAt` e `Id`.
* Indice criado: `IX_Workouts_UserProfileId_DisplayOrder`.

Regras:

* Novos treinos/templates recebem o proximo `DisplayOrder` disponivel para o usuario.
* Listagens de treinos salvos usam `ORDER BY DisplayOrder ASC`.
* Reordenacao manual normaliza a ordem para `1..N`.
* A operacao valida usuario, IDs duplicados, existencia dos treinos e se todos os treinos salvos do usuario foram enviados.
* Persistencia ocorre em uma unica transacao via `IApplicationTransaction`.

Endpoint:

* `PUT /api/workouts/reorder`
* Body: `userProfileId` e `items[]` com `workoutId` e `displayOrder`.
* Retorna `204 No Content` em sucesso.
* Usa `400` para entrada invalida e `409` para conflitos de dominio conforme middleware global.

Compatibilidade:

* A regra de historico foi preservada.
* Templates arquivados, execucoes em andamento e historico concluido continuam seguindo as regras existentes.
* Nenhuma exclusao em cascata foi adicionada.

Validacao recente:

```txt
dotnet build Forge.slnx -p:UseSharedCompilation=false
dotnet test Forge.slnx -p:UseSharedCompilation=false
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 115 aprovados, 0 falhas e 0 ignorados.
* Validacao local do endpoint criou um terceiro treino, confirmou que entrou no fim da lista e moveu o terceiro para a primeira posicao via `PUT /api/workouts/reorder`; o GET seguinte retornou `DisplayOrder` 1, 2 e 3.

## Templates de Treino e Preservacao de Historico - 17/07/2026

Implementada separacao segura entre treino salvo/template e execucao de treino, preservando o historico concluido.

Auditoria:

* Antes desta alteracao, `Workout` representava tanto template salvo quanto execucao/historico, diferenciado principalmente por `Status`.
* `ActiveWorkout` nos agregadores mobile era o `Workout` mais recente com `Status = InProgress`.
* O Historico usa `Workout` com `Status = Completed`.
* Dependencias diretas de `Workout`: `WorkoutExercises`, `WorkoutSets` via `WorkoutExercises`, `WorkoutMuscleGroups` e referencias logicas de XP em `XpTransactions.ReferenceId`.
* `DELETE /api/workouts/{id}` existia e fazia exclusao fisica, o que era arriscado caso fosse chamado sobre treino com historico.
* `PUT /api/workouts/{id}` existia e editava a entidade, entao passou a bloquear treinos arquivados, em andamento e concluidos.

Modelagem adicionada:

* `Workouts.TemplateWorkoutId` nullable aponta de uma execucao para o template salvo que a originou.
* `Workouts.IsArchived` marca templates removidos da biblioteca sem exclusao fisica.
* Relacionamento self-reference `Workout.TemplateWorkout` -> `Workout.Executions` usa `DeleteBehavior.Restrict`.
* Migration criada: `20260717120000_AddWorkoutTemplateArchive`.

Regras implementadas:

* `POST /api/workouts/{id}/start` em um template `Draft` cria uma nova execucao `InProgress`, copia os exercicios do template e retorna o ID da execucao.
* Iniciar um treino ja `InProgress` permanece idempotente e retorna o proprio treino.
* Templates arquivados, cancelados e concluidos nao podem ser iniciados.
* `DELETE /api/workouts/{id}` nao apaga fisicamente templates: marca `IsArchived = true`.
* `DELETE /api/workouts/{id}` retorna conflito para treinos `InProgress` e para historico `Completed`.
* `PUT /api/workouts/{id}` e os endpoints de vinculo de exercicios bloqueiam edicao de treino arquivado, em andamento ou concluido.
* A listagem mobile de treinos retorna apenas `Draft` nao arquivados e o treino `InProgress` ativo; concluidos seguem exclusivamente no Historico.

Endpoints preservados/reutilizados:

* `PUT /api/workouts/{id}`;
* `DELETE /api/workouts/{id}`;
* `POST /api/workouts/{id}/start`;
* `POST /api/workouts/{id}/finish`;
* `GET /api/mobile/users/{userProfileId}/workouts`;
* `GET /api/mobile/users/{userProfileId}/history`;
* `POST /api/workouts/{workoutId}/exercises`;
* `DELETE /api/workouts/{workoutId}/exercises/{id}`.

Comportamento de conflito:

* `InvalidOperationException` continua sendo convertido pelo middleware global para `409 Conflict` com `ProblemDetails`.
* Exclusao/edicao normal de historico concluido e treino em andamento permanece bloqueada.
* Historico, series, cargas, XP e conquistas nao sao apagados pela exclusao de template.

Testes adicionados/ajustados:

* inicio de template cria execucao separada e preserva o template;
* exclusao de template arquiva sem chamar exclusao fisica;
* exclusao de treino em andamento e historico concluido gera conflito;
* edicao de template arquivado gera conflito;
* agregador mobile nao retorna concluido em "salvos";
* fakes de `IWorkoutRepository` atualizados para o novo metodo `GetByIdWithExercisesAsync`.

Validacao:

```txt
dotnet build Forge.slnx
dotnet test Forge.slnx
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 110 aprovados, 0 falhas e 0 ignorados.

## Catálogo Oficial de Exercícios e Reset Local - 17/07/2026

Criado fluxo controlado para limpar dados locais de treino em desenvolvimento e substituir exercícios antigos por catálogo oficial.

Auditoria:

* `Exercises` é referenciada por `WorkoutExercises` com `DeleteBehavior.Restrict`.
* `WorkoutExercises` é referenciada por `WorkoutSets` com cascade a partir do vínculo, mas o reset manual remove explicitamente na ordem segura.
* `Workouts` possui `WorkoutExercises` e `WorkoutMuscleGroups`.
* `WorkoutMuscleGroups` aponta para `MuscleGroups` com restrict.
* `XpTransactions.ReferenceId` pode apontar logicamente para treinos concluídos, mas não possui FK e foi preservado conforme regra de manter XP.
* `UserProfiles`, `MuscleGroups`, `Rarities`, `Achievements`, `LevelDefinitions` e catálogos não relacionados são preservados.

Script criado:

* `docs/sql/reset-development-exercises.sql`.
* Exige SQLCMD variable `Environment=Development`.
* Remove `WorkoutSets`, `WorkoutMuscleGroups`, `WorkoutExercises`, `Workouts` e `Exercises`.
* Não é executado por startup, seed ou migration.

Seed oficial:

* `ExerciseCatalog` adiciona 150 exercícios oficiais.
* São 10 exercícios para cada grupo muscular oficial existente: Peito, Costas, Ombro, Bíceps, Tríceps, Antebraço, Abdômen, Lombar, Glúteo, Quadríceps, Posterior, Panturrilha, Corpo inteiro, Cardio e Outros.
* O seed usa IDs estáveis dos grupos (`MuscleGroupIds`) e IDs estáveis de exercícios derivados de chaves oficiais versionadas.
* O seed é idempotente: insere ausentes, não duplica por ID/nome, não apaga e não sobrescreve exercícios administrativos.

Comportamento preservado:

* Exclusão normal de exercício sem vínculo continua permitida.
* Exercício vinculado a `WorkoutExercise` continua retornando `409 Conflict`.
* Nenhum endpoint administrativo comum passou a fazer exclusão em cascata arbitrária.

Catálogo disponível:

* Listagens públicas/mobile de exercícios retornam somente `IsActive = true`.
* A ordenação do catálogo usa ordem do grupo muscular, `Exercise.DisplayOrder` e nome.
* Backoffice continua usando a listagem administrativa própria para consultar ativos e inativos.

## Upload Administrativo de Imagens de Conquistas e Niveis - 16/07/2026

Implementado o fluxo real de upload local para imagens administrativas usadas pelo Forge Backoffice.

Escopo entregue:

* Conquistas passam a possuir `BadgeImageUrl` e `BadgeImageStorageKey`.
* Niveis passam a possuir `GuardianImageStorageKey`; `GuardianImageUrl` ja existia e agora e atualizado por endpoint proprio.
* O CRUD administrativo comum continua sem aceitar URL digitada manualmente.
* Upload/remocao de imagem ocorre por endpoints dedicados em `multipart/form-data`.
* Storage local desacoplado por `IAdminImageStorage`, com provider `LocalAdminImageStorage`.
* Arquivos sao gravados em `wwwroot/uploads/backoffice/...` e servidos publicamente por `/uploads/backoffice/...`.
* Substituicao faz upload do novo arquivo, persiste a nova URL/chave e remove o arquivo anterior depois da persistencia.
* Remocao e idempotente para midia ausente e limpa URL/chave no banco.

Endpoints adicionados:

* `POST /api/backoffice/achievements/{id}/badge-image`;
* `DELETE /api/backoffice/achievements/{id}/badge-image`;
* `POST /api/backoffice/levels/{id}/guardian-image`;
* `DELETE /api/backoffice/levels/{id}/guardian-image`.

Validacoes de imagem:

* formatos aceitos: PNG, JPG/JPEG e WebP;
* tamanho maximo real do arquivo: 10 MB;
* validacao de arquivo obrigatorio, tamanho, content type, extensao e assinatura basica;
* request limit dos endpoints permite overhead do multipart sem alterar o limite de arquivo.

Migration:

* `20260716183754_AddAchievementAndLevelImageUploads`;
* adiciona campos nullable `Achievements.BadgeImageUrl`, `Achievements.BadgeImageStorageKey` e `LevelDefinitions.GuardianImageStorageKey`;
* preserva dados existentes e nao altera migrations anteriores.

Testes adicionados/ajustados:

* upload de badge de conquista;
* substituicao de badge removendo arquivo anterior;
* remocao de badge;
* upload de imagem do Guardiao;
* remocao de imagem do Guardiao;
* DTO administrativo de Conquistas retornando `badgeImageUrl`.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\ -p:UseSharedCompilation=false
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\ -p:UseSharedCompilation=false
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 96 aprovados, 0 falhas e 0 ignorados.

## Auditoria de Preparacao para Polimento

Auditoria completa de Backend + Mobile criada em `C:\Forge-mobile\PROJECT_AUDIT.md`.

Resumo da auditoria:

* O nucleo funcional esta integrado: Home, Treinos, Historico, Perfil e Conquistas consomem API real.
* O fluxo de criar, executar e finalizar treino funciona usando endpoints reais.
* XP e conquistas reais estao implementados no backend e integrados ao Mobile.
* Principais estabilizacoes tecnicas ja concluidas:
  * status explicito de treino;
  * duracao real com `StartedAt` e `FinishedAt`;
  * finalizacao transacional e idempotente;
  * seed/catalogo oficial de conquistas;
  * testes dedicados para finalizacao, rollback, idempotencia e catalogo de conquistas;
* Antes do polimento visual, prioridades tecnicas restantes:
  * contratos reais para Guardiao, streaks e progresso parcial de conquistas.

## Visao Geral

Forge e uma aplicacao mobile-first para acompanhamento de treinos, evolucao fisica e gamificacao.

O objetivo do produto e permitir que o usuario registre exercicios, treinos, series, cargas, peso corporal, consumo de agua, sono, XP, niveis e conquistas. O sistema nao cria treinos automaticamente, nao recomenda exercicios/cargas e nao substitui orientacao profissional.

## Visao do Produto

O Forge foi concebido com foco principal em dispositivos moveis. A API deve ser otimizada para atender um aplicativo mobile, pois o usuario normalmente utiliza esse tipo de produto durante ou logo apos o treino.

O mobile oferece uma experiencia mais pratica para registrar series, cargas, peso, agua e sono. Uma versao Web podera existir futuramente para consultas, relatorios e administracao, mas nao e a prioridade do projeto.

## Stack Utilizada

Backend:

* C#
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Swagger/OpenAPI

Clientes planejados:

* Aplicativo mobile como cliente principal
* Web futura para consultas, relatorios e administracao
* React e Tailwind CSS podem ser usados em uma futura interface Web

Projeto atual:

* Solucao: `Forge.slnx`
* Target framework: `net10.0`
* Banco padrao em desenvolvimento: SQL Server via Docker em `127.0.0.1,1433`, database `Forge`

## Estrutura da Solucao

```txt
src/
  Forge.Api/
    Controllers/
    Program.cs
    appsettings.json

  Forge.Application/
    DTOs/
    Interfaces/
    Mappings/
    Services/
    Validators/

  Forge.Domain/
    Entities/
    Enums/

  Forge.Infrastructure/
    Configurations/
    Data/
    Migrations/
    Repositories/
```

Responsabilidades:

* `Forge.Api`: controllers, configuracao da API, Swagger e DI.
* `Forge.Application`: services, DTOs, interfaces, validadores, mappings e regras de negocio.
* `Forge.Domain`: entidades e enums. Nao deve depender de outras camadas.
* `Forge.Infrastructure`: EF Core, DbContext, Fluent API, repositories e migrations.

## Arquivos de Documentacao Existentes

* `README.md`: descricao geral do produto, funcionalidades iniciais e stack planejada.
* `PROJECT_SPEC.md`: especificacao funcional do Forge.
* `DATABASE.md`: modelagem inicial do banco, entidades, relacionamentos e indices.
* `DEVELOPMENT_GUIDELINES.md`: diretrizes de arquitetura, camadas, controllers, services, repositories, DTOs e qualidade.
* `TASKS.md`: checklist macro das fases do projeto.
* `PROJECT_STATUS.md`: este arquivo, usado para continuidade entre chats/desenvolvedores.
* `API_AUDIT.md`: auditoria da API para consumo pelo aplicativo mobile, com problemas, ajustes obrigatorios, melhorias e contratos sugeridos.

## Modulos Implementados

### Implementado e exposto via API

UserProfile:

* Entidade `UserProfile`.
* DTOs de create, update e response.
* Validators de create/update.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `UserProfileController`.
* Fluent API em `UserProfileConfiguration`.
* Usado como usuario base para testes e relacionamentos enquanto nao ha autenticacao.

Exercise:

* Entidade `Exercise`.
* DTOs de create, update e response.
* Validators de create/update.
* Service com regras de negocio.
* Repository EF Core.
* Controller REST em `ExerciseController`.
* Fluent API em `ExerciseConfiguration`.

Workout:

* Entidade `Workout`.
* DTOs de create, update e response.
* Validators de create/update.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `WorkoutController`.
* Fluent API em `WorkoutConfiguration`.
* Campo persistido `Status` com enum `WorkoutStatus`: `Draft`, `InProgress`, `Completed`, `Cancelled`.
* Campos persistidos `StartedAt` e `FinishedAt` para controle real de tempo.
* Endpoint de finalizacao de treino em `POST /api/workouts/{id}/finish`.
* Endpoint de inicio de treino em `POST /api/workouts/{id}/start`.
* Endpoint de cancelamento de treino em `POST /api/workouts/{id}/cancel`.
* Finalizacao valida exercicios e series antes de concluir.
* Finalizacao calcula e atualiza `TotalVolume`.
* Finalizacao define `FinishedAt` e calcula a duracao real a partir de `StartedAt` e `FinishedAt`.
* Finalizacao concede XP uma unica vez por treino usando `ReferenceId = Workout.Id`.
* Finalizacao avalia conquistas automaticamente para criterios suportados.
* Finalizacao e transacional e idempotente: treino ja `Completed` retorna o estado atual sem repetir XP/conquistas.

WorkoutExercise:

* Entidade `WorkoutExercise`.
* DTOs de create e response.
* Validator de create.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `WorkoutExerciseController`.
* Fluent API em `WorkoutExerciseConfiguration`.
* Representa os exercicios vinculados a um treino.
* Ainda nao calcula volume ou XP.

WorkoutSet:

* Entidade `WorkoutSet`.
* DTOs de create, update e response.
* Validators de create/update.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `WorkoutSetController`.
* Fluent API em `WorkoutSetConfiguration`.
* Representa as series registradas em um exercicio do treino.
* Registra ordem da serie, repeticoes, peso e observacoes.
* Ainda nao calcula volume automaticamente, XP ou recordes.

WeightRecord:

* Entidade `WeightRecord`.
* DTOs de create e response.
* Validator de create.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `WeightRecordController`.
* Fluent API em `WeightRecordConfiguration`.
* Representa registros de peso corporal ao longo do tempo.
* Ao registrar novo peso, atualiza `UserProfile.CurrentWeight`.
* Ainda nao gera XP.

WaterIntake:

* Entidade `WaterIntake`.
* DTOs de create e response.
* Validator de create.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `WaterIntakeController`.
* Fluent API em `WaterIntakeConfiguration`.
* Representa o consumo diario de agua do usuario.
* Calcula `GoalAchieved` usando `UserProfile.DailyWaterGoalInLiters`.
* Ainda nao gera XP.

SleepRecord:

* Entidade `SleepRecord`.
* DTOs de create e response.
* Validator de create.
* Service com regras de negocio basicas.
* Repository EF Core.
* Controller REST em `SleepRecordController`.
* Fluent API em `SleepRecordConfiguration`.
* Representa registros de horas de sono do usuario.
* Usa `UserProfile.DailySleepGoalInHours` como meta do registro.
* Calcula `GoalAchieved` usando a meta de sono do perfil.
* Ainda nao gera XP ou conquistas.

Dashboard:

* DTO agregador `DashboardResponse`.
* Service agregador em `DashboardService`.
* Controller REST em `DashboardController`.
* Endpoint unico para a Home mobile em `GET /api/dashboard/{userProfileId}`.
* Consolida nome do usuario, peso, treinos concluidos, volume total, agua de hoje, sono e gamificacao real do `UserProfile`.
* Ainda nao agrega conquistas recentes.
* Nao cria tabelas novas.

Mobile Home:

* DTOs especificos em `Forge.Application.DTOs.Mobile.Home`.
* Service agregador em `MobileHomeService`.
* Controller REST em `MobileHomeController`.
* Endpoint mobile-first em `GET /api/mobile/users/{userProfileId}/home`.
* Consolida usuario, gamificacao real, peso, agua, sono, progresso semanal, treino em andamento e resumo de metricas.
* Reaproveita repositories existentes de UserProfile, Workout, WaterIntake e SleepRecord.
* Usa `UserProfile.Level` e `UserProfile.TotalXp` como fonte de gamificacao.

XP:

* Entidade `XpTransaction`.
* DTOs de transacao e resumo.
* Service real em `XpService`.
* Repository EF Core.
* Controller REST em `XpController`.
* Registra transacoes de XP, atualiza `UserProfile.TotalXp` e recalcula `UserProfile.Level`.
* Evita XP duplicado por treino/conquista usando `Source` + `ReferenceId`.
* Participa da transacao de finalizacao de treino quando XP e concedido por treino concluido.

Achievements:

* Entidades `Achievement` e `UserAchievement`.
* DTOs de catalogo e conquistas desbloqueadas.
* Service real em `AchievementService`.
* Repository EF Core.
* Controller REST em `AchievementController`.
* Avalia conquistas automaticamente ao finalizar treino para categorias suportadas no contrato atual: `Workout`, `Consistency` e `Progression`.
* Categorias `Hydration`, `Sleep` e `Secret` ficam pendentes de eventos/criterios especificos.
* Catalogo oficial possui 11 conquistas iniciais semeadas automaticamente por `AchievementSeeder`.
* Novas conquistas oficiais devem ser adicionadas em `src/Forge.Infrastructure/Seeding/AchievementCatalog.cs` com `Guid` estavel.

## Endpoints Disponiveis

Base route: `/api/user-profiles`

UserProfile:

* `POST /api/user-profiles`
  * Cria um perfil de usuario para testes/relacionamentos.
  * Retorna `201 Created`.
  * Header `Location` aponta para `GET /api/user-profiles/{id}`.
  * Email deve ser unico.

* `GET /api/user-profiles`
  * Lista todos os perfis ordenados por nome.

* `GET /api/user-profiles/{id}`
  * Busca um perfil por id.
  * Retorna `404 Not Found` se nao existir.
  * Rota nomeada: `GetUserProfileById`.

* `PUT /api/user-profiles/{id}`
  * Atualiza os dados basicos de um perfil.
  * Retorna `404 Not Found` se nao existir.
  * Email deve continuar unico.

* `DELETE /api/user-profiles/{id}`
  * Remove um perfil.
  * Retorna `404 Not Found` se nao existir.
  * Retorna `409 Conflict` se possuir exercicios customizados.

Base route: `/api/exercises`

Exercise:

* `POST /api/exercises`
  * Cria um exercicio.
  * Retorna `201 Created`.
  * Header `Location` aponta para `GET /api/exercises/{id}`.

* `GET /api/exercises`
  * Lista todos os exercicios ordenados por nome.

* `GET /api/exercises/{id}`
  * Busca um exercicio por id.
  * Retorna `404 Not Found` se nao existir.
  * Rota nomeada: `GetExerciseById`.

* `PUT /api/exercises/{id}`
  * Atualiza um exercicio existente.
  * Retorna `404 Not Found` se nao existir.

* `DELETE /api/exercises/{id}`
  * Remove um exercicio.
  * Retorna `409 Conflict` se o exercicio estiver vinculado a treino.

Base route: `/api/workouts`

Workout:

* `POST /api/workouts`
  * Cria um treino basico.
  * Retorna `201 Created`.
  * Header `Location` aponta para `GET /api/workouts/{id}`.
  * Exige `UserProfileId` existente.
  * Cria o treino com `Status = Draft`.

* `GET /api/workouts`
  * Lista todos os treinos ordenados por data do treino e criacao, ambos decrescentes.

* `GET /api/workouts/{id}`
  * Busca um treino por id.
  * Retorna `404 Not Found` se nao existir.
  * Rota nomeada: `GetWorkoutById`.
  * Retorna `status`, `startedAt`, `finishedAt` e `durationMinutes`.

* `PUT /api/workouts/{id}`
  * Atualiza os dados basicos de um treino.
  * Retorna `404 Not Found` se nao existir.
  * Exige `UserProfileId` existente.

* `DELETE /api/workouts/{id}`
  * Remove um treino.
  * Retorna `404 Not Found` se nao existir.

* `POST /api/workouts/{id}/finish`
  * Finaliza um treino existente.
  * Retorna `404 Not Found` se o treino nao existir.
  * Retorna `400 BadRequest` se o treino nao possuir exercicios.
  * Retorna `400 BadRequest` se algum exercicio do treino nao possuir series.
  * Calcula o volume de cada serie como `Weight * Repetitions`.
  * Atualiza `TotalVolume` com a soma dos volumes das series.
  * Concede XP por treino concluido.
  * Avalia e concede conquistas automaticamente quando criterios suportados forem atendidos.
  * Altera `Status` para `Completed`.
  * Define `FinishedAt` e preserva compatibilidade definindo `StartedAt` por fallback quando o treino antigo nao tiver horario de inicio.
  * Executa finalizacao, calculo de volume, XP/nivel e conquistas em uma unica transacao.
  * Se o treino ja estiver `Completed`, retorna o estado atual sem repetir efeitos.

* `POST /api/workouts/{id}/start`
  * Inicia um treino existente.
  * Retorna `404 Not Found` se o treino nao existir.
  * Retorna `409 Conflict` se o treino estiver `Completed` ou `Cancelled`.
  * Define `StartedAt` caso ainda nao exista.
  * Altera `Status` para `InProgress`.

* `POST /api/workouts/{id}/cancel`
  * Cancela um treino existente.
  * Retorna `404 Not Found` se o treino nao existir.
  * Retorna `409 Conflict` se o treino estiver `Completed`.
  * Altera `Status` para `Cancelled`.

Base route: `/api/workouts/{workoutId}/exercises`

WorkoutExercise:

* `POST /api/workouts/{workoutId}/exercises`
  * Adiciona um exercicio existente a um treino existente.
  * Retorna `201 Created`.
  * Valida existencia do treino.
  * Valida existencia do exercicio.
  * Nao permite adicionar o mesmo exercicio duas vezes ao mesmo treino.

* `GET /api/workouts/{workoutId}/exercises`
  * Lista os exercicios vinculados ao treino, ordenados por `Order`.
  * Retorna `404 Not Found` se o treino nao existir.

* `DELETE /api/workouts/{workoutId}/exercises/{id}`
  * Remove um exercicio do treino.
  * Retorna `404 Not Found` se o treino ou vinculo nao existir.

Base routes: `/api/workout-exercises/{workoutExerciseId}/sets` e `/api/workout-sets`

WorkoutSet:

* `POST /api/workout-exercises/{workoutExerciseId}/sets`
  * Registra uma serie em um exercicio do treino.
  * Retorna `201 Created`.
  * Valida existencia do `WorkoutExercise`.
  * Registra `SetNumber`, `Repetitions`, `Weight` e `Notes`.
  * `Volume` permanece `0`, sem calculo automatico neste modulo.

* `GET /api/workout-exercises/{workoutExerciseId}/sets`
  * Lista as series do exercicio do treino, ordenadas por `SetNumber`.
  * Retorna `404 Not Found` se o `WorkoutExercise` nao existir.

* `PUT /api/workout-sets/{id}`
  * Atualiza ordem da serie, repeticoes, peso e observacoes.
  * Retorna `404 Not Found` se a serie nao existir.
  * `Volume` permanece `0`, sem calculo automatico neste modulo.

* `DELETE /api/workout-sets/{id}`
  * Remove uma serie.
  * Retorna `404 Not Found` se a serie nao existir.

Base routes: `/api/user-profiles/{userProfileId}/weight-records` e `/api/weight-records`

WeightRecord:

* `POST /api/user-profiles/{userProfileId}/weight-records`
  * Registra o peso corporal de um usuario.
  * Retorna `201 Created`.
  * Valida existencia do `UserProfile`.
  * Atualiza `UserProfile.CurrentWeight` com o peso registrado.
  * Ainda nao gera XP.

* `GET /api/user-profiles/{userProfileId}/weight-records`
  * Lista registros de peso do usuario, ordenados por data decrescente.
  * Retorna `404 Not Found` se o usuario nao existir.

* `GET /api/user-profiles/{userProfileId}/weight-records/latest`
  * Retorna o registro de peso mais recente do usuario.
  * Retorna `404 Not Found` se o usuario nao existir ou se nao houver registros.

* `DELETE /api/weight-records/{id}`
  * Remove um registro de peso.
  * Retorna `404 Not Found` se o registro nao existir.
  * Nao recalcula `UserProfile.CurrentWeight`.

Base routes: `/api/user-profiles/{userProfileId}/water-intakes` e `/api/water-intakes`

WaterIntake:

* `POST /api/user-profiles/{userProfileId}/water-intakes`
  * Registra o consumo de agua de um usuario.
  * Retorna `201 Created`.
  * Valida existencia do `UserProfile`.
  * Registra `Liters` e `IntakeDate`.
  * Usa `UserProfile.DailyWaterGoalInLiters` como meta do registro.
  * Calcula `GoalAchieved` quando `Liters >= GoalInLiters`.
  * Ainda nao gera XP.

* `GET /api/user-profiles/{userProfileId}/water-intakes`
  * Lista consumos de agua do usuario, ordenados por data decrescente.
  * Retorna `404 Not Found` se o usuario nao existir.

* `GET /api/user-profiles/{userProfileId}/water-intakes/today`
  * Retorna o consumo de agua mais recente do dia atual em UTC.
  * Retorna `404 Not Found` se o usuario nao existir ou se nao houver registro no dia.

* `DELETE /api/water-intakes/{id}`
  * Remove um registro de consumo de agua.
  * Retorna `404 Not Found` se o registro nao existir.

Base routes: `/api/user-profiles/{userProfileId}/sleep-records` e `/api/sleep-records`

SleepRecord:

* `POST /api/user-profiles/{userProfileId}/sleep-records`
  * Registra horas de sono de um usuario.
  * Retorna `201 Created`.
  * Valida existencia do `UserProfile`.
  * Registra `SleepDate` e `HoursSlept`.
  * Usa `UserProfile.DailySleepGoalInHours` como meta do registro.
  * Calcula `GoalAchieved` quando `HoursSlept >= GoalInHours`.
  * Ainda nao gera XP ou conquistas.

* `GET /api/user-profiles/{userProfileId}/sleep-records`
  * Lista registros de sono do usuario, ordenados por data decrescente.
  * Retorna `404 Not Found` se o usuario nao existir.

* `GET /api/user-profiles/{userProfileId}/sleep-records/latest`
  * Retorna o registro de sono mais recente do usuario.
  * Retorna `404 Not Found` se o usuario nao existir ou se nao houver registros.

* `DELETE /api/sleep-records/{id}`
  * Remove um registro de sono.
  * Retorna `404 Not Found` se o registro nao existir.

Base route: `/api/user-profiles/{userProfileId}/xp`

XP:

* `GET /api/user-profiles/{userProfileId}/xp`
  * Retorna resumo de XP do usuario.
  * Retorna `404 Not Found` se o `UserProfile` nao existir.
  * Inclui `userProfileId`, `totalXp`, `level`, `xpToNextLevel` e historico de transacoes.
  * As transacoes sao ordenadas por data decrescente.

Base routes: `/api/achievements` e `/api/user-profiles/{userProfileId}/achievements`

Achievements:

* `GET /api/achievements`
  * Retorna o catalogo de conquistas.
  * Inclui nome, descricao, categoria, valor requerido, se e secreta e recompensa em XP.

* `GET /api/user-profiles/{userProfileId}/achievements`
  * Retorna conquistas desbloqueadas pelo usuario.
  * Inclui `UserAchievementId`, `UserProfileId`, `AchievementId` e data de desbloqueio.

Base route: `/api/dashboard`

Dashboard:

* `GET /api/dashboard/{userProfileId}`
  * Retorna dados agregados para a Home mobile.
  * Retorna `404 Not Found` se o `UserProfile` nao existir.
  * Inclui nome do usuario.
  * Inclui peso inicial, peso atual e diferenca.
  * Inclui quantidade de treinos concluidos, ultimo treino concluido e volume total movimentado.
  * Inclui consumo de agua do dia atual em UTC e meta diaria.
  * Inclui ultimo registro de sono, meta diaria de sono e `GoalAchieved`.
  * Retorna gamificacao real usando `UserProfile.Level`, `UserProfile.TotalXp` e XP restante para o proximo nivel.
  * Ainda nao retorna conquistas recentes.

Base route: `/api/mobile/users/{userProfileId}/home`

Mobile Home:

* `GET /api/mobile/users/{userProfileId}/home`
  * Retorna dados agregados para a Home do aplicativo mobile em uma unica chamada.
  * Retorna `404 Not Found` se o `UserProfile` nao existir.
  * Inclui usuario, gamificacao real, peso, agua, sono, progresso semanal, treino em andamento e resumo de metricas.
  * Usa semana UTC iniciando na segunda-feira para progresso semanal.
  * Treino em andamento usa `Workout.Status = InProgress`.
  * Ainda nao retorna conquistas recentes.

Base route: `/api/mobile/users/{userProfileId}/workouts`

Mobile Treinos:

* `GET /api/mobile/users/{userProfileId}/workouts`
  * Retorna dados agregados para a tela Treinos do aplicativo mobile em uma unica chamada.
  * Retorna `404 Not Found` se o `UserProfile` nao existir.
  * Filtra sempre por `userProfileId`.
  * Inclui `activeWorkout` com o treino em andamento mais recente do usuario.
  * Inclui `savedWorkouts` com os demais treinos do usuario.
  * Cada item retorna `id`, `name`, `muscleGroups`, `exerciseCount`, `durationMinutes`, `estimatedDurationMinutes` e `status`.
  * Status possiveis no contrato atual: `inProgress`, `available`, `completed` e `cancelled`.
  * Evita N+1 projetando resumos de treino no repository a partir de `Workouts`, `WorkoutExercises`, `Exercises` e `WorkoutSets`.
  * `durationMinutes` usa tempo real a partir de `StartedAt` e `FinishedAt`; `estimatedDurationMinutes` foi mantido por compatibilidade e recebe o mesmo valor real.

Base route: `/api/mobile/users/{userProfileId}/history`

Mobile Historico:

* `GET /api/mobile/users/{userProfileId}/history?page=1&pageSize=20`
  * Retorna dados agregados para a tela Historico do aplicativo mobile em uma unica chamada.
  * Retorna `404 Not Found` se o `UserProfile` nao existir.
  * Filtra sempre por `userProfileId`.
  * Considera apenas treinos com `Status = Completed`.
  * Inclui resumo com quantidade total de treinos, tempo total real e volume da semana.
  * Inclui lista paginada de treinos com `id`, `name`, `date`, `durationMinutes`, `volume` e `exerciseCount`.
  * Usa `page` e `pageSize`, com `pageSize` limitado a 50.
  * Volume da semana usa semana UTC iniciando na segunda-feira.
  * `durationMinutes` usa tempo real a partir de `StartedAt` e `FinishedAt`.

## Estado do Banco e Migrations

EF Core esta configurado em `ForgeDbContext`.

DbSets existentes:

* `UserProfiles`
* `Exercises`
* `Workouts`
* `WorkoutExercises`
* `WorkoutSets`
* `WeightRecords`
* `WaterIntakes`
* `SleepRecords`
* `XpTransactions`
* `Achievements`
* `UserAchievements`

Migration existente:

* `20260623022316_InitialCreate`
* `20260708153000_AddWorkoutSetNotes`
* `20260714132221_AddWorkoutStatus`
* `20260714133601_AddWorkoutExecutionTime`

Snapshot:

* `ForgeDbContextModelSnapshot.cs`

Estado relevante:

* Ambiente de desenvolvimento padronizado para SQL Server Docker.
* Connection string de runtime e design-time vem dos arquivos `appsettings`.
* `appsettings.json` e `appsettings.Development.json` apontam para `Server=127.0.0.1,1433;Database=Forge`.
* `ForgeDbContextFactory` carrega a mesma configuracao da API e nao possui fallback para outro banco.
* Nao ha referencias ao banco de desenvolvimento anterior nos arquivos de configuracao/documentacao do projeto.
* `Exercises.UserProfileId` esta nullable no modelo e na migration inicial.
* `Exercise.UserProfileId` e `Guid?`.
* `Exercise.UserProfile` e nullable.
* Relacionamento `Exercise -> UserProfile` configurado como opcional no Fluent API.
* Conversor `DateTimeUtcConverter` e usado em configuracoes para datas UTC.

Migration `AddWorkoutSetNotes` adiciona a coluna nullable `Notes` em `WorkoutSets`.

Migration `AddWorkoutStatus` adiciona a coluna obrigatoria `Status` em `Workouts`, com default `Draft`.
Na aplicacao da migration, dados existentes sao preservados quando possivel:

* treinos com exercicios e series em todos os exercicios sao migrados para `Completed`;
* treinos com exercicios, mas ainda nao concluidos pelo criterio antigo, sao migrados para `InProgress`;
* treinos sem exercicios permanecem como `Draft`.

Migration `AddWorkoutExecutionTime` adiciona as colunas nullable `StartedAt` e `FinishedAt` em `Workouts`.
Na aplicacao da migration, dados existentes sao preservados quando possivel:

* treinos `InProgress`, `Completed` e `Cancelled` recebem `StartedAt = WorkoutDate` quando ainda nao possuem horario de inicio;
* treinos `Completed` e `Cancelled` recebem `FinishedAt = UpdatedAt` quando `UpdatedAt` e posterior a `StartedAt`;
* quando nao ha intervalo valido, a duracao calculada permanece `0` em vez de usar estimativa fixa.

## Decisoes de Arquitetura

* Controllers devem ser finos e sem regra de negocio.
* Services concentram validacoes e regras de negocio.
* Repositories apenas acessam e persistem dados.
* API nao retorna entidades diretamente; usa DTOs.
* APIs devem ser pensadas primeiro para consumo mobile.
* Evitar multiplas requisicoes quando uma unica resposta puder atender uma tela mobile.
* Priorizar respostas objetivas e rapidas.
* Pensar sempre na experiencia do usuario mobile antes da versao Web.
* Fluent API e usado para configurar entidades no EF Core.
* Transacoes de caso de uso sao expostas para a Application por `IApplicationTransaction` e implementadas na Infrastructure com EF Core.
* Seed de catalogos oficiais roda no startup da API via `SeedAchievementsAsync`.
* O seed de conquistas e idempotente: compara por `Id` estavel e por `Name`, insere apenas ausentes e preserva registros ja existentes.
* `Guid` e usado como chave primaria.
* Datas devem ser tratadas em UTC.
* `UserProfileController` usa rota nomeada e `CreatedAtRoute` para gerar o header `Location` corretamente.
* `ExerciseController` usa rota nomeada e `CreatedAtRoute` para gerar o header `Location` corretamente.
* `WorkoutController` usa rota nomeada e `CreatedAtRoute` para gerar o header `Location` corretamente.
* `WorkoutExerciseController` usa rota nomeada e `CreatedAtRoute` para gerar o header `Location` corretamente.
* CORS possui policy especifica de desenvolvimento chamada `ForgeMobileWebDevelopment`.
* A policy de desenvolvimento permite apenas as origens `http://localhost:8082`, `http://localhost:5173` e `http://localhost:5174`, com metodos e headers necessarios para o Forge Mobile Web local e o Forge Backoffice local.
* CORS nao usa `AllowAnyOrigin` em producao.

## Regras Importantes

UserProfile:

* Ainda nao ha autenticacao.
* `UserProfile` e o usuario base para testes e relacoes com treinos, exercicios customizados, peso, agua e sono.
* Email e obrigatorio, normalizado para lowercase e deve ser unico.
* Nome e obrigatorio.
* Peso inicial, peso atual, meta diaria de agua, meta diaria de sono e meta semanal de treinos devem ser maiores que zero.
* Ao criar, `CurrentWeight` inicia igual a `InitialWeight`.
* Ao criar, `Level` inicia em `1` e `TotalXp` em `0`.
* Delete retorna conflito se o perfil possuir exercicios customizados.

Exercise:

* `IsCustom == false`:
  * O exercicio e global.
  * `UserProfileId` deve ser salvo como `null`.
  * Nao deve validar existencia de usuario.
  * Nao deve usar Guid fake ou hardcoded.

* `IsCustom == true`:
  * `UserProfileId` e obrigatorio.
  * `UserProfileId` nao pode ser `Guid.Empty`.
  * O usuario deve existir antes de salvar.
  * Se o usuario nao existir, a API deve retornar `400 BadRequest`, nao erro 500 de FK.

Delete de Exercise:

* Um exercicio vinculado a `WorkoutExercise` nao pode ser removido.
* Nesse caso, a API retorna `409 Conflict`.

Workout:

* O modulo atual cria apenas a sessao basica de treino.
* Series sao registradas pelo modulo `WorkoutSet`.
* Finalizacao de treino valida se existe pelo menos um exercicio e pelo menos uma serie por exercicio.
* Finalizacao de treino calcula `TotalVolume` somando os volumes das series.
* XP e conquistas nao sao gerados ao criar treino.
* Ao finalizar treino, XP e conquistas suportadas sao processados automaticamente.
* Ao finalizar treino, finalizacao, volume, XP/nivel e conquistas rodam na mesma transacao.
* Segunda tentativa de finalizar treino ja concluido e idempotente e nao repete XP/conquistas.
* `UserProfileId` e obrigatorio.
* `UserProfileId` nao pode ser `Guid.Empty`.
* O usuario deve existir antes de salvar.
* `TotalVolume` inicia com `0` e e atualizado na finalizacao do treino.

WorkoutExercise:

* Um `WorkoutExercise` pertence a um `Workout`.
* Um `WorkoutExercise` referencia um `Exercise`.
* O treino deve existir antes de adicionar exercicio.
* O exercicio deve existir antes de ser vinculado ao treino.
* O mesmo exercicio nao pode ser adicionado duas vezes ao mesmo treino.
* `Order` deve ser maior que zero.
* Series sao registradas pelo modulo `WorkoutSet`.
* Volume ainda nao e calculado.
* XP ainda nao e gerado.

WorkoutSet:

* Um `WorkoutSet` pertence a um `WorkoutExercise`.
* O `WorkoutExercise` deve existir antes de registrar a serie.
* `SetNumber` deve ser maior que zero.
* `Repetitions` deve ser maior que zero.
* `Weight` nao pode ser negativo.
* `Notes` e opcional.
* `Volume` permanece `0` por enquanto.
* XP e recordes ainda nao sao calculados.

WeightRecord:

* Um `WeightRecord` pertence a um `UserProfile`.
* O usuario deve existir antes de registrar peso.
* `Weight` deve ser maior que zero.
* Ao registrar novo peso, `UserProfile.CurrentWeight` e atualizado.
* Delete de registro de peso nao recalcula `CurrentWeight`.
* XP ainda nao e gerado.

WaterIntake:

* Um `WaterIntake` pertence a um `UserProfile`.
* O usuario deve existir antes de registrar consumo de agua.
* `Liters` deve ser maior que zero.
* `IntakeDate` registra data e hora do consumo.
* `GoalInLiters` e preenchido a partir de `UserProfile.DailyWaterGoalInLiters`.
* `GoalAchieved` e verdadeiro quando `Liters >= GoalInLiters`.
* O endpoint `today` considera o dia atual em UTC.
* XP ainda nao e gerado.

SleepRecord:

* Um `SleepRecord` pertence a um `UserProfile`.
* O usuario deve existir antes de registrar sono.
* `HoursSlept` deve ser maior que zero.
* `SleepDate` registra a data do sono.
* `GoalInHours` e preenchido a partir de `UserProfile.DailySleepGoalInHours`.
* `GoalAchieved` e verdadeiro quando `HoursSlept >= GoalInHours`.
* XP e conquistas ainda nao sao gerados.

Dashboard:

* O dashboard pertence a um `UserProfile`.
* O usuario deve existir para retornar o agregado.
* A diferenca de peso e calculada como `CurrentWeight - InitialWeight`.
* Treino concluido e considerado quando `Workout.Status == Completed`.
* Duracao de treino e calculada a partir de `StartedAt` e `FinishedAt`.
* O consumo de agua de hoje soma os registros do dia atual em UTC.
* A gamificacao usa `UserProfile.Level`, `UserProfile.TotalXp` e XP restante para o proximo nivel.
* Conquistas recentes ainda nao sao agregadas no dashboard.

Exemplo de JSON para criar exercicio global:

```json
{
  "name": "Supino reto",
  "description": "Exercicio global para peitoral",
  "muscleGroup": 1,
  "isCustom": false,
  "userProfileId": null
}
```

## Problemas Ja Corrigidos

* Warning `NU1903` por vulnerabilidade em `Microsoft.OpenApi` 2.3.0:
  * Correcao: removida referencia direta nao usada a `Microsoft.AspNetCore.OpenApi` e ajustado `Swashbuckle.AspNetCore` para versao sem dependencia vulneravel no estado atual.
  * `dotnet build Forge.slnx` passou com `0 Aviso(s)` apos a alteracao.

* Erros HTTP nao possuiam formato padronizado:
  * Correcao: adicionado middleware global de excecoes com resposta `ProblemDetails`.
  * `ArgumentException` retorna `400 BadRequest`.
  * `InvalidOperationException` retorna `409 Conflict`.
  * Excecoes inesperadas retornam `500 InternalServerError` sem expor detalhe interno.
  * Recursos nao encontrados com `404` sem corpo passam a retornar `ProblemDetails`.

* Nao havia projeto de testes automatizados:
  * Correcao: adicionado `tests/Forge.Api.Tests`.
  * Testes cobrem recurso nao encontrado, erro de validacao, conflito de dominio e excecao inesperada.

* Erro 500 por FK em `POST /api/exercises` ao criar exercicio global:
  * Causa: fluxo podia tentar salvar `UserProfileId` inexistente.
  * Correcao: service normaliza `UserProfileId` para `null` quando `IsCustom == false`.

* Erro 500 por FK ao criar exercicio customizado com usuario inexistente:
  * Correcao: service valida existencia do usuario antes de salvar.
  * Se nao existir, retorna `400 BadRequest`.

* `Guid.Empty` para exercicio customizado:
  * Correcao: validators rejeitam `null` e `Guid.Empty` quando `IsCustom == true`.

* Erro 500 `No route matches the supplied values` no `CreatedAtAction`:
  * Causa: incompatibilidade entre nome de action/route values e GET by id.
  * Correcao: GET by id recebeu rota nomeada `GetExerciseById` e POST passou a usar `CreatedAtRoute`.

* `WorkoutService` estava como esqueleto com `NotImplementedException`:
  * Correcao: implementado CRUD basico de Workout.
  * Adicionados repository, controller, DTO de update, validator de update e registros em DI.

* `UserProfile` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado CRUD completo de UserProfile.
  * Adicionados DTOs, mappings, validators, service, repository, controller e registros em DI.
  * Email unico validado no service antes de persistir.

* `WorkoutExercise` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado modulo para adicionar, listar e remover exercicios de um treino.
  * Adicionados DTO, validator, service, repository, controller e registros em DI.
  * Valida existencia de Workout e Exercise antes de persistir.
  * Impede duplicidade do mesmo Exercise no mesmo Workout.

* `WorkoutSet` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado modulo para registrar, listar, atualizar e remover series de um exercicio do treino.
  * Adicionados DTOs, validators, service, repository, controller e registros em DI.
  * Valida existencia de WorkoutExercise antes de persistir.
  * Adicionada coluna `Notes` em `WorkoutSets`.

* Treinos nao possuiam endpoint de finalizacao:
  * Correcao: implementado `POST /api/workouts/{id}/finish`.
  * Valida existencia do treino.
  * Valida existencia de pelo menos um exercicio no treino.
  * Valida que cada exercicio do treino possui pelo menos uma serie.
  * Calcula volume das series e atualiza `TotalVolume` do treino.
  * XP e conquistas passaram a ser processados pelo modulo de gamificacao ao finalizar treino.

* `WeightRecord` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado modulo para registrar, listar, buscar ultimo registro e remover registros de peso corporal.
  * Adicionados DTO, validator, service, repository, controller e registros em DI.
  * Valida existencia de UserProfile antes de persistir.
  * Atualiza `UserProfile.CurrentWeight` ao registrar novo peso.
  * XP permanece fora deste modulo.

* `WaterIntake` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado modulo para registrar, listar, buscar registro de hoje e remover consumos de agua.
  * Adicionados DTO, validator, service, repository, controller e registros em DI.
  * Valida existencia de UserProfile antes de persistir.
  * Calcula `GoalAchieved` a partir da meta diaria de agua do perfil.
  * XP permanece fora deste modulo.

* `SleepRecord` existia apenas como entidade/base de relacionamento:
  * Correcao: implementado modulo para registrar, listar, buscar ultimo registro e remover registros de sono.
  * Adicionados DTO, validator, service, repository, controller e registros em DI.
  * Valida existencia de UserProfile antes de persistir.
  * Calcula `GoalAchieved` a partir da meta diaria de sono do perfil.
  * XP e conquistas permanecem fora deste modulo.

* Home mobile exigia multiplas chamadas para montar dados principais:
  * Correcao: implementado `GET /api/dashboard/{userProfileId}`.
  * Adicionados DTO agregador, service, controller e registro em DI.
  * Endpoint consolida usuario, peso, treinos, agua, sono e gamificacao real.
  * Nao foram criadas novas tabelas.

* Home mobile precisava de contrato especifico para o app:
  * Correcao: implementado `GET /api/mobile/users/{userProfileId}/home`.
  * Adicionados DTOs mobile, service, controller e registro em DI.
  * Endpoint agrega usuario, nivel/XP real, peso, agua, sono, progresso semanal, treino em andamento e resumo de metricas.
  * Endpoint antigo de dashboard foi preservado.

* Tela Treinos mobile exigia multiplas chamadas para montar listas e status:
  * Correcao: implementado `GET /api/mobile/users/{userProfileId}/workouts`.
  * Adicionados DTOs mobile, service, controller, registro em DI e projecao no repository.
  * Endpoint agrega treino em andamento e treinos salvos/disponiveis em uma unica resposta.
  * Endpoint filtra por `userProfileId` e preserva endpoints existentes de `/api/workouts`.
  * Adicionados testes para usuario inexistente e retorno valido.

* Tela Historico mobile exigia agregacao e paginacao especificas:
  * Correcao: implementado `GET /api/mobile/users/{userProfileId}/history?page=1&pageSize=20`.
  * Adicionados DTOs mobile, service, controller, registro em DI e projecoes no repository.
  * Endpoint agrega resumo e lista paginada de treinos concluidos em uma unica resposta.
  * Endpoint filtra por `userProfileId` e preserva endpoints existentes de `/api/workouts`.
  * Adicionados testes para usuario inexistente e retorno valido.

* Forge Mobile Web em desenvolvimento precisava consumir a API local:
  * Correcao: configurada policy CORS `ForgeMobileWebDevelopment`.
  * Origem inicial permitida: `http://localhost:8082`; em 15/07/2026 a mesma policy foi ampliada para `http://localhost:5173` e `http://localhost:5174`.
  * Policy aplicada somente em ambiente `Development`.
  * Nao foi usado `AllowAnyOrigin` em producao.

* Fluxo funcional de treinos fechado no Forge Mobile usando endpoints existentes da API:
  * listagem de exercicios globais/customizados via `GET /api/exercises`;
  * criacao de treino via `POST /api/workouts`;
  * vinculo de exercicios via `POST /api/workouts/{workoutId}/exercises`;
  * registro, edicao e exclusao de series via `POST /api/workout-exercises/{workoutExerciseId}/sets`, `PUT /api/workout-sets/{id}` e `DELETE /api/workout-sets/{id}`;
  * finalizacao via `POST /api/workouts/{id}/finish`.
* Nenhum endpoint novo foi necessario nesta etapa.

## Grupos Musculares Oficiais - 15/07/2026

Implementacao adicionada para tornar grupos musculares parte oficial da arquitetura do Forge:

* Nova entidade `MuscleGroup` com `Id`, `Name`, `DisplayName`, `Icon`, `DisplayOrder`, `IsActive`, `CreatedAt` e `UpdatedAt`.
* Nova entidade de relacionamento `WorkoutMuscleGroup`, permitindo que um treino possua um ou mais grupos musculares.
* `Exercise` passou a ter `MuscleGroupId` como FK opcional de compatibilidade, preservando temporariamente o enum legado `MuscleGroup` para contratos existentes.
* Catalogo oficial em `MuscleGroupCatalog` com 15 grupos iniciais: Peito, Costas, Ombro, Biceps, Triceps, Antebraco, Abdomen, Lombar, Gluteo, Quadriceps, Posterior, Panturrilha, Corpo inteiro, Cardio e Outros.
* Seed idempotente `MuscleGroupSeeder`, executado no startup antes do seed de conquistas.
* Migration criada: `20260715143000_AddMuscleGroups`.
* Migration cria `MuscleGroups`, adiciona `Exercises.MuscleGroupId`, cria `WorkoutMuscleGroups`, faz backfill de exercicios antigos a partir do enum legado e popula grupos dos treinos existentes a partir dos exercicios vinculados.
* Endpoints adicionados:
  * `GET /api/mobile/muscle-groups` para listar grupos ativos ordenados por `DisplayOrder`.
  * `GET /api/mobile/exercises?muscleGroupId={id}` para listar exercicios com filtro opcional por grupo muscular.
* DTOs de exercicio passaram a retornar `muscleGroupId` e `muscleGroupDisplayName`.
* `CreateExerciseRequest` e `UpdateExerciseRequest` aceitam `MuscleGroupId` opcional; quando ausente, a API resolve pelo enum legado para manter compatibilidade.
* `WorkoutExerciseService` sincroniza automaticamente `WorkoutMuscleGroups` ao adicionar ou remover exercicios de um treino.
* Agregador mobile de treinos usa `WorkoutMuscleGroups` quando existir e mantem fallback pelos exercicios para dados antigos.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\
```

Resultado:

* Build passou com 0 erros.
* Testes passaram com 13 aprovados, 0 falhas e 0 ignorados.
* Foi usada saida alternativa porque havia processo `Forge.Api` local bloqueando as DLLs em `bin`.

## CRUD Administrativo de Grupos Musculares - 15/07/2026

Implementacao adicionada para preparar a Forge.Api para o Forge Backoffice sem alterar os endpoints mobile existentes:

* CORS de desenvolvimento atualizado na policy existente `ForgeMobileWebDevelopment`.
* Origens permitidas em desenvolvimento:
  * `http://localhost:8082`;
  * `http://localhost:5173`;
  * `http://localhost:5174`.
* Nao foi usado `AllowAnyOrigin`; producao continua restrita.
* Nova rota administrativa: `/api/backoffice/muscle-groups`.
* Endpoints administrativos adicionados:
  * `GET /api/backoffice/muscle-groups`;
  * `GET /api/backoffice/muscle-groups/{id}`;
  * `POST /api/backoffice/muscle-groups`;
  * `PUT /api/backoffice/muscle-groups/{id}`;
  * `PATCH /api/backoffice/muscle-groups/{id}/status`;
  * `DELETE /api/backoffice/muscle-groups/{id}`.
* Contrato administrativo retorna `id`, `name`, `displayName`, `icon`, `displayOrder`, `isActive` e `exerciseCount`.
* `name` administrativo e normalizado com `Trim().ToLowerInvariant()` antes de persistir.
* `name` e obrigatorio e unico por comparacao case-insensitive.
* `displayName` e obrigatorio.
* `displayOrder` deve ser maior que zero.
* Criacao retorna `201 Created` com `Location` usando rota nomeada.
* Grupo inexistente retorna `404 Not Found`.
* Nome duplicado retorna conflito pelo middleware global (`InvalidOperationException` -> `409 Conflict`).
* `PATCH /status` ativa/desativa sem excluir o registro.
* `DELETE` faz exclusao fisica apenas para grupos administrativos nao oficiais e sem exercicios associados.
* Grupos associados a exercicios retornam conflito ao tentar excluir.
* Grupos oficiais do catalogo nao podem ser excluidos fisicamente; devem ser desativados. Decisao tomada para preservar IDs estaveis e impedir que o seed recrie registros oficiais.
* Endpoint mobile `GET /api/mobile/muscle-groups` continua usando apenas grupos ativos via `GetActiveAsync`.
* Decisao documentada: grupos inativos ficam omitidos do endpoint mobile por comportamento seguro.
* Nenhuma migration foi criada, pois `MuscleGroups` ja possuia `Name`, `DisplayName`, `Icon`, `DisplayOrder`, `IsActive`, `CreatedAt` e `UpdatedAt` no modelo, migration e snapshot atuais.
* Testes adicionados para listagem administrativa, criacao valida, nome duplicado, atualizacao, ativacao/desativacao, bloqueio de exclusao com exercicios, grupo inexistente e endpoint mobile retornando apenas ativos.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 22 aprovados, 0 falhas e 0 ignorados.
* Verificacao runtime do Swagger em `http://localhost:5099/swagger/v1/swagger.json` foi tentada, mas a API nao respondeu no tempo do ambiente. Os endpoints estao expostos por controller compilado e devem aparecer no Swagger ao subir a API localmente com banco/seed disponiveis.

## CRUD Administrativo de Exercicios - 15/07/2026

Implementacao adicionada para preparar a Forge.Api para o Forge Backoffice sem alterar os endpoints mobile existentes:

* Nova rota administrativa: `/api/backoffice/exercises`.
* Endpoints administrativos adicionados:
  * `GET /api/backoffice/exercises`;
  * `GET /api/backoffice/exercises/{id}`;
  * `POST /api/backoffice/exercises`;
  * `PUT /api/backoffice/exercises/{id}`;
  * `PATCH /api/backoffice/exercises/{id}/status`;
  * `DELETE /api/backoffice/exercises/{id}`.
* Contrato administrativo contempla:
  * `id`;
  * `name`;
  * `description`;
  * `muscleGroupId`;
  * `muscleGroupName`;
  * `difficulty`;
  * `equipment`;
  * `isCustom`;
  * `isActive`;
  * `displayOrder`;
  * `imageUrl`;
  * `gifUrl`;
  * `videoUrl`;
  * `thumbnailUrl`.
* Upload de midia nao foi implementado nesta etapa; os campos de midia ficam preparados para futura integracao e podem permanecer nulos.
* Listagem administrativa possui pesquisa por nome, filtro por grupo muscular, filtro ativo/inativo, ordenacao e paginacao.
* Filtros, ordenacao e paginacao sao aplicados no repository com `IQueryable` antes da materializacao.
* `name` e obrigatorio e unico por comparacao case-insensitive.
* `muscleGroupId` e obrigatorio e deve apontar para grupo muscular existente/ativo.
* IDs vazios retornam `404 Not Found` nos endpoints por id ou erro de validacao quando enviados no payload.
* Criacao retorna `201 Created` com `Location` usando rota nomeada.
* Nome duplicado retorna conflito pelo middleware global (`InvalidOperationException` -> `409 Conflict`).
* `PATCH /status` ativa/desativa sem excluir o registro.
* `DELETE` faz exclusao fisica apenas quando o exercicio nao esta vinculado a treinos.
* Exercicios vinculados a treinos retornam conflito ao tentar excluir.
* Endpoints existentes `/api/exercises` e `/api/mobile/exercises` foram preservados.
* O endpoint mobile continua usando `ExerciseResponse` e nao teve seu contrato expandido pelos campos administrativos.
* Migration criada: `20260715180304_AddBackofficeExerciseFields`.
* A migration adiciona em `Exercises`: `Difficulty`, `Equipment`, `IsActive`, `DisplayOrder`, `ImageUrl`, `GifUrl`, `VideoUrl` e `ThumbnailUrl`.
* `IsActive` recebeu default `true` para registros existentes.
* Testes adicionados para CRUD administrativo, duplicidade, grupo inexistente, ativacao/desativacao, exclusao bloqueada, listagem, filtros, paginacao e endpoint mobile preservado.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 31 aprovados, 0 falhas e 0 ignorados.

## Sprint de Consolidacao da API - 15/07/2026

Auditoria executada com foco em estabilidade e consistencia dos modulos atualmente implementados, sem criacao de novos modulos, novas funcionalidades ou alteracao de regra de negocio.

Itens verificados:

* Contratos HTTP e rotas dos controllers existentes.
* DTOs mobile, administrativos e CRUD legado.
* Services, repositories, validators e interfaces das camadas Application/Infrastructure.
* Uso de `async/await` e propagacao de `CancellationToken` nos fluxos principais.
* Middleware global de `ProblemDetails` para excecoes e `404` sem corpo.
* CORS de desenvolvimento para Mobile Web e Backoffice local.
* Paginacao/filtros/ordenacao dos endpoints que ja possuem esse contrato.
* Nullability dos contratos administrativos recentes.
* Busca por TODOs, codigo morto obvio, endpoints duplicados, `AllowAnyOrigin`, chamadas bloqueantes e warnings.

Resultado da auditoria:

* Nenhum endpoint mobile existente foi removido ou renomeado.
* Nenhum endpoint administrativo existente foi removido ou renomeado.
* Mobile permanece compativel com os contratos atuais, incluindo `/api/mobile/muscle-groups`, `/api/mobile/exercises`, Home, Treinos e Historico.
* Backoffice permanece compativel com os CRUDs administrativos atualmente implementados para Grupos Musculares e Exercicios.
* `GET /api/backoffice/exercises` mantem pesquisa, filtro por grupo, filtro ativo/inativo, ordenacao e paginacao.
* `GET /api/mobile/muscle-groups` permanece retornando somente grupos ativos.
* `ProblemDetailsExceptionMiddleware` continua padronizando `ArgumentException` como `400`, `InvalidOperationException` como `409` e erros inesperados como `500` sem expor stack trace.
* `ProblemDetailsStatusCodeMiddleware` continua convertendo `404` sem corpo em `ProblemDetails`.
* CORS de desenvolvimento permanece restrito a `http://localhost:8082`, `http://localhost:5173` e `http://localhost:5174`; nao ha `AllowAnyOrigin`.
* Nenhuma migration foi criada nesta consolidacao.
* Nenhuma alteracao de codigo foi necessaria para passar build/testes.

Achados mantidos como pendencia, por estarem fora do escopo de consolidacao ou representarem funcionalidade nova:

* Upload/remocao de midia de exercicios ainda nao existe na Forge.Api; os campos `ImageUrl`, `GifUrl`, `VideoUrl` e `ThumbnailUrl` seguem apenas como estrutura preparada no CRUD administrativo.
* Alguns fakes de testes ainda usam `NotImplementedException` em metodos nao exercitados. A suite atual passa, mas a limpeza desses fakes segue recomendada para uma futura etapa de higiene de testes.
* `API_AUDIT.md` historicamente cita algumas lacunas dos CRUDs base sem paginacao; isso continua valido para endpoints legados que nao receberam contrato paginado.

Validacao executada:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\consolidation-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\consolidation-test-bin\
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 31 aprovados, 0 falhas e 0 ignorados.


## CRUD Administrativo de Conquistas - 15/07/2026

Implementacao adicionada para preparar a Forge.Api para o Forge Backoffice sem alterar o Forge.Mobile nem os endpoints publicos existentes de conquistas.

Endpoints administrativos adicionados:

* `GET /api/backoffice/achievements`;
* `GET /api/backoffice/achievements/{id}`;
* `POST /api/backoffice/achievements`;
* `PUT /api/backoffice/achievements/{id}`;
* `PATCH /api/backoffice/achievements/{id}/status`;
* `DELETE /api/backoffice/achievements/{id}`.

Contrato administrativo contempla:

* `id`;
* `name`;
* `description`;
* `category`;
* `categoryName`;
* `requiredValue`;
* `xpReward`;
* `isSecret`;
* `isActive`;
* `isOfficial`;
* `unlockedCount`;
* `createdAt`;
* `updatedAt`.

Listagem administrativa:

* pesquisa por nome;
* filtro por categoria;
* filtro ativo/inativo;
* filtro secreta/nao secreta;
* ordenacao por nome, categoria, valor necessario, recompensa de XP, status, segredo, quantidade de desbloqueios e criacao;
* paginacao com limite maximo de 100 itens por pagina;
* contagem de desbloqueios projetada no repository sem N+1.

Regras implementadas:

* `name` e obrigatorio e unico por comparacao case-insensitive;
* `description` e obrigatoria;
* `category` deve ser valor valido de `AchievementCategory`;
* `requiredValue` deve ser maior que zero;
* `xpReward` deve ser maior ou igual a zero;
* criacao gera novo `Guid`, sem IDs fixos para conquistas administrativas;
* criacao retorna `201 Created` com `Location` apontando para o detalhe administrativo;
* atualizacao nao altera ID, nao recalcula XP historico, nao revoga desbloqueios e nao altera transacoes antigas;
* mudancas em `xpReward` valem apenas para desbloqueios futuros;
* `PATCH /status` ativa/desativa sem excluir registro.

Regra de conquistas inativas:

* continuam existindo no banco;
* continuam visiveis no Backoffice;
* preservam todos os `UserAchievement` existentes;
* nao sao retornadas em `GET /api/achievements`;
* nao sao avaliadas em novos desbloqueios;
* nao concedem XP futuramente;
* conquistas ja desbloqueadas continuam retornando em `GET /api/user-profiles/{userProfileId}/achievements`.

Protecao de conquistas oficiais:

* IDs oficiais foram centralizados em `OfficialAchievementIds` na camada Domain;
* `AchievementCatalog` usa esses IDs estaveis;
* conquistas oficiais podem ser editadas/desativadas, mas nao excluidas fisicamente;
* tentativa de exclusao fisica de conquista oficial retorna conflito pelo middleware global (`InvalidOperationException` -> `409 Conflict`);
* conquistas administrativas sem vinculos podem ser excluidas fisicamente.

Seed oficial:

* continua idempotente;
* insere apenas conquistas oficiais ausentes;
* nao duplica por ID ou nome;
* nao apaga registros;
* nao sobrescreve status, descricao, recompensa ou outras alteracoes feitas pelo Backoffice;
* registros oficiais existentes receberam `IsActive = true` por default seguro via migration.

Banco/migration:

* Migration criada: `20260715185234_AddAchievementAdministrativeFields`;
* adiciona `Achievements.IsActive` como `bit` obrigatorio com default `true`;
* preserva as 11 conquistas oficiais existentes;
* preserva `UserAchievements` e historico de XP.

Compatibilidade:

* `GET /api/achievements` foi preservado e continua retornando o mesmo DTO publico, agora apenas com conquistas ativas;
* `GET /api/user-profiles/{userProfileId}/achievements` foi preservado e continua retornando conquistas desbloqueadas do usuario, incluindo historicas inativas;
* finalizacao transacional de treino continua avaliando conquistas dentro do fluxo existente;
* XP por conquista continua idempotente por `Source = AchievementUnlocked` + `ReferenceId = Achievement.Id`;
* categorias ainda sem regra de avaliacao (`Hydration`, `Sleep`, `Secret`) continuam ignoradas no fechamento de treino.

Testes adicionados/atualizados:

* listagem administrativa com filtros e paginacao;
* detalhe existente e inexistente;
* criacao valida;
* duplicidade de nome;
* categoria invalida;
* valor necessario invalido;
* atualizacao valida;
* duplicidade na atualizacao;
* ativacao/desativacao;
* exclusao permitida sem vinculos;
* bloqueio de exclusao com `UserAchievement`;
* bloqueio de exclusao de conquista oficial;
* catalogo publico retornando apenas ativas;
* conquista inativa ignorada na avaliacao;
* conquista ativa desbloqueada sem duplicar XP;
* historico do usuario preservando conquista ja desbloqueada mesmo inativa;
* catalogo oficial usando IDs estaveis centralizados.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 49 aprovados, 0 falhas e 0 ignorados.


## Auditoria Arquitetural de Gamificacao, Raridades e Niveis - 15/07/2026

Auditoria executada sobre Achievements, XP, Niveis, Guardiao/Gamificacao, seeds, DTOs, entidades, enums e banco antes da evolucao administrativa de Raridades/Niveis.

Estado real encontrado:

* Raridade ja existe como entidade/tabela administrativa no estado atual da Forge.Api: `Rarity` / `Rarities`.
* Nao existe entidade/tabela de Nivel. O nivel atual e apenas `UserProfile.Level`, um inteiro derivado de `TotalXp` pelo `XpService`.
* XP real existe via `XpTransaction`, `XpService` e `XpSource`.
* Conquistas reais existem via `Achievement`, `UserAchievement`, `AchievementService`, seed oficial e CRUD administrativo.
* Guardiao ainda nao existe como entidade, agregado, DTO dedicado ou regra de dominio; hoje aparece apenas como conceito futuro e em uma conquista secreta oficial.
* Gamificacao mobile/dashboard usa `UserProfile.Level`, `UserProfile.TotalXp` e XP restante para o proximo nivel.
* Nao ha relacao persistida atual entre Raridades e Conquistas.
* Nao ha relacao persistida atual entre Raridades e Niveis.

Decisoes arquiteturais:

* Raridade deve permanecer como tabela, nao enum, porque possui metadados administraveis reais: nome, cores, ordem, status e futura associacao com conquistas/niveis/recompensas.
* Um enum de raridade geraria retrabalho quando o Backoffice precisar administrar cor, ordem, status, catalogo visual e possiveis regras de exibicao.
* O CRUD de Raridades e valido como modulo independente, pois representa catalogo visual/de classificacao reutilizavel por Conquistas, Niveis e futuramente Guardiao.
* Os contadores `achievementCount` e `levelCount` ficam no contrato de Raridades, mas retornam `0` enquanto as relacoes reais nao existirem.

Ajuste recomendado antes do CRUD administrativo de Niveis:

* Criar primeiro uma entidade/tabela de catalogo de niveis, em vez de expor CRUD diretamente sobre o inteiro `UserProfile.Level`.
* O modelo sugerido para uma proxima tarefa deve separar o nivel atual do usuario do catalogo de progressao, por exemplo `LevelDefinition` ou `Level` com `Number`, `Name`, `RequiredXp`, `RarityId` opcional, `IsActive`, `DisplayOrder`, `CreatedAt` e `UpdatedAt`.
* `XpService` deve continuar calculando o nivel do usuario, mas a fonte da progressao deve migrar gradualmente de constante fixa `XpPerLevel = 500` para o catalogo de niveis quando esse catalogo existir.
* Essa mudanca deve ser pequena e focada, preservando `UserProfile.Level` por compatibilidade com Mobile e evitando recalculo historico inesperado.

Riscos evitados:

* Criar um CRUD de Niveis sem entidade de catalogo geraria uma administracao falsa ou acoplada ao inteiro do usuario.
* Persistir raridade como enum dificultaria alteracoes visuais e administrativas pelo Backoffice.
* Ligar Raridades a Conquistas/Niveis antes da modelagem de Nivel poderia criar chaves temporarias e retrabalho.

Conclusao:

* O CRUD de Raridades esta arquiteturalmente adequado e pode ser mantido.
* Antes do CRUD de Niveis, recomenda-se criar a modelagem pequena de catalogo de niveis e ajustar o `XpService` para ficar preparado para essa fonte de progressao.
* Nao ha recomendacao de refatoracao ampla neste momento.

## CRUD Administrativo de Raridades - 15/07/2026

Implementacao adicionada para preparar a Forge.Api para o Forge Backoffice sem alterar Forge.Mobile, Forge.Backoffice, Conquistas ou Niveis.

Endpoints administrativos adicionados:

* `GET /api/backoffice/rarities`;
* `GET /api/backoffice/rarities/{id}`;
* `POST /api/backoffice/rarities`;
* `PUT /api/backoffice/rarities/{id}`;
* `PATCH /api/backoffice/rarities/{id}/status`;
* `DELETE /api/backoffice/rarities/{id}`.

Contrato administrativo contempla:

* `id`;
* `name`;
* `primaryColor`;
* `secondaryColor`;
* `displayOrder`;
* `isActive`;
* `achievementCount`;
* `levelCount`;
* `createdAt`;
* `updatedAt`.

Listagem administrativa:

* pesquisa por nome;
* filtro ativo/inativo;
* ordenacao por nome, ordem, status, criacao e campos de contagem;
* paginacao com limite maximo de 100 itens por pagina;
* filtros, ordenacao e paginacao aplicados no repository com `IQueryable` antes da materializacao.

Regras implementadas:

* `name` e obrigatorio e unico por comparacao case-insensitive;
* `primaryColor` e obrigatoria;
* `primaryColor` e `secondaryColor`, quando informadas, devem usar formato hexadecimal `#RGB` ou `#RRGGBB`;
* `displayOrder` deve ser maior que zero;
* criacao gera novo `Guid` e retorna `201 Created` com `Location` apontando para o detalhe administrativo;
* `PATCH /status` ativa/desativa sem excluir registro;
* exclusao fisica e permitida apenas quando a raridade nao estiver vinculada a conquistas ou niveis;
* conflitos usam o middleware global de `ProblemDetails` via `InvalidOperationException` -> `409 Conflict`.

Banco/migration:

* Migration criada: `20260715192559_AddRarities`;
* cria a tabela `Rarities` com `Name`, `PrimaryColor`, `SecondaryColor`, `DisplayOrder`, `IsActive`, `CreatedAt` e `UpdatedAt`;
* `IsActive` possui default `true`;
* indices criados para `Name` unico e `DisplayOrder`;
* nenhuma tabela de Conquistas, Niveis ou Mobile foi alterada.

Decisao tecnica:

* O backend ainda nao possuia entidade/tabela de Raridades nem relacao persistida com Conquistas ou Niveis.
* Para respeitar a restricao de nao alterar Conquistas nem Niveis nesta tarefa, o CRUD introduz apenas o catalogo administrativo de Raridades.
* `achievementCount` e `levelCount` fazem parte do contrato e retornam `0` ate existir relacionamento real em uma sprint propria.
* Os metodos de bloqueio por vinculo ja existem no repository/service para preservar a regra quando os relacionamentos forem adicionados.

Testes adicionados:

* listagem administrativa com pesquisa, filtro, ordenacao e paginacao;
* detalhe existente;
* detalhe inexistente;
* criacao valida;
* duplicidade de nome;
* ordem invalida;
* cor invalida;
* atualizacao valida;
* duplicidade na atualizacao;
* ativacao/desativacao;
* exclusao permitida;
* bloqueio de exclusao por conquistas;
* bloqueio de exclusao por niveis.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 62 aprovados, 0 falhas e 0 ignorados.



## Seed Oficial de Raridades - 16/07/2026

Corre??o adicionada para garantir que a inicializa??o da Forge.Api tenha as raridades exigidas pelo cat?logo oficial de n?veis antes do `LevelDefinitionSeeder`.

Raridades oficiais com IDs est?veis:

* `11111111-1111-4111-8111-111111111101` - Incomum - Cinza.
* `11111111-1111-4111-8111-111111111102` - Comum - Verde.
* `11111111-1111-4111-8111-111111111103` - Raro - Azul.
* `11111111-1111-4111-8111-111111111104` - ?pico - Roxo.
* `11111111-1111-4111-8111-111111111105` - Lend?rio - Dourado.

Comportamento do seed:

* `RarityCatalog` define as cinco raridades oficiais.
* `OfficialRarityIds` centraliza os IDs oficiais.
* `RaritySeeder` insere somente raridades ausentes por ID e por nome.
* O seed nao duplica registros, nao remove raridades e nao sobrescreve alteracoes administrativas existentes.
* Em banco limpo, as cinco raridades sao criadas com IDs estaveis.
* Em banco ja existente com raridade de mesmo nome, o registro existente e preservado para evitar duplicidade e conflito de indice unico.
* `Program.cs` executa `SeedRaritiesAsync()` antes de `SeedLevelDefinitionsAsync()`.
* O CRUD administrativo de Raridades foi preservado.

Testes adicionados:

* catalogo oficial de raridades com cinco nomes, ordens e IDs estaveis;
* validacao de que o catalogo de niveis usa apenas nomes de raridades oficiais;
* validacao da ordem de inicializacao em `Program.cs`.

## Catalogo Real de Niveis e Progressao - 16/07/2026

Implementacao adicionada para separar o estado numerico do usuario do catalogo administravel de progressao.

Diferenca de conceitos:

* `UserProfile.Level` continua existindo e representa o nivel numerico atual do usuario.
* `LevelDefinition` representa o catalogo de definicoes de nivel, com nome, narrativa, XP minimo, ordem, raridade e campos futuros de imagem.
* `UserProfile.Level` nao foi transformado em CRUD.

Niveis oficiais criados com IDs estaveis:

* `22222222-2222-4222-8222-222222222221` - Esquecido - 0 XP - Raridade: Incomum.
* `22222222-2222-4222-8222-222222222222` - Forjado - 6.000 XP - Raridade: Comum.
* `22222222-2222-4222-8222-222222222223` - Guarda - 16.000 XP - Raridade: Raro.
* `22222222-2222-4222-8222-222222222224` - Sentinela - 30.000 XP - Raridade: Epico.
* `22222222-2222-4222-8222-222222222225` - Guardiao da Forja - 50.000 XP - Raridade: Lendario.

Modelo criado:

* Entidade `LevelDefinition`.
* Campos: `Id`, `Name`, `Description`, `MinimumXp`, `DisplayOrder`, `RarityId`, `BadgeImageUrl`, `GuardianImageUrl`, `IsActive`, `CreatedAt`, `UpdatedAt`.
* Relacionamento obrigatorio com `Rarity` via FK restrita.
* `BadgeImageUrl` e `GuardianImageUrl` permanecem nullable e sem upload nesta etapa.

Migration:

* Migration criada: `20260716122209_AddLevelDefinitions`.
* Cria a tabela `LevelDefinitions`.
* Cria indices unicos para `Name`, `DisplayOrder` e `MinimumXp`.
* Cria indice e FK para `RarityId`.
* Nao altera usuarios, XP historico, conquistas, mobile ou backoffice.

Seed oficial:

* `LevelDefinitionCatalog` define os cinco niveis oficiais.
* `OfficialLevelDefinitionIds` centraliza os IDs oficiais.
* `LevelDefinitionSeeder` insere somente niveis ausentes.
* O seed nao remove niveis, nao recria duplicados, nao altera IDs e preserva futuras imagens/alteracoes administrativas.
* Como Raridades ainda nao possuem IDs oficiais estaveis, o seed resolve as raridades pelo nome esperado.
* Se uma raridade necessaria nao existir, a inicializacao falha com mensagem clara e nao cria raridade duplicada.

Integracao com XP e gamificacao:

* Novo `LevelProgressionService` centraliza o calculo de progressao.
* Regra: nivel atual e o nivel ativo com maior `MinimumXp` menor ou igual ao `TotalXp`.
* A numeracao do usuario segue `DisplayOrder`.
* `XpService` usa o catalogo para atualizar `UserProfile.Level` apos novo XP.
* XP nunca e removido e historico de `XpTransaction` permanece intacto.
* Se nao houver catalogo ativo, ha fallback seguro para a formula anterior de 500 XP por nivel, evitando erro 500.
* Usuarios com 50.000 XP ou mais permanecem no nivel maximo oficial enquanto o catalogo oficial estiver ativo.

Endpoints publicos adicionados:

* `GET /api/levels` - retorna somente niveis ativos ordenados.
* `GET /api/levels/{id}` - retorna detalhe ativo ou `404`.

Endpoints administrativos adicionados:

* `GET /api/backoffice/levels`;
* `GET /api/backoffice/levels/{id}`;
* `POST /api/backoffice/levels`;
* `PUT /api/backoffice/levels/{id}`;
* `PATCH /api/backoffice/levels/{id}/status`;
* `DELETE /api/backoffice/levels/{id}`.

Regras administrativas:

* Nome obrigatorio e unico.
* Descricao obrigatoria.
* `MinimumXp` maior ou igual a zero e unico.
* `DisplayOrder` maior que zero e unico.
* `RarityId` obrigatorio e deve existir.
* Criacao retorna `201 Created`.
* Conflitos retornam `409 Conflict` pelo middleware global.
* O nivel inicial com `MinimumXp = 0` nao pode ser desativado.
* Os cinco niveis oficiais nao podem ser excluidos fisicamente; devem ser desativados quando necessario.
* Alterar limites afeta classificacoes futuras/dinamicas e nao recalcula nem remove XP.

Contratos mobile/dashboard atualizados de forma compativel:

* Campos numericos existentes `level`, `currentXp`/`xp` e `xpToNextLevel` foram preservados.
* Campos adicionais opcionais incluem `levelName`, `levelDescription`, `levelBadgeImageUrl`, `guardianImageUrl`, `rarity`, `nextLevelName`, `progressPercentage` e `isMaximumLevel`.

Testes adicionados:

* Catalogo oficial com cinco niveis, IDs estaveis, XP e raridades esperadas.
* Faixas de XP: 0, 5.999, 6.000, 15.999, 16.000, 29.999, 30.000, 49.999, 50.000 e acima de 50.000.
* Calculo de progresso dentro do nivel.
* Nivel maximo.
* Fallback sem catalogo.
* Criacao administrativa valida.
* Nome duplicado.
* Ordem duplicada.
* XP minimo duplicado.
* Raridade inexistente.
* Edicao.
* Ativacao/desativacao.
* Bloqueio de desativacao do nivel inicial.
* Protecao contra exclusao de niveis oficiais.

Validacao:

```txt
dotnet build Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-bin\ -p:UseSharedCompilation=false
dotnet test Forge.slnx -p:BaseOutputPath=C:\Forge\artifacts\codex-test-bin\ -p:UseSharedCompilation=false
```

Resultado:

* Build passou com 0 avisos e 0 erros.
* Testes passaram com 83 aprovados, 0 falhas e 0 ignorados.
* `UseSharedCompilation=false` foi usado para evitar locks locais do VBCSCompiler/MSBuild no ambiente.


## Corre??o de Carregamento de Raridade em LevelDefinitions - 16/07/2026

Corre??o aplicada no `LevelDefinitionRepository` para evitar `NullReferenceException` no m?dulo administrativo de n?veis.

Ajustes realizados:

* consultas p?blicas, administrativas e de progress?o que leem `LevelDefinition` agora carregam a rela??o obrigat?ria `Rarity` com `Include(level => level.Rarity)` e mant?m `AsNoTracking` nas leituras;
* mapeadores de `LevelDefinitionData` e `BackofficeLevelDefinitionData` possuem prote??o defensiva e retornam erro claro caso a raridade n?o esteja carregada ou n?o exista;
* listagem administrativa evita N+1 na contagem de usu?rios por n?vel carregando os XP dos usu?rios uma vez por consulta;
* contratos p?blicos, administrativos, XP, mobile e dashboard foram preservados.

Testes adicionados/ajustados:

* listagem administrativa retornando dados da raridade;
* detalhe administrativo retornando dados da raridade;
* mapeamento de n?vel com raridade carregada;
* erro defensivo claro quando a raridade n?o foi carregada.

## Proximos Passos Recomendados

1. Reiniciar a API apos builds quando houver processo `Forge.Api` rodando, para garantir que o Swagger use a DLL atual.
2. Confirmar migrations e seed em ambiente limpo quando a connection string mudar.
3. Concluir, em tarefa propria, os endpoints administrativos de midia de Exercicios caso o Backoffice precise executar upload/remocao real.
4. Expandir eventos de gamificacao para agua, sono, peso, streaks e metas semanais dedicadas.
5. Evoluir o Dashboard/agregadores mobile para incluir streaks, metas semanais, progresso parcial de conquistas e dados reais do Guardiao.
6. Revisar fakes de testes que ainda usam `NotImplementedException` em metodos nao exercitados.
7. Adicionar testes diretos para `XpService`, `AchievementService` e controllers de gamificacao.
8. Adicionar testes automatizados para regras de UserProfile, Exercise, Workout e WorkoutExercise.
9. Atualizar `TASKS.md`, pois varios itens da estrutura inicial e entidades ja existem, mas o checklist ainda esta desmarcado.
10. Manter `API_AUDIT.md` e `PROJECT_STATUS.md` sincronizados apos mudancas de contrato.

## Build Recente

Ultimos comandos executados durante a revisao do estado do projeto:

```txt
dotnet build Forge.slnx
```

Resultado:

* Build passou com `0 Erro(s)` e `0 Aviso(s)`.
* Warning `NU1903` do pacote `Microsoft.OpenApi` 2.3.0 foi corrigido.
* Migration `AddWorkoutStatus` criada e aplicada no banco local.
* Endpoint de Dashboard implementado em `GET /api/dashboard/{userProfileId}`.
* Endpoint mobile de Treinos implementado em `GET /api/mobile/users/{userProfileId}/workouts`.
* Endpoint mobile de Historico implementado em `GET /api/mobile/users/{userProfileId}/history`.
* CORS de desenvolvimento configurado para o Forge Mobile Web em `http://localhost:8082` e depois ampliado para o Forge Backoffice local em `http://localhost:5173` e `http://localhost:5174`.
* Coluna `Workouts.Status` adicionada como `nvarchar(50)` obrigatoria.
* Endpoints `POST /api/workouts/{id}/start` e `POST /api/workouts/{id}/cancel` adicionados.
* Agregadores mobile passaram a usar status persistido em vez de inferir conclusao por series.
* Agregadores mobile Home, Treinos e Historico passaram a usar duracao real persistida em vez de estimativa por quantidade de exercicios.
* Dashboard consome dados existentes de UserProfile, Workout, WaterIntake e SleepRecord.
* XP real e conquistas foram implementados para finalizacao de treino.
* Endpoints de XP e conquistas foram expostos.
* Observacao: na primeira tentativa, o build falhou por arquivos bloqueados pelo processo `Forge.Api` em execucao; apos encerrar o processo, o build passou.
* Para testes locais fora do Visual Studio, foi necessario desabilitar o provider de Windows EventLog no processo (`Logging__EventLog__LogLevel__Default=None`), pois o ambiente sem privilegios nao conseguia escrever no EventLog.

Ultimos testes executados:

```txt
dotnet test Forge.slnx
```

Resultado:

* Testes passaram com `13` aprovados, `0` falhas e `0` ignorados.
* Migration `AddWorkoutExecutionTime` criada e aplicada no banco local.
* Finalizacao de treino tornou-se transacional e idempotente.
* Adicionados testes para primeira finalizacao, segunda tentativa, rollback em falha e ausencia de duplicidade de XP/conquistas.
* Seed oficial de conquistas implementado com 11 conquistas iniciais e IDs estaveis.

## Revisao de Higiene - 14/07/2026

Itens executados:

* `API_AUDIT.md` atualizado para refletir o estado real da API apos status de treino, duracao real, finalizacao transacional/idempotente, XP/conquistas e seed oficial.
* `PROJECT_STATUS.md` atualizado para remover proximos passos ja concluidos e manter pendencias reais.
* Artefato temporario `preflight-options.txt` removido.
* Busca por mojibake/encoding em `API_AUDIT.md`, `PROJECT_STATUS.md`, `src` e `tests` sem ocorrencias.
* Ambiente limpo validado com banco temporario `Forge_Hygiene_20260714121149`:
  * migrations aplicadas do zero: `InitialCreate`, `AddWorkoutSetNotes`, `AddWorkoutStatus`, `AddWorkoutExecutionTime`;
  * API iniciada apontando para o banco temporario;
  * `GET /api/achievements` retornou 11 conquistas semeadas;
  * banco temporario removido apos a validacao.
* Validacao final:
  * `dotnet build Forge.slnx`: sucesso, 0 avisos, 0 erros;
  * `dotnet test Forge.slnx`: sucesso, 13 testes aprovados.
