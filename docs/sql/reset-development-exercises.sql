/*
    Forge.Api local development reset for workout test data and legacy exercises.

    This script is intentionally manual and destructive for local Development only.
    It is not executed by application startup, migrations, or seeders.

    Usage in SQLCMD mode:
      :setvar Environment "Development"
      :r docs/sql/reset-development-exercises.sql

    Scope:
      - removes WorkoutSets;
      - removes WorkoutExercises;
      - removes WorkoutMuscleGroups;
      - removes Workouts;
      - removes current Exercises so the official idempotent seed can recreate the catalog.

    Preserved:
      - UserProfiles;
      - MuscleGroups;
      - Rarities;
      - Achievements and UserAchievements;
      - LevelDefinitions;
      - XpTransactions and other non-workout catalogs/configuration.
*/

:setvar Environment "Development"

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF N'$(Environment)' <> N'Development'
BEGIN
    THROW 51000, 'Aborted: reset-development-exercises.sql must only run with Environment=Development.', 1;
END;

BEGIN TRANSACTION;

DECLARE @DeletedWorkoutSets int = 0;
DECLARE @DeletedWorkoutExercises int = 0;
DECLARE @DeletedWorkoutMuscleGroups int = 0;
DECLARE @DeletedWorkouts int = 0;
DECLARE @DeletedExercises int = 0;

DELETE workoutSet
FROM dbo.WorkoutSets AS workoutSet
INNER JOIN dbo.WorkoutExercises AS workoutExercise
    ON workoutExercise.Id = workoutSet.WorkoutExerciseId;
SET @DeletedWorkoutSets = @@ROWCOUNT;

DELETE FROM dbo.WorkoutMuscleGroups;
SET @DeletedWorkoutMuscleGroups = @@ROWCOUNT;

DELETE FROM dbo.WorkoutExercises;
SET @DeletedWorkoutExercises = @@ROWCOUNT;

DELETE FROM dbo.Workouts;
SET @DeletedWorkouts = @@ROWCOUNT;

DELETE FROM dbo.Exercises;
SET @DeletedExercises = @@ROWCOUNT;

COMMIT TRANSACTION;

SELECT
    @DeletedWorkoutSets AS DeletedWorkoutSets,
    @DeletedWorkoutExercises AS DeletedWorkoutExercises,
    @DeletedWorkoutMuscleGroups AS DeletedWorkoutMuscleGroups,
    @DeletedWorkouts AS DeletedWorkouts,
    @DeletedExercises AS DeletedExercises;
