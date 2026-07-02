using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceSchool.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationsAndUpdateSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "location",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "room",
                table: "groups");

            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                table: "group_schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "room",
                table: "group_schedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_schedules_location_id",
                table: "group_schedules",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "fk_group_schedules_locations_location_id",
                table: "group_schedules",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_group_schedules_locations_location_id",
                table: "group_schedules");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropIndex(
                name: "ix_group_schedules_location_id",
                table: "group_schedules");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "group_schedules");

            migrationBuilder.DropColumn(
                name: "room",
                table: "group_schedules");

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "groups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "room",
                table: "groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
