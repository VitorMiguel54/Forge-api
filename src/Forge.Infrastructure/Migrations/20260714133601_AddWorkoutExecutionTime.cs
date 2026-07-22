using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutExecutionTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "Workouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Workouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Workouts
                SET StartedAt = WorkoutDate
                WHERE StartedAt IS NULL
                    AND Status IN ('InProgress', 'Completed', 'Cancelled')
                """);

            migrationBuilder.Sql(
                """
                UPDATE Workouts
                SET FinishedAt =
                    CASE
                        WHEN UpdatedAt IS NOT NULL AND UpdatedAt > StartedAt THEN UpdatedAt
                        ELSE StartedAt
                    END
                WHERE FinishedAt IS NULL
                    AND StartedAt IS NOT NULL
                    AND Status IN ('Completed', 'Cancelled')
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_FinishedAt",
                table: "Workouts",
                column: "FinishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_StartedAt",
                table: "Workouts",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_FinishedAt",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_StartedAt",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Workouts");
        }
    }
}
