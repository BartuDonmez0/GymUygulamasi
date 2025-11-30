using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkingHoursJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "working_hours_json",
                table: "trainers",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "working_hours_json",
                table: "gym_centers",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "gym_center_working_hours",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gym_center_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gym_center_working_hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_gym_center_working_hours_gym_centers_gym_center_id",
                        column: x => x.gym_center_id,
                        principalTable: "gym_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gym_center_working_hours_gym_center_id",
                table: "gym_center_working_hours",
                column: "gym_center_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gym_center_working_hours");

            migrationBuilder.DropColumn(
                name: "working_hours_json",
                table: "trainers");

            migrationBuilder.DropColumn(
                name: "working_hours_json",
                table: "gym_centers");
        }
    }
}
