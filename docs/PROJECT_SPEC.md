# PROJECT_SPEC.md

# Forge - Especificação do Produto

## Visão Geral

Forge é uma aplicação mobile-first para acompanhamento de treinos, evolução física e gamificação.

O sistema permite que o usuário registre treinos, exercícios, séries, repetições, cargas, peso corporal, consumo diário de água e horas de sono.

A aplicação não cria treinos automáticos, não recomenda exercícios e não substitui orientação profissional.

## Visão do Produto

O Forge foi concebido com foco principal em dispositivos móveis. A experiência principal do produto deve acontecer em um aplicativo mobile, usado durante ou logo após o treino.

Essa decisão existe porque o usuário normalmente precisa registrar dados no momento da atividade: séries, repetições, cargas, exercícios realizados e observações rápidas. O mesmo vale para registros simples de peso corporal, consumo de água e horas de sono.

O mobile proporciona uma experiência mais prática, direta e acessível para esse tipo de uso. Por isso, a API deve ser otimizada para atender o aplicativo mobile em primeiro lugar.

Uma versão Web poderá existir futuramente para consultas, relatórios, visualização de progresso e administração, mas não é a prioridade do projeto.

## Princípios de Desenvolvimento

* APIs devem ser pensadas primeiro para consumo mobile.
* Evitar múltiplas requisições quando uma única resposta puder atender uma tela.
* Priorizar respostas objetivas e rápidas.
* Pensar sempre na experiência do usuário mobile antes da versão Web.

## Objetivo do Produto

Ajudar o usuário a acompanhar sua evolução física através de estatísticas, histórico, hábitos saudáveis, XP, níveis e conquistas.

O foco principal do sistema é incentivar consistência de longo prazo.

## Funcionalidades do MVP

* Dashboard inicial
* Cadastro de exercícios
* Criação de treinos pelo próprio usuário
* Registro de séries
* Registro de repetições
* Registro de cargas
* Registro de peso corporal
* Registro de água consumida no dia
* Registro de horas de sono
* Histórico de treinos
* Sistema de XP
* Sistema de níveis
* Sistema de conquistas
* Comparação mensal
* Gráfico de evolução

## O que o sistema não faz

* Não cria treinos automaticamente
* Não recomenda cargas
* Não recomenda exercícios
* Não substitui personal trainer
* Não substitui médico
* Não substitui nutricionista
* Não oferece diagnóstico profissional

## Dashboard

A tela inicial deve exibir:

* Nível atual
* XP atual
* XP necessário para o próximo nível
* Peso inicial
* Peso atual
* Quantidade total de treinos realizados
* Sequência atual de dias treinando
* Meta diária de água
* Meta diária de sono
* Progresso da meta semanal de treinos
* Últimas conquistas desbloqueadas

## Regras de Treino

Conta como treino qualquer sessão registrada pelo usuário contendo pelo menos:

* Um exercício
* Uma série

O treino pode ser realizado em academia, casa ou qualquer outro local.

Um treino pode conter:

* Nome
* Data
* Exercícios
* Séries
* Repetições
* Carga
* Observações opcionais

## Sistema de XP

O usuário ganha XP ao registrar atividades relacionadas à sua evolução física.

### Fontes de XP Iniciais

#### Treinos

* Registrar um treino concluído: +50 XP
* Registrar uma série de exercício: +10 XP

#### Hidratação

* Cumprir meta diária de água: +20 XP

#### Sono

* Dormir entre 6h e 7h: +10 XP
* Dormir entre 7h e 8h: +20 XP
* Dormir mais de 8h: +30 XP

#### Peso Corporal

* Registrar peso corporal no dia: +10 XP

#### Metas

* Cumprir meta semanal de treinos: +100 XP

## Sistema de Níveis

O usuário sobe de nível ao atingir determinada quantidade de XP acumulado.

A fórmula exata de progressão será definida durante o desenvolvimento.

Objetivos da progressão:

* Recompensar consistência
* Evitar evolução excessivamente rápida
* Tornar níveis altos difíceis de alcançar
* Estimular uso contínuo da plataforma

## Registro de Água

O usuário poderá registrar a quantidade de água consumida durante o dia.

Exemplo:

* Meta diária: 2 litros
* Consumido: 2.5 litros
* Status: Meta cumprida

## Registro de Sono

O usuário poderá registrar quantas horas dormiu.

Exemplo:

* Data: 10/06/2026
* Horas dormidas: 8h15

## Metas

O sistema deve acompanhar:

### Hidratação

* Meta diária de água
* Sequência de dias cumprindo meta

### Sono

* Meta diária de sono
* Sequência de dias cumprindo meta

### Treino

* Meta semanal de treinos
* Quantidade de treinos realizados na semana
* Sequência de dias treinando

## Estatísticas

### Volume de uma série

Volume = carga × repetições

Exemplo:

20kg × 10 repetições = 200kg

### Volume de um exercício

Volume do exercício = soma do volume de todas as séries

### Volume de um treino

Volume do treino = soma do volume de todos os exercícios do treino

## Comparação Mensal

O sistema deve comparar o mês atual com o mês anterior utilizando:

* Quantidade de treinos
* Volume total movimentado
* Quantidade de séries registradas
* Carga máxima registrada por exercício
* Consumo médio de água
* Média de horas dormidas

## Sistema de Conquistas

As conquistas serão divididas em categorias.

### Treino

* Primeiro Treino
* Primeira Série
* Primeiro Exercício
* 10 Treinos
* 50 Treinos
* 100 Treinos

### Hidratação

* Hidratação em Dia
* 2 Litros em um Dia
* 7 Dias Hidratado
* 30 Dias Hidratado

### Sono

* Primeira Noite Bem Dormida
* Semana do Descanso
* Mestre do Sono
* Disciplina Noturna

### Evolução

* Evoluindo
* Nova Marca
* Mestre da Progressão

### Consistência

* Foco Total
* 7 Dias Seguidos
* 30 Dias Seguidos
* 100 Dias Seguidos

### Conquistas Secretas

O sistema poderá possuir conquistas ocultas que só serão reveladas após serem desbloqueadas.

## Tema Visual

A aplicação deve utilizar:

* Tema escuro
* Aparência moderna
* Dashboard intuitivo
* Visual focado em evolução e progresso
* Componentes responsivos

## Objetivo Técnico

Demonstrar conhecimento em:

* ASP.NET Core
* C#
* SQL Server
* Entity Framework Core
* APIs REST
* Arquitetura em camadas
* Boas práticas de desenvolvimento
* Dashboard e estatísticas
* Gamificação
* Integração frontend/backend
