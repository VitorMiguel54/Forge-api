using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementAndLevelImageUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuardianImageStorageKey",
                table: "LevelDefinitions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeImageStorageKey",
                table: "Achievements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeImageUrl",
                table: "Achievements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardianImageStorageKey",
                table: "LevelDefinitions");

            migrationBuilder.DropColumn(
                name: "BadgeImageStorageKey",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "BadgeImageUrl",
                table: "Achievements");
        }
    }
}
