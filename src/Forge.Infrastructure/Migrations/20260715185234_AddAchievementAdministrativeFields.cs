using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementAdministrativeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Achievements");
        }
    }
}
