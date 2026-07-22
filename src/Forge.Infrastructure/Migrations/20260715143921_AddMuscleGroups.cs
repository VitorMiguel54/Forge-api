using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MuscleGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleGroups", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "MuscleGroupId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkoutMuscleGroups",
                columns: table => new
                {
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutMuscleGroups", x => new { x.WorkoutId, x.MuscleGroupId });
                    table.ForeignKey(
                        name: "FK_WorkoutMuscleGroups_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutMuscleGroups_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MuscleGroups",
                columns: new[] { "Id", "Name", "DisplayName", "Icon", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "Chest", "Peito", "dumbbell", 10, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("22222222-2222-4222-8222-222222222222"), "Back", "Costas", "body", 20, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("33333333-3333-4333-8333-333333333333"), "Shoulders", "Ombro", "shield", 30, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("44444444-4444-4444-8444-444444444444"), "Biceps", "Biceps", "dumbbell", 40, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("55555555-5555-4555-8555-555555555555"), "Triceps", "Triceps", "dumbbell", 50, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("66666666-6666-4666-8666-666666666666"), "Forearms", "Antebraco", "grip", 60, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("77777777-7777-4777-8777-777777777777"), "Core", "Abdomen", "core", 70, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("88888888-8888-4888-8888-888888888888"), "LowerBack", "Lombar", "body", 80, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("99999999-9999-4999-8999-999999999999"), "Glutes", "Gluteo", "body", 90, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), "Quadriceps", "Quadriceps", "leg", 100, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "Hamstrings", "Posterior", "leg", 110, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), "Calves", "Panturrilha", "leg", 120, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("dddddddd-dddd-4ddd-8ddd-dddddddddddd"), "FullBody", "Corpo inteiro", "body", 130, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"), "Cardio", "Cardio", "heart", 140, true, DateTime.UtcNow, DateTime.UtcNow },
                    { new Guid("ffffffff-ffff-4fff-8fff-ffffffffffff"), "Other", "Outros", "circle", 150, true, DateTime.UtcNow, DateTime.UtcNow }
                });

            migrationBuilder.Sql(@"
UPDATE Exercises
SET MuscleGroupId = CASE MuscleGroup
    WHEN 'Chest' THEN '11111111-1111-4111-8111-111111111111'
    WHEN 'Back' THEN '22222222-2222-4222-8222-222222222222'
    WHEN 'Shoulders' THEN '33333333-3333-4333-8333-333333333333'
    WHEN 'Arms' THEN '44444444-4444-4444-8444-444444444444'
    WHEN 'Legs' THEN 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'
    WHEN 'Glutes' THEN '99999999-9999-4999-8999-999999999999'
    WHEN 'Core' THEN '77777777-7777-4777-8777-777777777777'
    WHEN 'FullBody' THEN 'dddddddd-dddd-4ddd-8ddd-dddddddddddd'
    WHEN 'Cardio' THEN 'eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee'
    ELSE 'ffffffff-ffff-4fff-8fff-ffffffffffff'
END
WHERE MuscleGroupId IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleGroups_DisplayOrder",
                table: "MuscleGroups",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleGroups_Name",
                table: "MuscleGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_MuscleGroupId",
                table: "Exercises",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutMuscleGroups_MuscleGroupId",
                table: "WorkoutMuscleGroups",
                column: "MuscleGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_MuscleGroups_MuscleGroupId",
                table: "Exercises",
                column: "MuscleGroupId",
                principalTable: "MuscleGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
INSERT INTO WorkoutMuscleGroups (WorkoutId, MuscleGroupId, CreatedAt)
SELECT DISTINCT we.WorkoutId, e.MuscleGroupId, SYSUTCDATETIME()
FROM WorkoutExercises we
INNER JOIN Exercises e ON e.Id = we.ExerciseId
WHERE e.MuscleGroupId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM WorkoutMuscleGroups wmg
      WHERE wmg.WorkoutId = we.WorkoutId
        AND wmg.MuscleGroupId = e.MuscleGroupId
  );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_MuscleGroups_MuscleGroupId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutMuscleGroups");

            migrationBuilder.DropTable(
                name: "MuscleGroups");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_MuscleGroupId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MuscleGroupId",
                table: "Exercises");
        }
    }
}
