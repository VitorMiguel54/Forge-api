using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Workouts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.Sql(
                """
                UPDATE Workouts
                SET Status = 'Completed'
                WHERE EXISTS (
                    SELECT 1
                    FROM WorkoutExercises
                    WHERE WorkoutExercises.WorkoutId = Workouts.Id
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM WorkoutExercises
                    WHERE WorkoutExercises.WorkoutId = Workouts.Id
                    AND NOT EXISTS (
                        SELECT 1
                        FROM WorkoutSets
                        WHERE WorkoutSets.WorkoutExerciseId = WorkoutExercises.Id
                    )
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE Workouts
                SET Status = 'InProgress'
                WHERE Status = 'Draft'
                AND EXISTS (
                    SELECT 1
                    FROM WorkoutExercises
                    WHERE WorkoutExercises.WorkoutId = Workouts.Id
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_Status",
                table: "Workouts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_Status",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Workouts");
        }
    }
}
