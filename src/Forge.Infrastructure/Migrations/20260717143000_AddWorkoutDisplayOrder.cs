using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    public partial class AddWorkoutDisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Workouts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH OrderedWorkouts AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY UserProfileId
                            ORDER BY CreatedAt ASC, Id ASC
                        ) AS NextDisplayOrder
                    FROM Workouts
                )
                UPDATE Workouts
                SET DisplayOrder = OrderedWorkouts.NextDisplayOrder
                FROM Workouts
                INNER JOIN OrderedWorkouts ON Workouts.Id = OrderedWorkouts.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserProfileId_DisplayOrder",
                table: "Workouts",
                columns: new[] { "UserProfileId", "DisplayOrder" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserProfileId_DisplayOrder",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Workouts");
        }
    }
}
