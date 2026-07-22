/*
    Forge local Development cleanup

    Purpose:
    - Remove only workout-related data from the local Forge development database.
    - Preserve users, exercises, muscle groups, levels, rarities, achievements,
      XP catalogs and system configuration.

    Expected database:
    - Forge local development database only.

    Cleanup order:
    1. WorkoutSets
    2. WorkoutExercises
    3. WorkoutMuscleGroups
    4. Workouts.TemplateWorkoutId self references
    5. Workouts
*/

SET XACT_ABORT ON;

BEGIN TRY
    IF DB_NAME() <> N'Forge'
    BEGIN
        THROW 51000, 'This script must be executed only against the local Forge development database.', 1;
    END;

    DECLARE
        @WorkoutSetsDeleted int = 0,
        @WorkoutExercisesDeleted int = 0,
        @WorkoutMuscleGroupsDeleted int = 0,
        @TemplateWorkoutReferencesCleared int = 0,
        @WorkoutsDeleted int = 0;

    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.WorkoutSets', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.WorkoutSets;
        SET @WorkoutSetsDeleted = @@ROWCOUNT;
    END;

    IF OBJECT_ID(N'dbo.WorkoutExercises', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.WorkoutExercises;
        SET @WorkoutExercisesDeleted = @@ROWCOUNT;
    END;

    IF OBJECT_ID(N'dbo.WorkoutMuscleGroups', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.WorkoutMuscleGroups;
        SET @WorkoutMuscleGroupsDeleted = @@ROWCOUNT;
    END;

    IF COL_LENGTH(N'dbo.Workouts', N'TemplateWorkoutId') IS NOT NULL
    BEGIN
        UPDATE dbo.Workouts
        SET TemplateWorkoutId = NULL
        WHERE TemplateWorkoutId IS NOT NULL;

        SET @TemplateWorkoutReferencesCleared = @@ROWCOUNT;
    END;

    IF OBJECT_ID(N'dbo.Workouts', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.Workouts;
        SET @WorkoutsDeleted = @@ROWCOUNT;
    END;

    COMMIT TRANSACTION;

    SELECT N'WorkoutSets' AS TableName, @WorkoutSetsDeleted AS RecordsRemoved
    UNION ALL SELECT N'WorkoutExercises', @WorkoutExercisesDeleted
    UNION ALL SELECT N'WorkoutMuscleGroups', @WorkoutMuscleGroupsDeleted
    UNION ALL SELECT N'Workouts.TemplateWorkoutId', @TemplateWorkoutReferencesCleared
    UNION ALL SELECT N'Workouts', @WorkoutsDeleted;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    SELECT
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() AS ErrorState,
        ERROR_LINE() AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage;

    THROW;
END CATCH;
