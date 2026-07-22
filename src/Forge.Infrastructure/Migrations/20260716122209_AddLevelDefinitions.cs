using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevelDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MinimumXp = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RarityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GuardianImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LevelDefinitions_Rarities_RarityId",
                        column: x => x.RarityId,
                        principalTable: "Rarities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LevelDefinitions_DisplayOrder",
                table: "LevelDefinitions",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevelDefinitions_MinimumXp",
                table: "LevelDefinitions",
                column: "MinimumXp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevelDefinitions_Name",
                table: "LevelDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevelDefinitions_RarityId",
                table: "LevelDefinitions",
                column: "RarityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevelDefinitions");
        }
    }
}
