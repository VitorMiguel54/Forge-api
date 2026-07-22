using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackofficeExerciseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Exercises",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Exercises",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "Exercises",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GifUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Exercises",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_DisplayOrder",
                table: "Exercises",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_IsActive",
                table: "Exercises",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exercises_DisplayOrder",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_IsActive",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "GifUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Exercises");
        }
    }
}
