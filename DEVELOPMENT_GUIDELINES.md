# DEVELOPMENT_GUIDELINES.md

# Forge - Diretrizes de Desenvolvimento

## Objetivo

Este documento define os padrões de desenvolvimento, arquitetura e organização do projeto Forge.

Todas as implementações devem seguir estas diretrizes.

---

# Arquitetura

O projeto deve seguir uma arquitetura em camadas.

```txt
Forge.Api
    ↓
Forge.Application
    ↓
Forge.Domain

Forge.Infrastructure
    ↓
Forge.Domain
```

## Responsabilidades

### Forge.Api

Responsável por:

* Controllers
* Middlewares
* Configuração da API
* Swagger
* Injeção de dependência

Não deve conter:

* Regras de negócio
* Consultas ao banco
* Acesso direto ao DbContext

---

### Forge.Application

Responsável por:

* Services
* Casos de uso
* Regras de negócio
* DTOs
* Interfaces

---

### Forge.Domain

Responsável por:

* Entidades
* Enums
* Constantes
* Regras centrais do domínio

O Domain não deve depender de nenhuma outra camada.

---

### Forge.Infrastructure

Responsável por:

* Entity Framework Core
* DbContext
* Repositories
* Configurações de banco
* Migrations

---

# Estrutura de Pastas

## Forge.Api

```txt
Controllers/
Middlewares/
Extensions/
Configurations/
```

## Forge.Application

```txt
Services/
Interfaces/
DTOs/
Mappings/
Validators/
```

## Forge.Domain

```txt
Entities/
Enums/
Constants/
```

## Forge.Infrastructure

```txt
Data/
Repositories/
Configurations/
Migrations/
```

---

# Controllers

Controllers devem ser extremamente simples.

Responsabilidades:

* Receber requisição
* Validar entrada
* Chamar Service
* Retornar resposta

Controllers não devem:

* Possuir regra de negócio
* Acessar DbContext
* Fazer cálculos
* Executar consultas complexas

Exemplo correto:

```txt
Controller
    ↓
Service
    ↓
Repository
```

---

# Services

Services são responsáveis por:

* Regras de negócio
* Processamento
* Validações de domínio
* Cálculos

Exemplos:

* Ganho de XP
* Progressão de nível
* Verificação de conquistas
* Cálculo de volume de treino

---

# Repositories

Repositories são responsáveis por:

* Consultar banco
* Persistir dados
* Atualizar registros
* Excluir registros

Repositories não devem conter regra de negócio.

---

# Entity Framework

Utilizar:

* Entity Framework Core
* Fluent API
* Migrations

Evitar:

* Configuração excessiva por Data Annotation
* Consultas desnecessárias

---

# DTOs

Utilizar DTOs para:

* Entrada de dados
* Saída de dados

Nunca retornar entidades diretamente pela API.

---

# Assincronismo

Utilizar async/await em operações que envolvam:

* Banco de dados
* Arquivos
* Recursos externos

Exemplo:

```csharp
public async Task<UserDto> GetByIdAsync(Guid id)
```

---

# Boas Práticas

## Nomes

Utilizar nomes claros e descritivos.

Bom:

```csharp
WorkoutService
CalculateWorkoutVolume
RegisterWorkoutAsync
```

Ruim:

```csharp
Service1
Calc
DoWork
```

---

## Métodos

* Devem possuir responsabilidade única.
* Devem ser pequenos e legíveis.
* Evitar métodos gigantes.

---

## Classes

* Uma responsabilidade por classe.
* Seguir princípio SRP.

---

## Código Duplicado

Evitar duplicação.

Sempre avaliar possibilidade de reutilização.

---

# Tratamento de Erros

Toda exceção deve ser tratada adequadamente.

Utilizar middleware global para tratamento de exceções.

Não utilizar try/catch desnecessariamente em Controllers.

---

# Banco de Dados

Utilizar:

* Guid como chave primária
* UTC para datas
* Índices quando necessário

Evitar:

* Queries sem filtros
* Carregamento excessivo de dados

---

# Qualidade de Código

Antes de concluir qualquer tarefa:

Verificar:

* Código compila
* Sem erros
* Sem warnings relevantes
* Sem código morto
* Sem código duplicado
* Sem regras de negócio em Controllers
* Sem acesso direto ao DbContext em Controllers

---

# Revisão Obrigatória

Sempre que uma funcionalidade for implementada:

1. Revisar o código.
2. Identificar possíveis bugs.
3. Identificar possíveis melhorias.
4. Verificar aderência à arquitetura.
5. Verificar aderência ao PROJECT_SPEC.md.
6. Verificar aderência ao DATABASE.md.

Nenhuma tarefa deve ser considerada concluída sem revisão.
