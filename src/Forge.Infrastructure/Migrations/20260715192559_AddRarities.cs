using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRarities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rarities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rarities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rarities_DisplayOrder",
                table: "Rarities",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Rarities_Name",
                table: "Rarities",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rarities");
        }
    }
}
