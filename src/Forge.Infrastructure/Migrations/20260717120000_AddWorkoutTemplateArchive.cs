using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTemplateArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Workouts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateWorkoutId",
                table: "Workouts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_IsArchived",
                table: "Workouts",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_TemplateWorkoutId",
                table: "Workouts",
                column: "TemplateWorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Workouts_TemplateWorkoutId",
                table: "Workouts",
                column: "TemplateWorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Workouts_TemplateWorkoutId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_IsArchived",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_TemplateWorkoutId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "TemplateWorkoutId",
                table: "Workouts");
        }
    }
}
