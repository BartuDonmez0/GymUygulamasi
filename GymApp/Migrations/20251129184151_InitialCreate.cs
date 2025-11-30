using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gym_centers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    working_hours = table.Column<string>(type: "text", nullable: false),
                    advertisement = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gym_centers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gym_center_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_activities_gym_centers_gym_center_id",
                        column: x => x.gym_center_id,
                        principalTable: "gym_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gym_center_photos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gym_center_id = table.Column<int>(type: "integer", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gym_center_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_gym_center_photos_gym_centers_gym_center_id",
                        column: x => x.gym_center_id,
                        principalTable: "gym_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    gym_center_id = table.Column<int>(type: "integer", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainers", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainers_gym_centers_gym_center_id",
                        column: x => x.gym_center_id,
                        principalTable: "gym_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    registration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "trainer_activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainer_id = table.Column<int>(type: "integer", nullable: false),
                    activity_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainer_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainer_activities_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trainer_activities_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainer_working_hours",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainer_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainer_working_hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainer_working_hours_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_recommendations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    recommendation_type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_recommendations", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_recommendations_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    trainer_id = table.Column<int>(type: "integer", nullable: false),
                    activity_id = table.Column<int>(type: "integer", nullable: false),
                    gym_center_id = table.Column<int>(type: "integer", nullable: false),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    appointment_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointments_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointments_gym_centers_gym_center_id",
                        column: x => x.gym_center_id,
                        principalTable: "gym_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointments_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointments_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    response = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_messages_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activities_gym_center_id",
                table: "activities",
                column: "gym_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_recommendations_member_id",
                table: "ai_recommendations",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_activity_id",
                table: "appointments",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_gym_center_id",
                table: "appointments",
                column: "gym_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_member_id",
                table: "appointments",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_trainer_id",
                table: "appointments",
                column: "trainer_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_member_id",
                table: "chat_messages",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_gym_center_photos_gym_center_id",
                table: "gym_center_photos",
                column: "gym_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_members_email",
                table: "members",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_user_id",
                table: "members",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainer_activities_activity_id",
                table: "trainer_activities",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_activities_trainer_id_activity_id",
                table: "trainer_activities",
                columns: new[] { "trainer_id", "activity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainer_working_hours_trainer_id",
                table: "trainer_working_hours",
                column: "trainer_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainers_email",
                table: "trainers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainers_gym_center_id",
                table: "trainers",
                column: "gym_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_recommendations");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "gym_center_photos");

            migrationBuilder.DropTable(
                name: "trainer_activities");

            migrationBuilder.DropTable(
                name: "trainer_working_hours");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "trainers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "gym_centers");
        }
    }
}
