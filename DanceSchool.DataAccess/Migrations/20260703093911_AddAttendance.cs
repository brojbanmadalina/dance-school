using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceSchool.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_present = table.Column<bool>(type: "boolean", nullable: false),
                    marked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendances", x => x.id);
                    table.ForeignKey(
                        name: "fk_attendances_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attendances_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_points", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_points_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attendances_group_id_student_id_session_date",
                table: "attendances",
                columns: new[] { "group_id", "student_id", "session_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attendances_student_id",
                table: "attendances",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_points_student_id",
                table: "student_points",
                column: "student_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.DropTable(
                name: "student_points");
        }
    }
}
